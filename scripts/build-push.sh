#!/bin/bash
set -e
set -o pipefail

# OrbitAOS.V6 - Build and Push Docker Image Script
# This script builds the Docker image and pushes it to your chosen registry

echo "========================================="
echo "OrbitAOS.V6 - Build and Push to Registry"
echo "========================================="
echo ""

# Project configuration
PROJECT_NAME="OrbitAOS.V6"
DOCKERFILE_PATH="./Dockerfile"

# Sanitize project name for Docker image naming
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')
echo "Sanitized image name: $IMAGE_NAME"
echo ""

# Prompt for registry selection
echo "Select Docker Registry:"
echo "1. AWS ECR (Elastic Container Registry)"
echo "2. Docker Hub"
read -p "Enter your choice (1 or 2): " REGISTRY_CHOICE
echo ""

if [ "$REGISTRY_CHOICE" = "1" ]; then
    echo "=== AWS ECR Configuration ==="
    read -p "Enter AWS Region (e.g., us-east-1): " AWS_REGION
    read -p "Enter AWS Account ID: " AWS_ACCOUNT_ID
    read -p "Enter ECR Repository Name (default: $IMAGE_NAME): " ECR_REPO
    ECR_REPO=${ECR_REPO:-$IMAGE_NAME}
    
    REGISTRY_URL="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com"
    
    echo ""
    echo "Authenticating with AWS ECR..."
    aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$REGISTRY_URL"
    
    if [ $? -ne 0 ]; then
        echo "ERROR: ECR authentication failed"
        exit 1
    fi
    
    echo "Checking if ECR repository exists..."
    aws ecr describe-repositories --repository-names "$ECR_REPO" --region "$AWS_REGION" >/dev/null 2>&1 || {
        echo "Repository does not exist. Creating ECR repository: $ECR_REPO"
        aws ecr create-repository --repository-name "$ECR_REPO" --region "$AWS_REGION"
        if [ $? -ne 0 ]; then
            echo "ERROR: Failed to create ECR repository"
            exit 1
        fi
    }
    
    read -p "Enter image tag (default: latest): " IMAGE_TAG
    IMAGE_TAG=${IMAGE_TAG:-latest}
    IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
    
    FULL_IMAGE_NAME="${REGISTRY_URL}/${ECR_REPO}:${IMAGE_TAG}"
    
elif [ "$REGISTRY_CHOICE" = "2" ]; then
    echo "=== Docker Hub Configuration ==="
    read -p "Enter Docker Hub username: " DOCKER_USERNAME
    read -sp "Enter Docker Hub password or access token: " DOCKER_PASSWORD
    echo ""
    
    echo "Authenticating with Docker Hub..."
    echo "$DOCKER_PASSWORD" | docker login --username "$DOCKER_USERNAME" --password-stdin
    
    if [ $? -ne 0 ]; then
        echo "ERROR: Docker Hub authentication failed"
        exit 1
    fi
    
    read -p "Enter image tag (default: latest): " IMAGE_TAG
    IMAGE_TAG=${IMAGE_TAG:-latest}
    IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
    
    FULL_IMAGE_NAME="${DOCKER_USERNAME}/${IMAGE_NAME}:${IMAGE_TAG}"
    
else
    echo "ERROR: Invalid choice. Please run the script again and select 1 or 2."
    exit 1
fi

echo ""
echo "========================================="
echo "Building Docker Image"
echo "========================================="
echo "Image name: $FULL_IMAGE_NAME"
echo "Dockerfile: $DOCKERFILE_PATH"
echo ""

# Build the Docker image
docker build -f "$DOCKERFILE_PATH" -t "$FULL_IMAGE_NAME" .

if [ $? -ne 0 ]; then
    echo "ERROR: Docker build failed"
    exit 1
fi

echo ""
echo "========================================="
echo "Pushing Docker Image to Registry"
echo "========================================="
echo ""

# Push the image
docker push "$FULL_IMAGE_NAME"

if [ $? -ne 0 ]; then
    echo "ERROR: Docker push failed"
    exit 1
fi

echo ""
echo "========================================="
echo "Build and Push Completed Successfully!"
echo "========================================="
echo "Image: $FULL_IMAGE_NAME"
echo ""
echo "Next steps:"
echo "1. Use this image URI in your ECS task definition"
echo "2. Run the deploy-image.sh script to deploy to AWS ECS"
echo ""