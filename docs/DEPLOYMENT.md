# OrbitComp - AWS ECS Fargate Deployment Guide

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Local Development Setup](#local-development-setup)
4. [Docker Deployment](#docker-deployment)
5. [AWS ECS Fargate Prerequisites](#aws-ecs-fargate-prerequisites)
6. [ECS Fargate Setup](#ecs-fargate-setup)
7. [ECS Task Definition Explained](#ecs-task-definition-explained)
8. [ECS Service Configuration](#ecs-service-configuration)
9. [ECS Fargate Deployment Walkthrough](#ecs-fargate-deployment-walkthrough)
10. [Configuration Management](#configuration-management)
11. [Security Considerations](#security-considerations)
12. [ECS-Specific Troubleshooting](#ecs-specific-troubleshooting)
13. [ECS Fargate Scaling and Management](#ecs-fargate-scaling-and-management)
14. [Technology-Specific Notes](#technology-specific-notes)

---

## Overview

OrbitComp is a .NET 6.0 ASP.NET Core application designed for containerized deployment on AWS ECS Fargate. This guide provides comprehensive instructions for building, deploying, and managing the application in production.

**Application Details:**
- **Technology**: .NET 6.0 ASP.NET Core
- **Build Tool**: dotnet CLI
- **Application Port**: 80
- **Health Endpoint**: `/health`
- **Target Platform**: AWS ECS Fargate

---

## Prerequisites

### Required Software

1. **Docker Desktop** (20.10+)
   - Download: https://www.docker.com/products/docker-desktop
   - Verify: `docker --version`

2. **AWS CLI** (2.x)
   - Download: https://aws.amazon.com/cli/
   - Verify: `aws --version`
   - Configure: `aws configure`

3. **.NET SDK** (6.0+) - for local development
   - Download: https://dotnet.microsoft.com/download
   - Verify: `dotnet --version`

4. **Git** - for version control
   - Download: https://git-scm.com/downloads
   - Verify: `git --version`

### AWS Account Requirements

- AWS account with appropriate permissions
- IAM user with programmatic access
- AWS credentials configured locally

---

## Local Development Setup

### Running Locally Without Docker

1. **Navigate to Project Directory:**
   ```bash
   cd /modernize-data/studio-data/TNT1001/APP2275/transformed-code/621/studio-workspace/OrbitComp
   ```

2. **Restore Dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build Application:**
   ```bash
   dotnet build -c Release
   ```

4. **Run Application:**
   ```bash
   dotnet run
   ```

5. **Access Application:**
   - URL: http://localhost:5000 (or port specified in appsettings.json)
   - Health Check: http://localhost:5000/health

### Configuration Files

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production overrides

---

## Docker Deployment

### Building Docker Image Locally

1. **Build Image:**
   ```bash
   docker build -f Dockerfile -t orbitcomp:latest .
   ```

2. **Run Container:**
   ```bash
   docker run -d -p 8080:80 --name orbitcomp-app orbitcomp:latest
   ```

3. **Verify Container:**
   ```bash
   docker ps
   docker logs orbitcomp-app
   ```

4. **Access Application:**
   - URL: http://localhost:8080
   - Health Check: http://localhost:8080/health

### Using Docker Compose

1. **Start Application:**
   ```bash
   docker-compose up -d
   ```

2. **View Logs:**
   ```bash
   docker-compose logs -f
   ```

3. **Stop Application:**
   ```bash
   docker-compose down
   ```

---

## AWS ECS Fargate Prerequisites

### 1. IAM Roles

#### ECS Task Execution Role

Create IAM role `ecsTaskExecutionRole` with the following managed policy:
- `AmazonECSTaskExecutionRolePolicy`

This role allows ECS to:
- Pull container images from ECR
- Write logs to CloudWatch Logs

**Create via AWS CLI:**
```bash
aws iam create-role --role-name ecsTaskExecutionRole \
  --assume-role-policy-document file://ecs-trust-policy.json

aws iam attach-role-policy --role-name ecsTaskExecutionRole \
  --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy
```

**ecs-trust-policy.json:**
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "ecs-tasks.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
```

#### ECS Task Role (Optional)

Create IAM role `ecsTaskRole` for application-specific permissions:
- S3 bucket access
- DynamoDB table access
- Secrets Manager access
- etc.

### 2. VPC Configuration

ECS Fargate requires:
- **VPC** with at least 2 availability zones
- **Public or Private Subnets** (at least 2)
- **Security Group** with appropriate ingress/egress rules
- **Internet Gateway** (for public subnets) or **NAT Gateway** (for private subnets)

**Security Group Rules:**
- **Inbound**: Port 80 (HTTP) from ALB or 0.0.0.0/0
- **Outbound**: All traffic (for pulling images and external API calls)

### 3. ECR Repository

Create ECR repository to store Docker images:

```bash
aws ecr create-repository --repository-name orbitcomp --region us-east-1
```

---

## ECS Fargate Setup

### 1. CloudWatch Log Group

Create log group for application logs:

```bash
aws logs create-log-group --log-group-name /ecs/orbitcomp --region us-east-1
```

### 2. ECS Cluster

Create ECS cluster:

```bash
aws ecs create-cluster --cluster-name my-ecs-cluster --region us-east-1
```

### 3. Build and Push Docker Image

Use the provided build script:

**Linux/macOS:**
```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

**Windows:**
```cmd
scripts\build-push.bat
```

The script will:
1. Prompt for registry selection (AWS ECR or Docker Hub)
2. Prompt for registry credentials
3. Build Docker image
4. Push image to selected registry
5. Auto-create ECR repository if it doesn't exist

---

## ECS Task Definition Explained

### Valid Fargate CPU/Memory Combinations

**CRITICAL**: Only specific CPU and memory combinations are valid for Fargate:

| CPU (vCPU) | Valid Memory (MB) |
|------------|-------------------|
| 256 (.25)  | 512, 1024, 2048 |
| 512 (.5)   | 1024, 2048, 3072, 4096 |
| 1024 (1)   | 2048-8192 (increments of 1024) |
| 2048 (2)   | 4096-16384 (increments of 1024) |
| 4096 (4)   | 8192-30720 (increments of 1024) |

**Default Configuration**: CPU: "512", Memory: "1024"

### Task Definition Structure

```json
{
  "family": "orbitcomp-task",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "512",
  "memory": "1024",
  "executionRoleArn": "arn:aws:iam::ACCOUNT_ID:role/ecsTaskExecutionRole",
  "taskRoleArn": "arn:aws:iam::ACCOUNT_ID:role/ecsTaskRole",
  "containerDefinitions": [...]
}
```

### Container Definition

```json
{
  "name": "orbitcomp",
  "image": "IMAGE_URI",
  "essential": true,
  "portMappings": [
    {
      "containerPort": 80,
      "protocol": "tcp"
    }
  ],
  "environment": [
    {"name": "ASPNETCORE_ENVIRONMENT", "value": "Production"},
    {"name": "ASPNETCORE_URLS", "value": "http://+:80"}
  ],
  "logConfiguration": {
    "logDriver": "awslogs",
    "options": {
      "awslogs-group": "/ecs/orbitcomp",
      "awslogs-region": "us-east-1",
      "awslogs-stream-prefix": "ecs"
    }
  }
}
```

**Note**: Container health checks are optional. If the runtime image doesn't include curl/wget, omit the healthCheck and rely on ALB health checks.

---

## ECS Service Configuration

### Service Definition Structure

```json
{
  "serviceName": "orbitcomp-service",
  "cluster": "my-ecs-cluster",
  "taskDefinition": "orbitcomp-task",
  "desiredCount": 2,
  "launchType": "FARGATE",
  "networkConfiguration": {
    "awsvpcConfiguration": {
      "subnets": ["subnet-xxx", "subnet-yyy"],
      "securityGroups": ["sg-xxx"],
      "assignPublicIp": "ENABLED"
    }
  },
  "loadBalancers": [
    {
      "targetGroupArn": "arn:aws:elasticloadbalancing:...",
      "containerName": "orbitcomp",
      "containerPort": 80
    }
  ],
  "healthCheckGracePeriodSeconds": 300
}
```

### Key Configuration Options

- **desiredCount**: Number of task instances to run (default: 2)
- **assignPublicIp**: "ENABLED" for public subnets, "DISABLED" for private with NAT
- **healthCheckGracePeriodSeconds**: Time before health checks start (300s for .NET apps)
- **deploymentConfiguration**: Controls rolling update behavior

**CRITICAL**: Use `"tags"` parameter, NEVER use `"serviceTags"` (invalid and will cause deployment failure)

---

## ECS Fargate Deployment Walkthrough

### Step 1: Build and Push Docker Image

**Linux/macOS:**
```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

**Windows:**
```cmd
scripts\build-push.bat
```

**Expected Prompts:**
1. Select registry (1 for ECR, 2 for Docker Hub)
2. Enter AWS region (e.g., us-east-1)
3. Enter AWS Account ID
4. Enter ECR repository name (default: orbitcomp)
5. Enter image tag (default: latest)

**Script Actions:**
- Authenticates with selected registry
- Auto-creates ECR repository if it doesn't exist
- Builds Docker image with multi-stage optimization
- Pushes image to registry
- Displays full image URI for deployment

### Step 2: Deploy to ECS Fargate

**Linux/macOS:**
```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

**Windows:**
```cmd
scripts\deploy-image.bat
```

**Expected Prompts:**
1. Enter AWS region
2. Enter ECS cluster name
3. Enter VPC ID
4. Enter subnet IDs (comma-separated)
5. Enter security group ID
6. Enter Docker image URI (from Step 1)
7. Do you need a load balancer? (y/n)

**Script Actions:**
- Retrieves AWS Account ID automatically
- Creates ECS cluster if it doesn't exist
- Creates CloudWatch log group
- If load balancer selected:
  - Creates Application Load Balancer
  - Creates Target Group (target-type: ip for Fargate)
  - Creates ALB Listener on port 80
  - Configures health checks
- Registers task definition with provided configuration
- Creates or updates ECS service
- Waits for service to become stable
- Displays deployment summary and access URLs

### Step 3: Verify Deployment

1. **Check Service Status:**
   ```bash
   aws ecs describe-services --cluster my-ecs-cluster --services orbitcomp-service --region us-east-1
   ```

2. **View Running Tasks:**
   ```bash
   aws ecs list-tasks --cluster my-ecs-cluster --service-name orbitcomp-service --region us-east-1
   ```

3. **Check CloudWatch Logs:**
   ```bash
   aws logs tail /ecs/orbitcomp --follow --region us-east-1
   ```

4. **Access Application:**
   - If using ALB: http://ALB_DNS_NAME
   - If using public IP: http://TASK_PUBLIC_IP
   - Health Check: http://ALB_DNS_NAME/health

### Step 4: Monitor Deployment

**ECS Console:**
```
https://console.aws.amazon.com/ecs/v2/clusters/my-ecs-cluster/services/orbitcomp-service/health?region=us-east-1
```

**CloudWatch Logs Console:**
```
https://console.aws.amazon.com/cloudwatch/home?region=us-east-1#logsV2:log-groups/log-group/$252Fecs$252Forbitcomp
```

---

## Configuration Management

### Environment Variables

Manage environment variables in `ecs/task-definition.json`:

```json
"environment": [
  {"name": "ASPNETCORE_ENVIRONMENT", "value": "Production"},
  {"name": "ASPNETCORE_URLS", "value": "http://+:80"},
  {"name": "ConnectionStrings__DefaultConnection", "value": "Server=..."}
]
```

### Secrets Management

For sensitive data, use AWS Secrets Manager:

```json
"secrets": [
  {
    "name": "DB_PASSWORD",
    "valueFrom": "arn:aws:secretsmanager:us-east-1:ACCOUNT_ID:secret:db-password"
  }
]
```

**Create Secret:**
```bash
aws secretsmanager create-secret --name db-password --secret-string "MySecretPassword" --region us-east-1
```

**Grant Access** - Add to `ecsTaskExecutionRole`:
```json
{
  "Effect": "Allow",
  "Action": [
    "secretsmanager:GetSecretValue",
    "kms:Decrypt"
  ],
  "Resource": "arn:aws:secretsmanager:us-east-1:ACCOUNT_ID:secret:db-password*"
}
```

### Application Settings

**appsettings.Production.json** can be mounted as a volume or baked into the Docker image.

**Option 1: Bake into Image** (Recommended for immutable deployments)
- Include `appsettings.Production.json` in source code
- Dockerfile copies it during build

**Option 2: Mount from S3** (For dynamic configuration)
- Store config in S3 bucket
- Use ECS Volumes with EFS or S3
- Mount at container runtime

---

## Security Considerations

### 1. Non-Root User

The Dockerfile creates and uses a non-root user (`appuser`) for enhanced security:

```dockerfile
RUN groupadd -r appuser && useradd -r -g appuser appuser
USER appuser
```

### 2. Network Security

- **Security Groups**: Restrict inbound traffic to only necessary ports
- **Private Subnets**: Use private subnets with NAT Gateway for enhanced isolation
- **VPC Endpoints**: Use VPC endpoints for AWS services (ECR, CloudWatch, Secrets Manager)

### 3. IAM Least Privilege

- Grant only necessary permissions to ECS task roles
- Use separate roles for execution and task permissions
- Regularly audit IAM policies

### 4. Secrets Management

- **NEVER** hardcode secrets in Dockerfile or source code
- Use AWS Secrets Manager or Parameter Store
- Rotate secrets regularly
- Use encryption at rest and in transit

### 5. Container Image Security

- Use official Microsoft base images
- Regularly update base images for security patches
- Scan images for vulnerabilities (AWS ECR scanning)
- Use specific image tags (not `latest` in production)

### 6. HTTPS/TLS

- Use Application Load Balancer with ACM certificate for HTTPS
- Configure HTTPS redirection in ALB listener rules
- Enable HTTP Strict Transport Security (HSTS)

---

## ECS-Specific Troubleshooting

### Task Failures

**Symptom**: Tasks fail to start or stop immediately

**Common Causes**:
1. **Invalid CPU/Memory Combination**
   - Check task definition uses valid Fargate combinations
   - Default safe values: cpu: "512", memory: "1024"

2. **Image Pull Errors**
   - Verify `executionRoleArn` has ECR permissions
   - Check ECR repository permissions
   - Verify image URI is correct

3. **Application Errors**
   - Check CloudWatch Logs: `aws logs tail /ecs/orbitcomp --follow`
   - Review application startup logs
   - Verify environment variables are correct

**Debug Commands**:
```bash
# Describe task to see failure reason
aws ecs describe-tasks --cluster my-ecs-cluster --tasks TASK_ARN --region us-east-1

# Check stopped tasks
aws ecs list-tasks --cluster my-ecs-cluster --desired-status STOPPED --region us-east-1
```

### Network Issues

**Symptom**: Cannot access application or tasks cannot reach external services

**Common Causes**:
1. **Security Group Configuration**
   - Verify inbound rules allow traffic on port 80
   - Verify outbound rules allow ALL traffic (0.0.0.0/0)

2. **Subnet Configuration**
   - Public subnets require Internet Gateway
   - Private subnets require NAT Gateway
   - Verify route tables are correct

3. **Load Balancer Configuration**
   - Verify target group health checks are passing
   - Check target group target-type is `ip` (required for Fargate)
   - Verify security group allows traffic from ALB

**Debug Commands**:
```bash
# Check target health
aws elbv2 describe-target-health --target-group-arn TARGET_GROUP_ARN --region us-east-1

# Check security group rules
aws ec2 describe-security-groups --group-ids sg-xxx --region us-east-1
```

### Service Update Failures

**Symptom**: Service fails to update or deploy new task definition

**Common Causes**:
1. **Deployment Circuit Breaker**
   - Tasks fail health checks repeatedly
   - Circuit breaker triggers rollback

2. **Insufficient Resources**
   - Not enough capacity in subnets
   - IP address exhaustion

**Resolution**:
```bash
# Force new deployment
aws ecs update-service --cluster my-ecs-cluster --service orbitcomp-service --force-new-deployment --region us-east-1

# Describe service events
aws ecs describe-services --cluster my-ecs-cluster --services orbitcomp-service --region us-east-1 | jq '.services[0].events'
```

### CloudWatch Logs Not Appearing

**Common Causes**:
1. **Missing IAM Permissions**
   - Verify `executionRoleArn` has `AmazonECSTaskExecutionRolePolicy`

2. **Log Group Doesn't Exist**
   - Create log group: `aws logs create-log-group --log-group-name /ecs/orbitcomp`

3. **Incorrect logConfiguration**
   - Verify region matches deployment region
   - Verify log group name is correct

---

## ECS Fargate Scaling and Management

### Manual Scaling

Update desired count:

```bash
aws ecs update-service --cluster my-ecs-cluster --service orbitcomp-service --desired-count 4 --region us-east-1
```

### Auto Scaling

**1. Create Scaling Target:**
```bash
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --scalable-dimension ecs:service:DesiredCount \
  --resource-id service/my-ecs-cluster/orbitcomp-service \
  --min-capacity 2 \
  --max-capacity 10 \
  --region us-east-1
```

**2. Create Scaling Policy (Target Tracking):**
```bash
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --scalable-dimension ecs:service:DesiredCount \
  --resource-id service/my-ecs-cluster/orbitcomp-service \
  --policy-name cpu-scaling-policy \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration file://scaling-policy.json \
  --region us-east-1
```

**scaling-policy.json:**
```json
{
  "TargetValue": 70.0,
  "PredefinedMetricSpecification": {
    "PredefinedMetricType": "ECSServiceAverageCPUUtilization"
  },
  "ScaleInCooldown": 300,
  "ScaleOutCooldown": 60
}
```

### Blue/Green Deployments

ECS supports blue/green deployments with AWS CodeDeploy:

1. Create CodeDeploy application and deployment group
2. Configure ALB with two target groups (blue and green)
3. Use CodeDeploy to manage traffic shifting

**Benefits**:
- Zero-downtime deployments
- Automatic rollback on failure
- Gradual traffic shifting

### Rolling Updates

By default, ECS performs rolling updates:

```json
"deploymentConfiguration": {
  "maximumPercent": 200,
  "minimumHealthyPercent": 50
}
```

- **maximumPercent**: Maximum tasks during deployment (200% = 2x capacity)
- **minimumHealthyPercent**: Minimum healthy tasks (50% = half capacity)

---

## Technology-Specific Notes

### .NET 6.0 ASP.NET Core

**1. Startup Performance:**
- .NET applications may take 30-60 seconds to start
- Set `healthCheckGracePeriodSeconds: 300` in service definition
- Configure longer health check start periods

**2. Graceful Shutdown:**
The Dockerfile uses `dotnet` entrypoint which handles SIGTERM correctly:
```dockerfile
ENTRYPOINT ["dotnet", "OrbitComp.dll"]
```

**3. Logging Configuration:**
- Use Serilog or built-in logging providers
- Configure console logging for CloudWatch
- Set appropriate log levels in `appsettings.Production.json`

**4. Memory Management:**
- .NET GC behavior differs in containers
- Set appropriate memory limits in task definition
- Monitor memory usage in CloudWatch Container Insights

**5. Culture and Timezone:**
Configured in Dockerfile:
```dockerfile
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=America/New_York
```

**6. Health Checks:**
Implement ASP.NET Core health checks:
```csharp
services.AddHealthChecks();
app.MapHealthChecks("/health");
```

**7. Application Insights:**
Add Application Insights for monitoring:
```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

Configure in `appsettings.Production.json`:
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here"
  }
}
```

---

## Additional Resources

- **AWS ECS Documentation**: https://docs.aws.amazon.com/ecs/
- **AWS Fargate Documentation**: https://docs.aws.amazon.com/AmazonECS/latest/developerguide/AWS_Fargate.html
- **.NET Docker Images**: https://hub.docker.com/_/microsoft-dotnet
- **ASP.NET Core Documentation**: https://docs.microsoft.com/aspnet/core
- **AWS CLI Reference**: https://docs.aws.amazon.com/cli/

---

## Support and Troubleshooting

For additional support:
1. Check CloudWatch Logs for application errors
2. Review ECS service events in AWS Console
3. Verify all prerequisites are met
4. Consult AWS documentation for platform-specific issues
5. Review application logs for .NET-specific errors

---

**Document Version**: 1.0  
**Last Updated**: 2025-12-17  
**Target Platform**: AWS ECS Fargate  
**Application**: OrbitComp (.NET 6.0 ASP.NET Core)
