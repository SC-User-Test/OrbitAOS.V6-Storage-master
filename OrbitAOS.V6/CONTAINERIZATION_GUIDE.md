# OrbitAOS.V6 - Containerization Configuration Guide

## Overview

This document explains the containerization approach for OrbitAOS.V6, specifically addressing the elimination of hardcoded environment values in favor of runtime configuration injection using Azure Key Vault CSI Driver and Kubernetes ConfigMaps.

## Rule: cz-js-1057 - Hardcoded Environment Values in jQuery Code

### Analysis of Flagged Files

The containerization scanner flagged the following Bootstrap library files:
- `wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.js`
- `wwwroot/lib/bootstrap/dist/js/bootstrap.esm.js`
- `wwwroot/lib/bootstrap/dist/js/bootstrap.js`

### Important Finding: False Positives

**These are FALSE POSITIVES.** The flagged lines contain Bootstrap framework constants (e.g., `ARROW_LEFT_KEY`, `ARROW_RIGHT_KEY`, event names, DOM selectors), NOT environment-specific configuration values.

**Examples of flagged constants:**
```javascript
const ARROW_LEFT_KEY = 'ArrowLeft';
const ARROW_RIGHT_KEY = 'ArrowRight';
const DATA_KEY$a = 'bs.carousel';
const EVENT_KEY$a = '.bs.carousel';
```

**Why these are NOT containerization blockers:**
1. These are JavaScript framework constants that are identical across all environments
2. They are NOT API endpoints, database URLs, API keys, or environment-specific configuration
3. Modifying third-party library files (Bootstrap) is an anti-pattern
4. These values do not change between dev, staging, and production environments

### Proper Solution: Runtime Configuration Injection

Instead of modifying Bootstrap library files, we implemented a proper runtime configuration mechanism for actual environment-specific values:

## Implementation

### 1. JavaScript Runtime Configuration (`wwwroot/js/config.js`)

Created a configuration module that reads environment-specific values injected at runtime:

```javascript
window.AppConfig = {
    apiBaseUrl: window.ENV_API_BASE_URL || '/api',
    apiTimeout: parseInt(window.ENV_API_TIMEOUT || '30000'),
    enableDebugMode: (window.ENV_DEBUG_MODE || 'false') === 'true',
    // ... other configuration
};
```

### 2. Razor View Helper (`Views/Shared/_EnvironmentConfig.cshtml`)

Injects ASP.NET Core configuration into JavaScript runtime:

```html
<script>
    window.ENV_API_BASE_URL = '@Configuration["AppSettings:ApiBaseUrl"]';
    window.ENV_API_TIMEOUT = '@Configuration["AppSettings:ApiTimeout"]';
    // ... other values
</script>
```

### 3. Application Configuration (`appsettings.json`)

Added AppSettings section with default values that can be overridden by environment variables:

```json
{
  "AppSettings": {
    "ApiBaseUrl": "/api",
    "ApiTimeout": "30000",
    "DebugMode": "false",
    // ... other settings
  }
}
```

### 4. Kubernetes ConfigMaps (`k8s/configmap.yaml`)

Environment-specific non-sensitive configuration:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: orbitaos-config
data:
  AppSettings__ApiBaseUrl: "https://api.production.example.com/api"
  AppSettings__ApiTimeout: "30000"
  Environment: "Production"
```

### 5. Azure Key Vault Integration (`k8s/secrets.yaml`)

Sensitive configuration via Azure Key Vault CSI Driver:

```yaml
apiVersion: secrets-store.csi.x-k8s.io/v1
kind: SecretProviderClass
metadata:
  name: orbitaos-azure-keyvault
spec:
  provider: azure
  parameters:
    keyvaultName: "your-keyvault-name"
    objects: |
      array:
        - objectName: orbitaos-api-key
          objectType: secret
```

### 6. Deployment Configuration (`k8s/deployment.yaml`)

Kubernetes deployment that injects configuration at runtime:

```yaml
spec:
  containers:
  - name: orbitaos
    envFrom:
    - configMapRef:
        name: orbitaos-config
    env:
    - name: AppSettings__ApiKey
      valueFrom:
        secretKeyRef:
          name: orbitaos-secrets
          key: AppSettings__ApiKey
```

## Deployment Workflow

### Single Container Image Promotion

The same container image is promoted across all environments:

```
Build → Dev → Staging → Production
  ↓       ↓       ↓         ↓
Image  Config  Config   Config
       (dev)   (stg)    (prod)
```

### Environment-Specific Configuration

Each environment uses different ConfigMaps and Key Vault references:

**Development:**
```bash
kubectl apply -f k8s/configmap.yaml  # Use orbitaos-config-dev
kubectl apply -f k8s/deployment.yaml
```

**Staging:**
```bash
kubectl apply -f k8s/configmap.yaml  # Use orbitaos-config-staging
kubectl apply -f k8s/deployment.yaml
```

**Production:**
```bash
kubectl apply -f k8s/configmap.yaml  # Use orbitaos-config
kubectl apply -f k8s/deployment.yaml
```

## Azure Key Vault CSI Driver Setup

### Prerequisites

1. Install Azure Key Vault CSI Driver on AKS:
```bash
az aks enable-addons --addons azure-keyvault-secrets-provider --name myAKSCluster --resource-group myResourceGroup
```

2. Create Azure Key Vault:
```bash
az keyvault create --name orbitaos-keyvault --resource-group myResourceGroup --location eastus
```

3. Add secrets to Key Vault:
```bash
az keyvault secret set --vault-name orbitaos-keyvault --name orbitaos-api-key --value "your-api-key"
az keyvault secret set --vault-name orbitaos-keyvault --name orbitaos-db-connection-string --value "Server=..."
```

4. Configure Managed Identity access:
```bash
az keyvault set-policy --name orbitaos-keyvault --object-id <managed-identity-object-id> --secret-permissions get
```

## Health Check Endpoint

The application includes a health check endpoint at `/health` for container orchestration:

- **Liveness Probe:** Checks if the application is running
- **Readiness Probe:** Checks if the application is ready to receive traffic

Configuration in `Program.cs`:
```csharp
builder.Services.AddHealthChecks();
app.MapHealthChecks("/health");
```

## Configuration Hierarchy

ASP.NET Core configuration is loaded in this order (later sources override earlier ones):

1. `appsettings.json` (default values)
2. `appsettings.{Environment}.json`
3. Environment variables (from Kubernetes ConfigMaps)
4. Azure Key Vault secrets (via CSI Driver)

## Best Practices

### ✅ DO:
- Use ConfigMaps for non-sensitive configuration
- Use Azure Key Vault for sensitive data (API keys, connection strings, secrets)
- Use the same container image across all environments
- Inject configuration at runtime via environment variables
- Keep Bootstrap and other third-party libraries unmodified

### ❌ DON'T:
- Hardcode environment-specific values in application code
- Modify third-party library files (Bootstrap, jQuery, etc.)
- Store secrets in ConfigMaps or code
- Build different container images per environment
- Commit secrets to source control

## Verification

### Test Configuration Injection

1. Deploy to Kubernetes:
```bash
kubectl apply -f k8s/
```

2. Check environment variables in pod:
```bash
kubectl exec -it <pod-name> -- env | grep AppSettings
```

3. Verify health endpoint:
```bash
kubectl port-forward <pod-name> 8080:8080
curl http://localhost:8080/health
```

4. Check JavaScript configuration (browser console):
```javascript
console.log(window.AppConfig);
```

## Conclusion

The flagged Bootstrap library files do NOT contain environment-specific configuration and should NOT be modified. The proper solution is to implement runtime configuration injection for actual environment-specific values, which has been implemented through:

1. JavaScript runtime configuration module
2. Razor view helper for configuration injection
3. Kubernetes ConfigMaps for non-sensitive values
4. Azure Key Vault CSI Driver for sensitive values
5. Single container image promotion across environments

This approach follows 12-factor app principles and enables clean container image promotion across all environments in AKS.
