# OrbitAOS.V6 - Containerization Ready

## Overview

OrbitAOS.V6 is an ASP.NET Core 6.0 web application that has been prepared for containerized deployment on Azure Kubernetes Service (AKS) with proper configuration management following 12-factor app principles.

## Quick Start

### Run Locally with Docker Compose
```bash
docker-compose up --build
```
Access at: http://localhost:8080

### Build Docker Image
```bash
docker build -t orbitaos:latest .
```

### Deploy to Kubernetes
```bash
kubectl apply -f k8s/
```

## Key Features

✅ **Runtime Configuration Injection**
- Environment-specific values injected at runtime
- No hardcoded configuration in code
- Single container image for all environments

✅ **Azure Key Vault Integration**
- Secrets managed via Azure Key Vault CSI Driver
- Sensitive data never in code or ConfigMaps
- Automatic secret rotation support

✅ **Health Check Endpoint**
- `/health` endpoint for Kubernetes probes
- Liveness and readiness checks configured
- Automatic pod restart on failure

✅ **Security Best Practices**
- Non-root user execution
- Minimal base image (aspnet:6.0)
- No secrets in source control
- TLS/HTTPS enabled

✅ **12-Factor App Compliant**
- Configuration via environment variables
- Stateless processes
- Horizontal scaling ready
- Dev/prod parity

## Project Structure

```
OrbitAOS.V6/
├── Controllers/          # MVC Controllers
├── Data/                 # Entity Framework DbContext
├── Models/               # Data models
├── Views/                # Razor views
│   └── Shared/
│       └── _EnvironmentConfig.cshtml  # Config injection
├── wwwroot/
│   ├── js/
│   │   ├── config.js     # Runtime configuration module
│   │   └── site.js
│   └── lib/              # Third-party libraries (Bootstrap, jQuery)
├── k8s/                  # Kubernetes manifests
│   ├── configmap.yaml    # Environment-specific config
│   ├── secrets.yaml      # Azure Key Vault integration
│   └── deployment.yaml   # Deployment, Service, Ingress
├── appsettings.json      # Default configuration
├── Program.cs            # Application entry point
├── Dockerfile            # Multi-stage Docker build
├── docker-compose.yml    # Local development
├── CONTAINERIZATION_GUIDE.md  # Detailed containerization guide
├── DEPLOYMENT_GUIDE.md        # Step-by-step deployment
└── BLOCKER_FIX_SUMMARY.md     # Blocker resolution details
```

## Configuration Management

### Environment Variables

Configuration is loaded from environment variables in this order:
1. `appsettings.json` (defaults)
2. Environment variables (from Kubernetes ConfigMaps)
3. Azure Key Vault secrets (via CSI Driver)

### JavaScript Configuration

Client-side JavaScript accesses configuration via `window.AppConfig`:
```javascript
// Example usage
fetch(window.AppConfig.apiBaseUrl + '/endpoint')
  .then(response => response.json())
  .then(data => console.log(data));
```

### Kubernetes ConfigMaps

Three ConfigMaps for different environments:
- `orbitaos-config-dev` - Development
- `orbitaos-config-staging` - Staging
- `orbitaos-config` - Production

## Documentation

- **[CONTAINERIZATION_GUIDE.md](CONTAINERIZATION_GUIDE.md)** - Comprehensive guide to containerization approach, false positive analysis, and configuration injection
- **[DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md)** - Step-by-step deployment instructions for Docker, ACR, and AKS
- **[BLOCKER_FIX_SUMMARY.md](BLOCKER_FIX_SUMMARY.md)** - Summary of containerization blocker resolution

## Technology Stack

- **Framework:** ASP.NET Core 6.0
- **Database:** SQL Server (via Entity Framework Core)
- **Authentication:** ASP.NET Core Identity
- **Frontend:** Bootstrap 5.1.0, jQuery
- **Container:** Docker
- **Orchestration:** Kubernetes (AKS)
- **Secrets:** Azure Key Vault CSI Driver
- **Configuration:** Kubernetes ConfigMaps

## Health Check

The application exposes a health check endpoint at `/health` for Kubernetes:
- **Liveness Probe:** Checks if application is running
- **Readiness Probe:** Checks if application is ready for traffic

## Security

- ✅ Non-root user in container
- ✅ Secrets in Azure Key Vault
- ✅ Managed identities for authentication
- ✅ HTTPS/TLS enabled
- ✅ No secrets in source control
- ✅ Minimal attack surface

## Deployment Environments

### Development
```bash
kubectl apply -f k8s/configmap.yaml  # Use orbitaos-config-dev
kubectl apply -f k8s/deployment.yaml
```

### Staging
```bash
kubectl apply -f k8s/configmap.yaml  # Use orbitaos-config-staging
kubectl apply -f k8s/deployment.yaml
```

### Production
```bash
kubectl apply -f k8s/configmap.yaml  # Use orbitaos-config
kubectl apply -f k8s/deployment.yaml
```

## Monitoring

### View Logs
```bash
kubectl logs -f -l app=orbitaos
```

### Check Health
```bash
kubectl port-forward svc/orbitaos-service 8080:80
curl http://localhost:8080/health
```

### Scale Application
```bash
kubectl scale deployment orbitaos-deployment --replicas=5
```

## CI/CD Integration

The application is ready for CI/CD pipelines:
- Dockerfile for automated builds
- Kubernetes manifests for automated deployments
- Health checks for deployment verification
- Rolling update support

## Support

For detailed information, see:
- [CONTAINERIZATION_GUIDE.md](CONTAINERIZATION_GUIDE.md) - Configuration and architecture
- [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) - Deployment procedures
- [BLOCKER_FIX_SUMMARY.md](BLOCKER_FIX_SUMMARY.md) - Blocker resolution

## License

Copyright © 2024 OrbitAOS.V6
