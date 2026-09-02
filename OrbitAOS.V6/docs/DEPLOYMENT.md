# Deployment Guide for OrbitAOS.V6 on Azure AKS

## Overview
This guide provides instructions for containerizing and deploying the OrbitAOS.V6 .NET 6 application to Azure Kubernetes Service (AKS).

## Prerequisites
- Docker Desktop installed
- Azure CLI installed
- kubectl installed
- An active Azure Subscription
- An Azure Container Registry (ACR)

## Local Development Setup
To run the application locally using Docker Compose:
1. Create a `.env` file or set environment variables for `DATABASE_URL` and `API_KEY`.
2. Run:
   ```bash
   docker-compose up --build
   ```
3. Access the application at `http://localhost:80`.

## Build and Push Process
Use the provided scripts to build the Docker image and push it to your registry.

### Linux/macOS
```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

### Windows
```cmd
scripts\build-push.bat
```

## Azure AKS Deployment

### 1. AKS Cluster Setup
Ensure your AKS cluster is running and you have the necessary permissions.

### 2. Deployment Execution
Run the deployment script to configure the cluster and apply manifests.

#### Linux/macOS
```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

#### Windows
```cmd
scripts\deploy-image.bat
```

## Kubernetes Manifests Description
- `namespace.yaml`: Creates a dedicated namespace `orbitaos-v6` for the application.
- `deployment.yaml`: Defines the pod specification, including 2 replicas, resource limits (500m CPU, 1Gi RAM), and health probes.
- `service.yaml`: Exposes the application internally within the cluster via ClusterIP.
- `ingress.yaml`: Configures the Azure Application Gateway to route external traffic to the service.

## Configuration Management
The application uses environment variables for configuration:
- `ConnectionStrings__DefaultConnection`: SQL Server connection string.
- `AppSettings__ApiKey`: API key for external services.
- `ASPNETCORE_ENVIRONMENT`: Set to `Production` for deployed environments.

## Troubleshooting
- **Pod CrashLoopBackOff**: Check logs using `kubectl logs -l app=orbitaos-v6 -n orbitaos-v6`.
- **Health Check Failures**: Ensure the database is reachable from the AKS cluster.
- **Ingress Issues**: Verify the Azure Application Gateway is correctly configured and the DNS host matches.

## .NET Specific Notes
- **Runtime**: The application uses the `mcr.microsoft.com/dotnet/runtime:6.0-alpine` image for a minimal footprint.
- **Health Checks**: The application exposes a `/health` endpoint used by Kubernetes for liveness and readiness probes.
- **GC Tuning**: For high-load scenarios, consider setting `COMPlus_gcServer=1` in the deployment environment variables.
