@echo off
setlocal enabledelayedexpansion

set PROJECT_NAME=orbitaos-v6

echo -------------------------------------------------------
echo Azure AKS Deployment Script
echo -------------------------------------------------------

set /p RESOURCE_GROUP="Enter Azure Resource Group: "
set /p CLUSTER_NAME="Enter AKS Cluster Name: "
set /p IMAGE_URI="Enter Full Docker Image URI (e.g., myacr.azurecr.io/orbitaos-v6:latest): "

echo Configuration for OrbitAOS.V6:
set /p DATABASE_URL="Enter DATABASE_URL (or press Enter to skip): "
set /p API_KEY="Enter API_KEY (or press Enter to skip): "

echo Configuring kubectl for AKS...
az aks get-credentials --resource-group !RESOURCE_GROUP! --name !CLUSTER_NAME!
if !ERRORLEVEL! neq 0 (
    echo Failed to get AKS credentials
    exit /b 1
)

echo Verifying cluster connectivity...
kubectl cluster-info
if !ERRORLEVEL! neq 0 (
    echo Cluster connectivity failed
    exit /b 1
)

echo Updating manifests with provided values...
:: Using PowerShell for sed-like replacement in Windows
powershell -Command "(Get-Content kubernetes/deployment.yaml) -replace '\{\{IMAGE_URI\}\}', '!IMAGE_URI!' | Set-Content kubernetes/deployment.yaml"
powershell -Command "(Get-Content kubernetes/deployment.yaml) -replace '\{\{DATABASE_URL\}\}', '!DATABASE_URL!' | Set-Content kubernetes/deployment.yaml"
powershell -Command "(Get-Content kubernetes/deployment.yaml) -replace '\{\{API_KEY\}\}', '!API_KEY!' | Set-Content kubernetes/deployment.yaml"

echo Applying Kubernetes manifests...
kubectl apply -f kubernetes/namespace.yaml
kubectl apply -f kubernetes/deployment.yaml
kubectl apply -f kubernetes/service.yaml
kubectl apply -f kubernetes/ingress.yaml
if !ERRORLEVEL! neq 0 (
    echo Kubernetes apply failed
    exit /b 1
)

echo Waiting for deployment rollout...
kubectl rollout status deployment/!PROJECT_NAME! -n !PROJECT_NAME!

echo Verifying resources...
kubectl get pods,svc,ingress -n !PROJECT_NAME!

echo -------------------------------------------------------
echo Deployment Complete!
echo Application URL: http://orbitaos-v6.example.com
echo -------------------------------------------------------
echo Rollback command: kubectl rollout undo deployment/!PROJECT_NAME! -n !PROJECT_NAME!
