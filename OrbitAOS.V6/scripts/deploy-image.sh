#!/bin/bash
set -e
set -o pipefail

PROJECT_NAME="orbitaos-v6"

echo "-------------------------------------------------------"
echo " Azure AKS Deployment Script"
echo "-------------------------------------------------------"

# Azure Credentials
read -p "Enter Azure Resource Group: " RESOURCE_GROUP
read -p "Enter AKS Cluster Name: " CLUSTER_NAME

# Image URI
read -p "Enter Full Docker Image URI (e.g., myacr.azurecr.io/orbitaos-v6:latest): " IMAGE_URI

# Application Specific Environment Variables
echo "Configuration for OrbitAOS.V6:"
read -p "Enter DATABASE_URL (or press Enter to skip): " DATABASE_URL
read -p "Enter API_KEY (or press Enter to skip): " API_KEY

# Configure kubectl
echo "Configuring kubectl for AKS..."
az aks get-credentials --resource-group "$RESOURCE_GROUP" --name "$CLUSTER_NAME"

# Verify connectivity
echo "Verifying cluster connectivity..."
kubectl cluster-info || { echo "Cluster connectivity failed"; exit 1; }

# Update manifests
echo "Updating manifests with provided values..."
sed -i "s|{{IMAGE_URI}}|$IMAGE_URI|g" kubernetes/deployment.yaml
sed -i "s|{{DATABASE_URL}}|$DATABASE_URL|g" kubernetes/deployment.yaml
sed -i "s|{{API_KEY}}|$API_KEY|g" kubernetes/deployment.yaml

# Apply manifests
echo "Applying Kubernetes manifests..."
kubectl apply -f kubernetes/namespace.yaml
kubectl apply -f kubernetes/deployment.yaml
kubectl apply -f kubernetes/service.yaml
kubectl apply -f kubernetes/ingress.yaml

# Wait for rollout
echo "Waiting for deployment rollout..."
kubectl rollout status deployment/"$PROJECT_NAME" -n "$PROJECT_NAME"

# Verify resources
echo "Verifying resources..."
kubectl get pods,svc,ingress -n "$PROJECT_NAME"

echo "-------------------------------------------------------"
echo "Deployment Complete!"
echo "Application URL: http://orbitaos-v6.example.com"
echo "-------------------------------------------------------"
echo "Rollback command: kubectl rollout undo deployment/$PROJECT_NAME -n $PROJECT_NAME"
