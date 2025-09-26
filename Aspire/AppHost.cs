IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ParameterResource> devServerPassword = builder.AddParameter("DevServerPassword", secret: true);

IResourceBuilder<SqlServerServerResource> devServer = builder.AddSqlServer("DevServer", devServerPassword, 1433)
    .WithLifetime(ContainerLifetime.Session)
    .WithImage("mssql/server", "2022-latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("TZ", "Europe/London")
    .WithDataVolume("mssql_data");

IResourceBuilder<ContainerResource> zookeeper = builder.AddContainer("zookeeper", "confluentinc/cp-zookeeper", "7.4.0")
    .WithEnvironment("ZOOKEEPER_CLIENT_PORT", "2181")
    .WithEnvironment("ZOOKEEPER_TICK_TIME", "2000")
    .WithEndpoint("zk", x =>
    {
        x.TargetPort = 2181;
        x.Port = 2181;
    });

IResourceBuilder<ContainerResource> kafka = builder.AddContainer("kafka", "confluentinc/cp-kafka", "7.4.0")
    .WithEnvironment("KAFKA_BROKER_ID", "1")
    .WithEnvironment("KAFKA_ZOOKEEPER_CONNECT", "zookeeper:2181")
    .WithEnvironment("KAFKA_LISTENER_SECURITY_PROTOCOL_MAP", "PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT")
    .WithEnvironment("KAFKA_ADVERTISED_LISTENERS", "PLAINTEXT://kafka:29092,PLAINTEXT_HOST://localhost:9092")
    .WithEnvironment("KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR", "1")
    .WithEndpoint("kafka", x =>
    {
        x.TargetPort = 9092;
        x.Port = 9092;
    })
    .WaitFor(zookeeper);

builder.AddContainer("debezium", "quay.io/debezium/connect", "latest")
    .WithEnvironment("BOOTSTRAP_SERVERS", "kafka:29092")
    .WithEnvironment("GROUP_ID", "1")
    .WithEnvironment("CONFIG_STORAGE_TOPIC", "my_connect_configs")
    .WithEnvironment("OFFSET_STORAGE_TOPIC", "my_connect_offsets")
    .WithEnvironment("STATUS_STORAGE_TOPIC", "my_connect_statuses")
    .WaitFor(kafka)  
    .WaitFor(zookeeper)
    .WaitFor(devServer);

DistributedApplication distributedApplication = builder.Build();

await distributedApplication.RunAsync();
