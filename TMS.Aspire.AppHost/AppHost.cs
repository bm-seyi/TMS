using Aspire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Projects;
using TMS.Aspire.AppHost.Services;


IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

IResourceBuilder<SqlServerServerResource> devServer = builder.AddSqlServer("DevServer", builder.AddParameter("DevServerPassword", secret: true), 1433)
    .WithLifetime(ContainerLifetime.Session)
    .WithImage("mssql/server", "2022-latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("TZ", "Europe/London")
    .WithDataVolume("mssql_data");

IResourceBuilder<SqlServerDatabaseResource> tmsDatabase = devServer.AddDatabase("TMS-Database", "TMS");

IResourceBuilder<ContainerResource> debezium = builder.AddContainer("debezium", "quay.io/debezium/connect", "latest")
    .WithLifetime(ContainerLifetime.Session)
    .WithEnvironment("BOOTSTRAP_SERVERS", "kafka:9092")
    .WithEnvironment("GROUP_ID", "1")
    .WithEnvironment("CONFIG_STORAGE_TOPIC", "my_connect_configs")
    .WithEnvironment("OFFSET_STORAGE_TOPIC", "my_connect_offsets")
    .WithEnvironment("STATUS_STORAGE_TOPIC", "my_connect_statuses")
    .WaitFor(devServer)
    .WithHttpEndpoint(port: 8083, targetPort: 8083);

IResourceBuilder<ContainerResource> kafka = builder.AddContainer("kafka", "confluentinc/cp-kafka", "latest")
    .WithLifetime(ContainerLifetime.Session)
    .WithEnvironment("KAFKA_NODE_ID", "1")
    .WithEnvironment("KAFKA_PROCESS_ROLES", "broker,controller")
    .WithEnvironment("KAFKA_LISTENERS", "PLAINTEXT://:9092,CONTROLLER://:9093,PLAINTEXT_HOST://:29092")
    .WithEnvironment("KAFKA_ADVERTISED_LISTENERS", "PLAINTEXT://kafka:9092,PLAINTEXT_HOST://localhost:29092")
    .WithEnvironment("KAFKA_LISTENER_SECURITY_PROTOCOL_MAP", "PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT,CONTROLLER:PLAINTEXT")
    .WithEnvironment("KAFKA_CONTROLLER_LISTENER_NAMES", "CONTROLLER")
    .WithEnvironment("KAFKA_CONTROLLER_QUORUM_VOTERS", "1@kafka:9093")
    .WithEnvironment("KAFKA_INTER_BROKER_LISTENER_NAME", "PLAINTEXT")
    .WithEnvironment("CLUSTER_ID", "bmcwR01GorP3gKszzXQFQA")
    .WithEnvironment("KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR", "1")
    .WithHttpEndpoint(port: 29092, targetPort: 29092);

IResourceBuilder<RedisResource> redis = builder.AddRedis("redis-backplane", 6379, builder.AddParameter("redisPassword", secret: true))
    .WithImageTag("8.2.3")
    .WithLifetime(ContainerLifetime.Session)
    .WithRedisInsight();

IResourceBuilder<ContainerResource> vault = builder.AddContainer("vault", "hashicorp/vault", "latest")
    .WithLifetime(ContainerLifetime.Session)
    .WithEnvironment("VAULT_DEV_ROOT_TOKEN_ID", "root")
    .WithEnvironment("VAULT_DEV_LISTEN_ADDRESS", "0.0.0.0:8200")
    .WithArgs("server", "-dev")
    .WithHttpEndpoint(port: 8200, targetPort: 8200)
    .OnResourceReady(async (resource, evt, ct) =>
    {
        string password = builder.Configuration.GetValue<string>("Parameters:ArcGisVaultPassword") ?? throw new InvalidOperationException("ArcGisVaultPassword is not configured.");
        await VaultService.WriteArcGisPasswordAsync(resource, password, ct);
    });

IResourceBuilder<ProjectResource> signalR = builder.AddProject<TMS_SignalR>("SignalR")
    .WaitFor(tmsDatabase)
    .WithReference(tmsDatabase, "DefaultConnection")
    .WaitFor(redis);

builder.AddProject<TMS_WorkerService>("WorkerService")
    .WaitFor(signalR)
    .WaitFor(debezium)
    .WaitFor(kafka);

IResourceBuilder<ProjectResource> tmsApi = builder.AddProject<TMS_API>("TMS-API")
    .WaitFor(tmsDatabase)
    .WithReference(tmsDatabase, "DefaultConnection");

builder.AddProject<TMS_Gateway>("TMS-Gateway")
    .WaitFor(tmsApi);

DistributedApplication distributedApplication = builder.Build();

await distributedApplication.RunAsync();
