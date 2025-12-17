@echo off
setlocal enabledelayedexpansion

echo ==================================
echo AWS ECS Fargate Deployment Script
echo ==================================
echo.

set PROJECT_NAME=orbitcomp
set SERVICE_NAME=!PROJECT_NAME!-service
set TASK_FAMILY=!PROJECT_NAME!-task

set /p AWS_REGION="Enter AWS Region (e.g., us-east-1): "
set /p CLUSTER_NAME="Enter ECS Cluster Name (e.g., my-ecs-cluster): "
set /p VPC_ID="Enter VPC ID (e.g., vpc-0abc123def456): "
set /p SUBNET_IDS="Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): "
set /p SECURITY_GROUP="Enter Security Group ID (e.g., sg-0abc123def): "
set /p IMAGE_URI="Enter Docker Image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/orbitcomp:latest): "

for /f "tokens=1,2 delims=," %%a in ("!SUBNET_IDS!") do (
    set SUBNET_1=%%a
    set SUBNET_2=%%b
)

if "!SUBNET_2!"=="" set SUBNET_2=!SUBNET_1!

echo.
echo Deployment Configuration:
echo   Region: !AWS_REGION!
echo   Cluster: !CLUSTER_NAME!
echo   VPC: !VPC_ID!
echo   Subnets: !SUBNET_1!, !SUBNET_2!
echo   Security Group: !SECURITY_GROUP!
echo   Image: !IMAGE_URI!
echo.

echo Retrieving AWS Account ID...
for /f "delims=" %%a in ('aws sts get-caller-identity --query Account --output text') do set ACCOUNT_ID=%%a
echo Account ID: !ACCOUNT_ID!
echo.

echo Checking if ECS cluster exists...
aws ecs describe-clusters --clusters !CLUSTER_NAME! --region !AWS_REGION! >nul 2>&1

if !ERRORLEVEL! neq 0 (
    echo Cluster does not exist. Creating ECS cluster: !CLUSTER_NAME!
    aws ecs create-cluster --cluster-name !CLUSTER_NAME! --region !AWS_REGION!
    echo Cluster created successfully
)
echo.

echo Creating CloudWatch Log Group...
set LOG_GROUP=/ecs/!PROJECT_NAME!
aws logs create-log-group --log-group-name !LOG_GROUP! --region !AWS_REGION! 2>nul
echo.

set /p NEED_LB="Do you need a load balancer for this service? (y/n): "

if /i "!NEED_LB!"=="y" (
    echo.
    echo Creating Application Load Balancer and Target Group...
    
    set ALB_NAME=!PROJECT_NAME!-alb
    echo Creating ALB: !ALB_NAME!
    
    for /f "delims=" %%a in ('aws elbv2 create-load-balancer --name !ALB_NAME! --subnets !SUBNET_1! !SUBNET_2! --security-groups !SECURITY_GROUP! --scheme internet-facing --type application --ip-address-type ipv4 --region !AWS_REGION! --query "LoadBalancers[0].LoadBalancerArn" --output text 2^>nul') do set ALB_ARN=%%a
    
    if "!ALB_ARN!"=="" (
        for /f "delims=" %%a in ('aws elbv2 describe-load-balancers --names !ALB_NAME! --region !AWS_REGION! --query "LoadBalancers[0].LoadBalancerArn" --output text') do set ALB_ARN=%%a
    )
    
    echo ALB ARN: !ALB_ARN!
    
    for /f "delims=" %%a in ('aws elbv2 describe-load-balancers --load-balancer-arns !ALB_ARN! --region !AWS_REGION! --query "LoadBalancers[0].DNSName" --output text') do set ALB_DNS=%%a
    
    set TG_NAME=!PROJECT_NAME!-tg
    echo Creating Target Group: !TG_NAME!
    
    for /f "delims=" %%a in ('aws elbv2 create-target-group --name !TG_NAME! --protocol HTTP --port 80 --vpc-id !VPC_ID! --target-type ip --health-check-enabled --health-check-protocol HTTP --health-check-path "/health" --health-check-interval-seconds 30 --health-check-timeout-seconds 10 --healthy-threshold-count 2 --unhealthy-threshold-count 3 --region !AWS_REGION! --query "TargetGroups[0].TargetGroupArn" --output text 2^>nul') do set TARGET_GROUP_ARN=%%a
    
    if "!TARGET_GROUP_ARN!"=="" (
        for /f "delims=" %%a in ('aws elbv2 describe-target-groups --names !TG_NAME! --region !AWS_REGION! --query "TargetGroups[0].TargetGroupArn" --output text') do set TARGET_GROUP_ARN=%%a
    )
    
    echo Target Group ARN: !TARGET_GROUP_ARN!
    
    echo Creating ALB Listener...
    aws elbv2 create-listener --load-balancer-arn !ALB_ARN! --protocol HTTP --port 80 --default-actions Type=forward,TargetGroupArn=!TARGET_GROUP_ARN! --region !AWS_REGION! >nul 2>&1
    
    echo Load balancer setup completed
    echo.
    
    set USE_LOAD_BALANCER=true
) else (
    set USE_LOAD_BALANCER=false
    echo Skipping load balancer setup
    echo.
)

echo Preparing task definition...
copy ecs\task-definition.json %TEMP%\task-definition.json >nul

powershell -Command "(Get-Content %TEMP%\task-definition.json) -replace '{{IMAGE_URI}}','!IMAGE_URI!' | Set-Content %TEMP%\task-definition.json"
powershell -Command "(Get-Content %TEMP%\task-definition.json) -replace '{{AWS_REGION}}','!AWS_REGION!' | Set-Content %TEMP%\task-definition.json"
powershell -Command "(Get-Content %TEMP%\task-definition.json) -replace '{{ACCOUNT_ID}}','!ACCOUNT_ID!' | Set-Content %TEMP%\task-definition.json"

echo Registering task definition...
for /f "delims=" %%a in ('aws ecs register-task-definition --cli-input-json file://%TEMP%/task-definition.json --region !AWS_REGION! --query "taskDefinition.taskDefinitionArn" --output text') do set TASK_DEF_ARN=%%a

if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to register task definition
    exit /b 1
)

echo Task definition registered: !TASK_DEF_ARN!
echo.

echo Preparing service definition...
copy ecs\service-definition.json %TEMP%\service-definition.json >nul

powershell -Command "(Get-Content %TEMP%\service-definition.json) -replace '{{CLUSTER_NAME}}','!CLUSTER_NAME!' | Set-Content %TEMP%\service-definition.json"
powershell -Command "(Get-Content %TEMP%\service-definition.json) -replace '{{SUBNET_1}}','!SUBNET_1!' | Set-Content %TEMP%\service-definition.json"
powershell -Command "(Get-Content %TEMP%\service-definition.json) -replace '{{SUBNET_2}}','!SUBNET_2!' | Set-Content %TEMP%\service-definition.json"
powershell -Command "(Get-Content %TEMP%\service-definition.json) -replace '{{SECURITY_GROUP}}','!SECURITY_GROUP!' | Set-Content %TEMP%\service-definition.json"

if "!USE_LOAD_BALANCER!"=="true" (
    powershell -Command "(Get-Content %TEMP%\service-definition.json) -replace '{{TARGET_GROUP_ARN}}','!TARGET_GROUP_ARN!' | Set-Content %TEMP%\service-definition.json"
) else (
    powershell -Command "$json = Get-Content %TEMP%\service-definition.json | ConvertFrom-Json; $json.PSObject.Properties.Remove('loadBalancers'); $json.PSObject.Properties.Remove('healthCheckGracePeriodSeconds'); $json | ConvertTo-Json -Depth 10 | Set-Content %TEMP%\service-definition.json"
)

echo Checking if service exists...
for /f "delims=" %%a in ('aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION! --query "services[0].serviceName" --output text 2^>nul') do set EXISTING_SERVICE=%%a

if "!EXISTING_SERVICE!"=="!SERVICE_NAME!" (
    echo Service exists. Updating service...
    aws ecs update-service --cluster !CLUSTER_NAME! --service !SERVICE_NAME! --task-definition !TASK_DEF_ARN! --force-new-deployment --region !AWS_REGION! >nul
    
    if !ERRORLEVEL! neq 0 (
        echo ERROR: Failed to update service
        exit /b 1
    )
    
    echo Service updated successfully
) else (
    echo Service does not exist. Creating service...
    aws ecs create-service --cli-input-json file://%TEMP%/service-definition.json --region !AWS_REGION! >nul
    
    if !ERRORLEVEL! neq 0 (
        echo ERROR: Failed to create service
        exit /b 1
    )
    
    echo Service created successfully
)

echo.
echo Waiting for service to become stable...
aws ecs wait services-stable --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION!

if !ERRORLEVEL! neq 0 (
    echo WARNING: Service did not become stable within the expected time
    echo Check the ECS console for more details
) else (
    echo Service is stable
)

echo.
echo ==================================
echo Deployment Summary
echo ==================================

for /f "delims=" %%a in ('aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION! --query "services[0].runningCount" --output text') do set RUNNING_COUNT=%%a
for /f "delims=" %%a in ('aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION! --query "services[0].desiredCount" --output text') do set DESIRED_COUNT=%%a

echo Cluster: !CLUSTER_NAME!
echo Service: !SERVICE_NAME!
echo Task Definition: !TASK_DEF_ARN!
echo Running Tasks: !RUNNING_COUNT!/!DESIRED_COUNT!
echo.

if "!USE_LOAD_BALANCER!"=="true" (
    echo Application Load Balancer: !ALB_DNS!
    echo Access your application at: http://!ALB_DNS!
    echo.
)

echo CloudWatch Logs: !LOG_GROUP!
echo View logs: aws logs tail !LOG_GROUP! --follow --region !AWS_REGION!
echo.
echo ECS Console: https://console.aws.amazon.com/ecs/v2/clusters/!CLUSTER_NAME!/services/!SERVICE_NAME!/health?region=!AWS_REGION!
echo.
echo Deployment completed successfully!
echo.

del %TEMP%\task-definition.json %TEMP%\service-definition.json 2>nul

endlocal