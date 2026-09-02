# OrbitAOS.V6 - Deployment Guide

## Quick Start

### Local Development with Docker Compose

1. **Build and run the application:**
   ```bash
   docker-compose up --build
   ```

2. **Access the application:**
   - Application: http://localhost:8080
   - Health Check: http://localhost:8080/health

3. **Stop the application:**
   ```bash
   docker-compose down
   ```

### Build Docker Image

```bash
docker build -t orbitaos:latest .
```

### Run Docker Container

```bash
docker run -d \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Server=your-db;Database=OrbitAOS;..." \
  -e AppSettings__ApiBaseUrl="https://api.example.com" \
  --name orbitaos \
  orbitaos:latest
```

## Azure Container Registry (ACR) Deployment

### 1. Login to ACR

```bash
az acr login --name yourregistry
```

### 2. Tag Image

```bash
docker tag orbitaos:latest yourregistry.azurecr.io/orbitaos:latest
docker tag orbitaos:latest yourregistry.azurecr.io/orbitaos:v1.0.0
```

### 3. Push Image

```bash
docker push yourregistry.azurecr.io/orbitaos:latest
docker push yourregistry.azurecr.io/orbitaos:v1.0.0
```

## Azure Kubernetes Service (AKS) Deployment

### Prerequisites

1. **Install Azure CLI:**
   ```bash
   curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
   ```

2. **Install kubectl:**
   ```bash
   az aks install-cli
   ```

3. **Connect to AKS cluster:**
   ```bash
   az aks get-credentials --resource-group myResourceGroup --name myAKSCluster
   ```

### Setup Azure Key Vault CSI Driver

1. **Enable CSI Driver on AKS:**
   ```bash
   az aks enable-addons \
     --addons azure-keyvault-secrets-provider \
     --name myAKSCluster \
     --resource-group myResourceGroup
   ```

2. **Create Azure Key Vault:**
   ```bash
   az keyvault create \
     --name orbitaos-keyvault \
     --resource-group myResourceGroup \
     --location eastus
   ```

3. **Add secrets to Key Vault:**
   ```bash
   # API Key
   az keyvault secret set \
     --vault-name orbitaos-keyvault \
     --name orbitaos-api-key \
     --value "your-production-api-key"
   
   # Database Connection String
   az keyvault secret set \
     --vault-name orbitaos-keyvault \
     --name orbitaos-db-connection-string \
     --value "Server=prod-sql.database.windows.net;Database=OrbitAOS;User Id=admin;Password=..."
   
   # Auth Client Secret
   az keyvault secret set \
     --vault-name orbitaos-keyvault \
     --name orbitaos-auth-client-secret \
     --value "your-auth-client-secret"
   ```

4. **Create Managed Identity:**
   ```bash
   az identity create \
     --name orbitaos-identity \
     --resource-group myResourceGroup
   ```

5. **Grant Key Vault access to Managed Identity:**
   ```bash
   # Get identity details
   IDENTITY_CLIENT_ID=$(az identity show --name orbitaos-identity --resource-group myResourceGroup --query clientId -o tsv)
   IDENTITY_OBJECT_ID=$(az identity show --name orbitaos-identity --resource-group myResourceGroup --query principalId -o tsv)
   
   # Grant access
   az keyvault set-policy \
     --name orbitaos-keyvault \
     --object-id $IDENTITY_OBJECT_ID \
     --secret-permissions get list
   ```

6. **Update SecretProviderClass:**
   Edit `k8s/secrets.yaml` and update:
   - `userAssignedIdentityID`: Use `$IDENTITY_CLIENT_ID`
   - `keyvaultName`: Use `orbitaos-keyvault`
   - `tenantId`: Your Azure AD tenant ID

### Deploy to AKS

#### Development Environment

```bash
# Apply ConfigMap for dev
kubectl apply -f k8s/configmap.yaml

# Apply secrets configuration
kubectl apply -f k8s/secrets.yaml

# Deploy application (update deployment.yaml to use orbitaos-config-dev)
kubectl apply -f k8s/deployment.yaml

# Verify deployment
kubectl get pods -l app=orbitaos
kubectl logs -l app=orbitaos
```

#### Staging Environment

```bash
# Update deployment to use staging ConfigMap
# Edit k8s/deployment.yaml: change configMapRef name to orbitaos-config-staging

kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/deployment.yaml
```

#### Production Environment

```bash
# Update deployment to use production ConfigMap
# Edit k8s/deployment.yaml: change configMapRef name to orbitaos-config

kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/deployment.yaml
```

### Verify Deployment

1. **Check pod status:**
   ```bash
   kubectl get pods -l app=orbitaos
   ```

2. **Check pod logs:**
   ```bash
   kubectl logs -l app=orbitaos --tail=100
   ```

3. **Check environment variables:**
   ```bash
   kubectl exec -it <pod-name> -- env | grep AppSettings
   ```

4. **Test health endpoint:**
   ```bash
   kubectl port-forward svc/orbitaos-service 8080:80
   curl http://localhost:8080/health
   ```

5. **Check mounted secrets:**
   ```bash
   kubectl exec -it <pod-name> -- ls -la /mnt/secrets-store
   ```

## Configuration Management

### Environment-Specific Configuration

Each environment uses a different ConfigMap:

| Environment | ConfigMap Name | Key Vault |
|------------|----------------|-----------|
| Development | orbitaos-config-dev | orbitaos-keyvault-dev |
| Staging | orbitaos-config-staging | orbitaos-keyvault-staging |
| Production | orbitaos-config | orbitaos-keyvault |

### Update Configuration

1. **Update ConfigMap:**
   ```bash
   kubectl edit configmap orbitaos-config
   ```

2. **Restart pods to pick up changes:**
   ```bash
   kubectl rollout restart deployment orbitaos-deployment
   ```

3. **Update Key Vault secret:**
   ```bash
   az keyvault secret set \
     --vault-name orbitaos-keyvault \
     --name orbitaos-api-key \
     --value "new-api-key"
   ```

4. **Restart pods to pick up new secrets:**
   ```bash
   kubectl rollout restart deployment orbitaos-deployment
   ```

## Monitoring and Troubleshooting

### View Logs

```bash
# Real-time logs
kubectl logs -f -l app=orbitaos

# Last 100 lines
kubectl logs -l app=orbitaos --tail=100

# Logs from specific pod
kubectl logs <pod-name>
```

### Check Health

```bash
# Port forward to local machine
kubectl port-forward svc/orbitaos-service 8080:80

# Test health endpoint
curl http://localhost:8080/health
```

### Debug Pod

```bash
# Get shell access
kubectl exec -it <pod-name> -- /bin/bash

# Check environment variables
kubectl exec -it <pod-name> -- env

# Check mounted volumes
kubectl exec -it <pod-name> -- ls -la /mnt/secrets-store
```

### Common Issues

#### Pod not starting

```bash
# Check pod events
kubectl describe pod <pod-name>

# Check logs
kubectl logs <pod-name>
```

#### Health check failing

```bash
# Check if application is listening on correct port
kubectl exec -it <pod-name> -- netstat -tlnp

# Test health endpoint from inside pod
kubectl exec -it <pod-name> -- curl http://localhost:8080/health
```

#### Secrets not loading

```bash
# Check SecretProviderClass
kubectl describe secretproviderclass orbitaos-azure-keyvault

# Check if secrets are mounted
kubectl exec -it <pod-name> -- ls -la /mnt/secrets-store

# Check pod events for CSI driver errors
kubectl describe pod <pod-name>
```

## Scaling

### Manual Scaling

```bash
# Scale to 5 replicas
kubectl scale deployment orbitaos-deployment --replicas=5

# Verify scaling
kubectl get pods -l app=orbitaos
```

### Horizontal Pod Autoscaler (HPA)

```bash
# Create HPA
kubectl autoscale deployment orbitaos-deployment \
  --cpu-percent=70 \
  --min=3 \
  --max=10

# Check HPA status
kubectl get hpa
```

## Rolling Updates

### Update Image

```bash
# Update deployment with new image
kubectl set image deployment/orbitaos-deployment \
  orbitaos=yourregistry.azurecr.io/orbitaos:v1.1.0

# Check rollout status
kubectl rollout status deployment/orbitaos-deployment

# View rollout history
kubectl rollout history deployment/orbitaos-deployment
```

### Rollback

```bash
# Rollback to previous version
kubectl rollout undo deployment/orbitaos-deployment

# Rollback to specific revision
kubectl rollout undo deployment/orbitaos-deployment --to-revision=2
```

## Security Best Practices

1. **Use non-root user in container** ✅ (Implemented in Dockerfile)
2. **Store secrets in Azure Key Vault** ✅ (Configured)
3. **Use managed identities** ✅ (Configured)
4. **Enable HTTPS** ✅ (Configured in Ingress)
5. **Scan images for vulnerabilities:**
   ```bash
   az acr task run --registry yourregistry --name scan-image
   ```

## CI/CD Integration

### Azure DevOps Pipeline Example

```yaml
trigger:
  branches:
    include:
    - main

pool:
  vmImage: 'ubuntu-latest'

variables:
  dockerRegistryServiceConnection: 'your-acr-connection'
  imageRepository: 'orbitaos'
  containerRegistry: 'yourregistry.azurecr.io'
  dockerfilePath: '$(Build.SourcesDirectory)/Dockerfile'
  tag: '$(Build.BuildId)'

stages:
- stage: Build
  jobs:
  - job: BuildAndPush
    steps:
    - task: Docker@2
      inputs:
        command: buildAndPush
        repository: $(imageRepository)
        dockerfile: $(dockerfilePath)
        containerRegistry: $(dockerRegistryServiceConnection)
        tags: |
          $(tag)
          latest

- stage: Deploy
  jobs:
  - job: DeployToAKS
    steps:
    - task: KubernetesManifest@0
      inputs:
        action: deploy
        manifests: |
          k8s/configmap.yaml
          k8s/secrets.yaml
          k8s/deployment.yaml
```

## Support

For issues or questions:
1. Check logs: `kubectl logs -l app=orbitaos`
2. Check pod status: `kubectl describe pod <pod-name>`
3. Review CONTAINERIZATION_GUIDE.md for configuration details
