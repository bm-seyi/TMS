# TMS (Transport Management System) - Backend

**TMS** is a distributed **.**NET** backend system** designed to support the ****TMS** .**NET** **MAUI** application**. It is built as a collection of microservices and infrastructure components orchestrated locally using **.**NET** Aspire**.

The system is designed to scale and run in production using **Azure Container Apps**.

---

## 🚀 Tech Stack

### Core

- **.**NET** 10**
- ****ASP**.**NET** Core Web **API****
- **SignalR**
- ****YARP** Reverse Proxy (Gateway)**
- **Worker Services**

### Infrastructure

- ****SQL** Server**
- **Kafka**
- **Debezium (**CDC** / Change Data Capture)**
- **Redis**
- **HashiCorp Vault**
- **Docker**
- **.**NET** Aspire**

---

## 🧩 System Architecture

```mermaid
flowchart TD
    **MAUI**[.**NET** **MAUI** Client App] -->|**HTTPS**| GW[**TMS**.Gateway (**YARP**)]
    
    GW --> **API**[**TMS**.**API**]
    GW --> **SIG**[**TMS**.SignalR]

    **API** --> DB[(**SQL** Server)]
    **API** --> **REDIS**[(Redis Cache)]
    **API** --> **VAULT**[(Vault Secrets)]

    **SIG** --> **REDIS**
    **SIG** --> **VAULT**

    **WORKER**[**TMS**.WorkerService] --> DB
    **WORKER** --> **KAFKA**[(Kafka)]
    **WORKER** --> **VAULT**

    DB -->|**CDC**| **DEBEZ**[Debezium Connector]
    **DEBEZ** --> **KAFKA**

    **KAFKA** --> **WORKER**
    **KAFKA** --> **API**
````

---

## 📦 Repository Overview

This repository contains multiple services and an Aspire AppHost for orchestration.

### Main Projects

| Project               | Description                                                        |
| --------------------- | ------------------------------------------------------------------ |
| `TMS.API`             | Core backend API containing business logic and database operations |
| `TMS.Gateway`         | Reverse proxy gateway (YARP) routing requests to internal services |
| `TMS.SignalR`         | Real-time communication hub for MAUI clients                       |
| `TMS.WorkerService`   | Background jobs and event-driven processing                        |
| `TMS.Aspire.AppHost`  | Aspire AppHost that orchestrates all services locally              |
| `TMS.Aspire.ServiceDefaults` | Shared defaults for OpenTelemetry                    |

---

## 🏁 Running Locally (Recommended)

### Prerequisites

Install the following:

- **.**NET** 10 **SDK****
- **Docker Desktop**
- **Visual Studio Code** (recommended)
- ****SQL** Server Management Studio** (optional, for viewing DB)

---


### Run the Full System

The intended way to run the system locally is through the Aspire AppHost.

This will start:

- All internal .**NET** services
- **SQL** Server container
- Kafka + Debezium containers
- Redis container
- Vault container
- Aspire dashboard

---

## 🖥 Aspire Dashboard

When running via Aspire, the dashboard will expose:

- service health
- logs
- container status
- dependency wiring
- environment variables

The dashboard **URL** is printed in the console output when the AppHost starts.

---

## 🌐 Gateway & Routing

All external traffic should go through:

- `**TMS**.Gateway`

The gateway is responsible for routing requests to:

- `**TMS**.**API**`
- `**TMS**.SignalR`

This ensures the **MAUI** client only needs to know one backend endpoint.

---

## 🔌 MAUI Application Integration

The **.**NET** **MAUI** client** should communicate with the backend through the Gateway.

Example base **URL**:

[https://localhost:7258](https://localhost:7258)

All **API** requests and SignalR connections should target this address.

---

## 📡 SignalR

The system includes a dedicated SignalR service (`**TMS**.SignalR`) for:

- real-time map updates


Redis is used as the SignalR backplane when scaling horizontally.

---

## 🗄 Database (SQL Server)

The backend persists its primary data in **SQL** Server.

**SQL** Server runs as a container during local development when started through Aspire.

---

## 🟠 Kafka + Debezium

This project includes Kafka and Debezium to support **event-driven architecture**.

### Debezium Role

Debezium listens for database changes (**CDC**) in **SQL** Server and publishes those changes into Kafka topics.

### Kafka Role

Kafka is used to distribute events between services such as:

- WorkerService

---

## 🧠 Redis

Redis is used for:

- caching
- SignalR backplane support

---

## 🔐 Vault (Secrets Management)

HashiCorp Vault is included for managing secrets such as:

- connection strings
- **API** keys
- internal service credentials

During local development, Vault runs as a container and is wired automatically by Aspire.

> **Note:** Do not commit secrets to source control. > Use Vault and environment variables instead.

---

## ⚙ Configuration

Most service configuration is handled through:

- Aspire dependency injection + service discovery
- environment variables
- Vault secrets

---

## 🧪 Testing

To run tests:

```dotnet test ```

---

## 🐳 Docker Notes

The system depends heavily on containerized infrastructure services. Running locally without Docker is **not supported**.

Ensure Docker Desktop is running before launching Aspire.

---

## ☁️ Deployment (Production)

The production target for this system is:

✅ **Azure Container Apps**

Recommended production approach:

- containerize each service (`**TMS**.**API**`, `**TMS**.SignalR`, `**TMS**.WorkerService`, `**TMS**.Gateway`)
- deploy infrastructure using managed services where possible:

    * Azure **SQL**
    * Azure Cache for Redis
    * Azure Event Hubs (Kafka-compatible) or Kafka cluster
    * Key Vault (instead of Vault)

> Aspire is primarily for local orchestration, but its configuration model aligns well with container deployment.

---

## 📌 Key Notes / Developer Workflow

- Always run the system via **Aspire AppHost**
- Always access backend through ****TMS**.Gateway**
- **MAUI** app should never directly call internal services
- Infrastructure services are started automatically via Aspire

---


## 🛠 Troubleshooting

### Docker not running

If Aspire fails to start dependencies:

- ensure Docker Desktop is running
- ensure **WSL2** is enabled (Windows)

### Aspire workload missing

If AppHost fails to run:

```bash dotnet workload install aspire ```

### Port conflicts

If startup fails due to ports already being used:

- stop conflicting services
- restart the Aspire AppHost

---
