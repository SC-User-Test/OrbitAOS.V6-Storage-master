#!/bin/bash
set -e
set -o pipefail

PROJECT_NAME="OrbitAOS.V6"

echo "-------------------------------------------------------"
echo " .NET Build and Push Script for Azure AKS"
echo "-------------------------------------------------------"

# Tag Sanitization
read -p "Enter IMAGE_TAG (default: latest): " IMAGE_TAG
IMAGE_TAG=${IMAGE_TAG:-latest}
IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

# Project Name Sanitization
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

echo "Registry Selection:"
echo "1) Azure ACR"
echo "2) Docker Hub"
read -p "Select registry [1-2]: " REGISTRY_CHOICE

if [ "$REGISTRY_CHOICE" == "1" ]; then
    read -p "Enter ACR Name (e.g., myacr.azurecr.io): " ACR_NAME
    # Remove .azurecr.io if provided to use with az acr login
    ACR_SHORT_NAME=$(echo "$ACR_NAME" | sed 's/\.azurecr\.io//')
    az acr login --name "$ACR_SHORT_NAME"
    REGISTRY_URL="$ACR_NAME"
elif [ "$REGISTRY_CHOICE" == "2" ]; then
    read -p "Enter Docker Hub Username: " DOCKER_USERNAME
    read -sp "Enter Docker Hub Password: " DOCKER_PASSWORD
    echo ""
    echo "$DOCKER_PASSWORD" | docker login --username "$DOCKER_USERNAME" --password-stdin
    REGISTRY_URL="docker.io/$DOCKER_USERNAME"
else
    echo "Invalid selection"
    exit 1
fi

FULL_IMAGE_NAME="$REGISTRY_URL/$IMAGE_NAME:$IMAGE_TAG"

echo "Building image: $FULL_IMAGE_NAME..."
docker build -t "$FULL_IMAGE_NAME" .

echo "Pushing image to registry..."
docker push "$FULL_IMAGE_NAME"

echo "-------------------------------------------------------"
echo "Successfully built and pushed: $FULL_IMAGE_NAME"
echo "-------------------------------------------------------"
