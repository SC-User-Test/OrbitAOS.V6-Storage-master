@echo off
setlocal enabledelayedexpansion

set PROJECT_NAME=OrbitAOS.V6

echo -------------------------------------------------------
echo .NET Build and Push Script for Azure AKS
echo -------------------------------------------------------

set /p IMAGE_TAG="Enter IMAGE_TAG (default: latest): "
if "!IMAGE_TAG!"=="" set IMAGE_TAG=latest

:: Tag Sanitization using PowerShell
for /f "usebackq tokens=*" %%i in (`powershell -Command "'!IMAGE_TAG!'.ToLower().Replace(' ', '-').Trim('-')"`) do set IMAGE_TAG=%%i

:: Project Name Sanitization
for /f "usebackq tokens=*" %%i in (`powershell -Command "'!PROJECT_NAME!'.ToLower().Replace(' ', '-').Trim('-')"`) do set IMAGE_NAME=%%i

echo Registry Selection:
echo 1) Azure ACR
echo 2) Docker Hub
set /p REGISTRY_CHOICE="Select registry [1-2]: "

if "!REGISTRY_CHOICE!"=="1" (
    set /p ACR_NAME="Enter ACR Name (e.g., myacr.azurecr.io): "
    set ACR_SHORT_NAME=!ACR_NAME:.azurecr.io=!
    az acr login --name !ACR_SHORT_NAME!
    if !ERRORLEVEL! neq 0 (
        echo ACR login failed
        exit /b 1
    )
    set REGISTRY_URL=!ACR_NAME!
) else if "!REGISTRY_CHOICE!"=="2" (
    set /p DOCKER_USERNAME="Enter Docker Hub Username: "
    set /p DOCKER_PASSWORD="Enter Docker Hub Password: "
    echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
    if !ERRORLEVEL! neq 0 (
        echo Docker Hub login failed
        exit /b 1
    )
    set REGISTRY_URL=docker.io/!DOCKER_USERNAME!
) else (
    echo Invalid selection
    exit /b 1
)

set FULL_IMAGE_NAME=!REGISTRY_URL!/!IMAGE_NAME!:!IMAGE_TAG!

echo Building image: !FULL_IMAGE_NAME!...
docker build -t !FULL_IMAGE_NAME! .
if !ERRORLEVEL! neq 0 (
    echo Docker build failed
    exit /b 1
)

echo Pushing image to registry...
docker push !FULL_IMAGE_NAME!
if !ERRORLEVEL! neq 0 (
    echo Docker push failed
    exit /b 1
)

echo -------------------------------------------------------
echo Successfully built and pushed: !FULL_IMAGE_NAME!
echo -------------------------------------------------------
