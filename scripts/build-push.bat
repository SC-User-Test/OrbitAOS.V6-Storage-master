@echo off
setlocal enabledelayedexpansion

REM OrbitAOS.V6 - Build and Push Docker Image Script (Windows)
REM This script builds the Docker image and pushes it to your chosen registry

echo =========================================
echo OrbitAOS.V6 - Build and Push to Registry
echo =========================================
echo.

REM Project configuration
set PROJECT_NAME=OrbitAOS.V6
set DOCKERFILE_PATH=.\Dockerfile

REM Sanitize project name for Docker image naming
set IMAGE_NAME=%PROJECT_NAME%
set IMAGE_NAME=!IMAGE_NAME: =-!
for %%i in (A B C D E F G H I J K L M N O P Q R S T U V W X Y Z) do set IMAGE_NAME=!IMAGE_NAME:%%i=%%i!
set IMAGE_NAME=!IMAGE_NAME:.=-!
echo Sanitized image name: !IMAGE_NAME!
echo.

REM Prompt for registry selection
echo Select Docker Registry:
echo 1. AWS ECR (Elastic Container Registry)
echo 2. Docker Hub
set /p REGISTRY_CHOICE="Enter your choice (1 or 2): "
echo.

if "!REGISTRY_CHOICE!"=="1" (
    echo === AWS ECR Configuration ===
    set /p AWS_REGION="Enter AWS Region (e.g., us-east-1): "
    set /p AWS_ACCOUNT_ID="Enter AWS Account ID: "
    set /p ECR_REPO="Enter ECR Repository Name (default: !IMAGE_NAME!): "
    if "!ECR_REPO!"=="" set ECR_REPO=!IMAGE_NAME!
    
    set REGISTRY_URL=!AWS_ACCOUNT_ID!.dkr.ecr.!AWS_REGION!.amazonaws.com
    
    echo.
    echo Authenticating with AWS ECR...
    for /f "tokens=*" %%i in ('aws ecr get-login-password --region !AWS_REGION!') do set ECR_PASSWORD=%%i
    echo !ECR_PASSWORD! | docker login --username AWS --password-stdin !REGISTRY_URL!
    
    if !ERRORLEVEL! neq 0 (
        echo ERROR: ECR authentication failed
        exit /b 1
    )
    
    echo Checking if ECR repository exists...
    aws ecr describe-repositories --repository-names !ECR_REPO! --region !AWS_REGION! >nul 2>&1
    if !ERRORLEVEL! neq 0 (
        echo Repository does not exist. Creating ECR repository: !ECR_REPO!
        aws ecr create-repository --repository-name !ECR_REPO! --region !AWS_REGION!
        if !ERRORLEVEL! neq 0 (
            echo ERROR: Failed to create ECR repository
            exit /b 1
        )
    )
    
    set /p IMAGE_TAG="Enter image tag (default: latest): "
    if "!IMAGE_TAG!"=="" set IMAGE_TAG=latest
    
    set FULL_IMAGE_NAME=!REGISTRY_URL!/!ECR_REPO!:!IMAGE_TAG!
    
) else if "!REGISTRY_CHOICE!"=="2" (
    echo === Docker Hub Configuration ===
    set /p DOCKER_USERNAME="Enter Docker Hub username: "
    set /p DOCKER_PASSWORD="Enter Docker Hub password or access token: "
    
    echo Authenticating with Docker Hub...
    echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
    
    if !ERRORLEVEL! neq 0 (
        echo ERROR: Docker Hub authentication failed
        exit /b 1
    )
    
    set /p IMAGE_TAG="Enter image tag (default: latest): "
    if "!IMAGE_TAG!"=="" set IMAGE_TAG=latest
    
    set FULL_IMAGE_NAME=!DOCKER_USERNAME!/!IMAGE_NAME!:!IMAGE_TAG!
    
) else (
    echo ERROR: Invalid choice. Please run the script again and select 1 or 2.
    exit /b 1
)

echo.
echo =========================================
echo Building Docker Image
echo =========================================
echo Image name: !FULL_IMAGE_NAME!
echo Dockerfile: !DOCKERFILE_PATH!
echo.

REM Build the Docker image
docker build -f "!DOCKERFILE_PATH!" -t "!FULL_IMAGE_NAME!" .

if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker build failed
    exit /b 1
)

echo.
echo =========================================
echo Pushing Docker Image to Registry
echo =========================================
echo.

REM Push the image
docker push "!FULL_IMAGE_NAME!"

if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker push failed
    exit /b 1
)

echo.
echo =========================================
echo Build and Push Completed Successfully!
echo =========================================
echo Image: !FULL_IMAGE_NAME!
echo.
echo Next steps:
echo 1. Use this image URI in your ECS task definition
echo 2. Run the deploy-image.bat script to deploy to AWS ECS
echo.

endlocal