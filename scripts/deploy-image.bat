@echo off
setlocal enabledelayedexpansion

REM OrbitAOS.V6 - Deploy to AWS ECS Fargate (Windows)
REM This script deploys the application to AWS ECS Fargate

echo =========================================
echo OrbitAOS.V6 - Deploy to AWS ECS Fargate
echo =========================================
echo.

REM Configuration
set PROJECT_NAME=orbitaos-v6
set TASK_FAMILY=!PROJECT_NAME!-task
set SERVICE_NAME=!PROJECT_NAME!-service

REM Prompt for AWS configuration
echo === AWS Configuration ===
set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
set /p CLUSTER_NAME="Enter ECS cluster name (e.g., orbitaos-cluster): "
set /p IMAGE_URI="Enter Docker image URI: "

echo.
echo === Network Configuration ===
set /p VPC_ID="Enter VPC ID: "
set /p SUBNETS_INPUT="Enter Subnet IDs comma-separated: "
set /p SECURITY_GROUP="Enter Security Group ID: "

REM Split subnets
for /f "tokens=1,2 delims=," %%a in ("!SUBNETS_INPUT!") do (
    set SUBNET_1=%%a
    set SUBNET_2=%%b
)
if "!SUBNET_2!"=="" set SUBNET_2=!SUBNET_1!

echo.
echo === Database Configuration ===
set /p DB_SERVER="Enter Database Server: "
set /p DB_NAME="Enter Database Name (default: OrbitAOS): "
if "!DB_NAME!"=="" set DB_NAME=OrbitAOS
set /p DB_USER="Enter Database User (default: admin): "
if "!DB_USER!"=="" set DB_USER=admin
set /p DB_PASSWORD="Enter Database Password: "

echo.
echo === Load Balancer Configuration ===
set /p NEED_LB="Do you need a load balancer? (y/n): "

if /i "!NEED_LB!"=="y" (
    set USE_LOAD_BALANCER=true
    echo Creating Application Load Balancer and Target Group...
    
    set ALB_NAME=!PROJECT_NAME!-alb
    echo Creating ALB: !ALB_NAME!
    
    for /f "tokens=*" %%i in ('aws elbv2 create-load-balancer --name !ALB_NAME! --subnets !SUBNET_1! !SUBNET_2! --security-groups !SECURITY_GROUP! --scheme internet-facing --type application --ip-address-type ipv4 --region !AWS_REGION! --query "LoadBalancers[0].LoadBalancerArn" --output text 2^>nul') do set ALB_ARN=%%i
    
    if "!ALB_ARN!"=="" (
        for /f "tokens=*" %%i in ('aws elbv2 describe-load-balancers --names !ALB_NAME! --region !AWS_REGION! --query "LoadBalancers[0].LoadBalancerArn" --output text 2^>nul') do set ALB_ARN=%%i
    )
    
    echo ALB ARN: !ALB_ARN!
    
    for /f "tokens=*" %%i in ('aws elbv2 describe-load-balancers --load-balancer-arns !ALB_ARN! --region !AWS_REGION! --query "LoadBalancers[0].DNSName" --output text') do set ALB_DNS=%%i
    
    set TG_NAME=!PROJECT_NAME!-tg
    echo Creating Target Group: !TG_NAME!
    
    for /f "tokens=*" %%i in ('aws elbv2 create-target-group --name !TG_NAME! --protocol HTTP --port 80 --vpc-id !VPC_ID! --target-type ip --health-check-enabled --health-check-path "/health" --health-check-interval-seconds 30 --region !AWS_REGION! --query "TargetGroups[0].TargetGroupArn" --output text 2^>nul') do set TARGET_GROUP_ARN=%%i
    
    if "!TARGET_GROUP_ARN!"=="" (
        for /f "tokens=*" %%i in ('aws elbv2 describe-target-groups --names !TG_NAME! --region !AWS_REGION! --query "TargetGroups[0].TargetGroupArn" --output text 2^>nul') do set TARGET_GROUP_ARN=%%i
    )
    
    echo Target Group ARN: !TARGET_GROUP_ARN!
    
    aws elbv2 create-listener --load-balancer-arn !ALB_ARN! --protocol HTTP --port 80 --default-actions Type=forward,TargetGroupArn=!TARGET_GROUP_ARN! --region !AWS_REGION! >nul 2>&1
) else (
    set USE_LOAD_BALANCER=false
    set TARGET_GROUP_ARN=
    echo Skipping load balancer creation.
)

echo.
echo === Getting AWS Account ID ===
for /f "tokens=*" %%i in ('aws sts get-caller-identity --query Account --output text') do set ACCOUNT_ID=%%i
echo Account ID: !ACCOUNT_ID!

echo.
echo === Checking ECS Cluster ===
aws ecs describe-clusters --clusters !CLUSTER_NAME! --region !AWS_REGION! >nul 2>&1
if !ERRORLEVEL! neq 0 (
    echo Creating cluster: !CLUSTER_NAME!
    aws ecs create-cluster --cluster-name !CLUSTER_NAME! --region !AWS_REGION!
)
echo Cluster !CLUSTER_NAME! is ready

echo.
echo === Preparing Task Definition ===
set TASK_DEF_TEMPLATE=ecs\task-definition.json
set TASK_DEF_FILE=%TEMP%\!TASK_FAMILY!-task.json

copy /y "!TASK_DEF_TEMPLATE!" "!TASK_DEF_FILE!" >nul

powershell -Command "(Get-Content '!TASK_DEF_FILE!') -replace '{{IMAGE_URI}}', '!IMAGE_URI!' | Set-Content '!TASK_DEF_FILE!'"
powershell -Command "(Get-Content '!TASK_DEF_FILE!') -replace '{{AWS_REGION}}', '!AWS_REGION!' | Set-Content '!TASK_DEF_FILE!'"
powershell -Command "(Get-Content '!TASK_DEF_FILE!') -replace '{{ACCOUNT_ID}}', '!ACCOUNT_ID!' | Set-Content '!TASK_DEF_FILE!'"
powershell -Command "(Get-Content '!TASK_DEF_FILE!') -replace '{{DB_SERVER}}', '!DB_SERVER!' | Set-Content '!TASK_DEF_FILE!'"
powershell -Command "(Get-Content '!TASK_DEF_FILE!') -replace '{{DB_NAME}}', '!DB_NAME!' | Set-Content '!TASK_DEF_FILE!'"
powershell -Command "(Get-Content '!TASK_DEF_FILE!') -replace '{{DB_USER}}', '!DB_USER!' | Set-Content '!TASK_DEF_FILE!'"
powershell -Command "(Get-Content '!TASK_DEF_FILE!') -replace '{{DB_PASSWORD}}', '!DB_PASSWORD!' | Set-Content '!TASK_DEF_FILE!'"

echo Registering task definition...
for /f "tokens=*" %%i in ('aws ecs register-task-definition --cli-input-json file://!TASK_DEF_FILE! --region !AWS_REGION! --query "taskDefinition.taskDefinitionArn" --output text') do set TASK_DEF_ARN=%%i

if "!TASK_DEF_ARN!"=="" (
    echo ERROR: Failed to register task definition
    exit /b 1
)

echo Task Definition ARN: !TASK_DEF_ARN!

echo.
echo === Preparing Service Definition ===
set SERVICE_DEF_TEMPLATE=ecs\service-definition.json
set SERVICE_DEF_FILE=%TEMP%\!SERVICE_NAME!-service.json

copy /y "!SERVICE_DEF_TEMPLATE!" "!SERVICE_DEF_FILE!" >nul

powershell -Command "(Get-Content '!SERVICE_DEF_FILE!') -replace '{{CLUSTER_NAME}}', '!CLUSTER_NAME!' | Set-Content '!SERVICE_DEF_FILE!'"
powershell -Command "(Get-Content '!SERVICE_DEF_FILE!') -replace '{{SUBNET_1}}', '!SUBNET_1!' | Set-Content '!SERVICE_DEF_FILE!'"
powershell -Command "(Get-Content '!SERVICE_DEF_FILE!') -replace '{{SUBNET_2}}', '!SUBNET_2!' | Set-Content '!SERVICE_DEF_FILE!'"
powershell -Command "(Get-Content '!SERVICE_DEF_FILE!') -replace '{{SECURITY_GROUP}}', '!SECURITY_GROUP!' | Set-Content '!SERVICE_DEF_FILE!'"

if "!USE_LOAD_BALANCER!"=="true" (
    powershell -Command "(Get-Content '!SERVICE_DEF_FILE!') -replace '{{TARGET_GROUP_ARN}}', '!TARGET_GROUP_ARN!' | Set-Content '!SERVICE_DEF_FILE!'"
) else (
    powershell -Command "$json = Get-Content '!SERVICE_DEF_FILE!' | ConvertFrom-Json; $json.PSObject.Properties.Remove('loadBalancers'); $json.PSObject.Properties.Remove('healthCheckGracePeriodSeconds'); $json | ConvertTo-Json -Depth 10 | Set-Content '!SERVICE_DEF_FILE!'"
)

echo.
echo === Checking if Service Exists ===
for /f "tokens=*" %%i in ('aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION! --query "services[0].serviceName" --output text 2^>nul') do set SERVICE_EXISTS=%%i

if "!SERVICE_EXISTS!"=="None" (
    echo Creating new service...
    aws ecs create-service --cli-input-json file://!SERVICE_DEF_FILE! --region !AWS_REGION!
    echo Service created successfully
) else (
    echo Updating existing service...
    aws ecs update-service --cluster !CLUSTER_NAME! --service !SERVICE_NAME! --task-definition !TASK_DEF_ARN! --region !AWS_REGION!
    echo Service updated successfully
)

echo.
echo === Waiting for Service Stability ===
echo This may take a few minutes...
aws ecs wait services-stable --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION!

if !ERRORLEVEL! neq 0 (
    echo WARNING: Service did not stabilize
) else (
    echo Service is stable
)

echo.
echo === Deployment Summary ===
echo Cluster: !CLUSTER_NAME!
echo Service: !SERVICE_NAME!
echo Task Definition: !TASK_DEF_ARN!
echo Region: !AWS_REGION!

if "!USE_LOAD_BALANCER!"=="true" (
    echo Load Balancer DNS: !ALB_DNS!
    echo Access your application at: http://!ALB_DNS!
)

echo CloudWatch Log Group: /ecs/orbitaos-v6
echo.
echo =========================================
echo Deployment Complete!
echo =========================================

del /f /q "!TASK_DEF_FILE!" "!SERVICE_DEF_FILE!" >nul 2>&1

endlocal