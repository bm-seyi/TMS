# Tram Management System - API

## Description

This project is an ASP.NET application that utilizes SignalR for real-time communication with a Transportation Management System (TMS) application built using .NET MAUI. SignalR is used to provide seamless and low-latency data exchange between the backend ASP.NET server and the TMS mobile app, enabling features like real-time updates, notifications, and bidirectional communication.

## Building Docker Images
Images can be built using

```PowerShell
docker build -t <name>:<tag> -f <relative-path-to-Dockerfile> .
```

#### Example:
```PowerShell
docker build -t tms-workerservice:latest -f Docker/Dockerfile.TMS-WorkerService .
```

#### Notes:
- `<name>`: The name of your Docker image (e.g., tms-workerservice)
- `<tag>`: The image version tag (e.g., latest)
- `<relative-path-to-Dockerfile>`: Path to your Dockerfile relative to the project root

## License

This work is licensed under a Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International License.