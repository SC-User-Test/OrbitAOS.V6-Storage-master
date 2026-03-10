# OrbitAOS.V6 - AWS ECS Fargate Deployment Guide

This guide provides comprehensive instructions for deploying the OrbitAOS.V6 ASP.NET Core 6.0 application to AWS ECS Fargate.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Project Overview](#project-overview)
3. [Local Development](#local-development)
4. [Docker Build and Push](#docker-build-and-push)
5. [AWS ECS Fargate Prerequisites](#aws-ecs-fargate-prerequisites)
6. [AWS ECS Deployment](#aws-ecs-deployment)
7. [Configuration Management](#configuration-management)
8. [Monitoring and Logging](#monitoring-and-logging)
9. [Troubleshooting](#troubleshooting)
10. [Security Considerations](#security-considerations)

---

## Prerequisites

### Required Tools

- **Docker** (v20.10+): For building container images
- **AWS CLI** (v2.x): For AWS resource management
- **.NET 6.0 SDK**: For local development and testing
- **Git**: For version control

### AWS Account Requirements

- Active AWS account with appropriate permissions
- IAM user with ECS, ECR, VPC, and CloudWatch permissions
- AWS CLI configured with access credentials

### Verify Prerequisites

```bash
# Check Docker installation
docker --version

# Check AWS CLI installation and configuration
aws --version
aws sts get-caller-identity

# Check .NET SDK
dotnet --version
```

---

## Project Overview

### Technology Stack

- **Framework**: ASP.NET Core 6.0
- **Project Type**: Web Application with MVC and Identity
- **Database**: SQL Server (via Entity Framework Core)
- **Authentication**: ASP.NET Core Identity
- **Health Checks**: Built-in health check endpoint at `/health`

### Application Structure

```
OrbitAOS.V6/
├── Controllers/         # MVC Controllers
├── Models/             # Data models
├── Views/              # Razor views
├── Data/               # Entity Framework DbContext
├── Areas/              # Identity UI areas
├── wwwroot/            # Static files
├── appsettings.json    # Application configuration
└── Program.cs          # Application entry point
```

### Key Features

- ASP.NET Core Identity for authentication
- Entity Framework Core with SQL Server
- Health check endpoint for container orchestration
- Environment-based configuration
- Logging and diagnostics

---

## Local Development

### Running Locally

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd compTestOrbit
   ```

2. **Configure database connection**:
   Edit `OrbitAOS.V6/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=OrbitAOS;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Run database migrations**:
   ```bash
   cd OrbitAOS.V6
   dotnet ef database update
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

5. **Access the application**:
   - HTTP: `http://localhost:5008`
   - HTTPS: `https://localhost:7073`
   - Health check: `http://localhost:5008/health`

### Running with Docker Compose

```bash
# Set required environment variables
export DB_SERVER=your-db-server
export DB_NAME=OrbitAOS
export DB_USER=your-user
export DB_PASSWORD=your-password

# Start the application
docker-compose up -d

# View logs
docker-compose logs -f

# Stop the application
docker-compose down
```

Access the application at `http://localhost:8080`

---

## Docker Build and Push

### Building the Docker Image

The project includes scripts for building and pushing Docker images to AWS ECR or Docker Hub.

#### Linux/macOS

```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

#### Windows

```cmd
scripts\build-push.bat
```

### Script Workflow

1. **Registry Selection**: Choose between AWS ECR or Docker Hub
2. **Authentication**: Automatically authenticate with the selected registry
3. **Repository Creation**: For ECR, automatically creates the repository if it doesn't exist
4. **Image Build**: Builds the Docker image using the multi-stage Dockerfile
5. **Image Push**: Pushes the image to the selected registry
6. **Tag Handling**: Sanitizes and validates image tags

### AWS ECR Example

```bash
# The script will prompt for:
# - AWS Region: us-east-1
# - AWS Account ID: 123456789012
# - ECR Repository Name: orbitaos-v6
# - Image Tag: v1.0.0

# Resulting image URI:
# 123456789012.dkr.ecr.us-east-1.amazonaws.com/orbitaos-v6:v1.0.0
```

### Docker Hub Example

```bash
# The script will prompt for:
# - Docker Hub Username: myusername
# - Docker Hub Password: <your-token>
# - Image Tag: v1.0.0

# Resulting image:
# myusername/orbitaos-v6:v1.0.0
```

---

## AWS ECS Fargate Prerequisites

### 1. VPC and Networking

Create or identify a VPC with the following:

#### VPC Configuration
- **CIDR Block**: 10.0.0.0/16 (or your preferred range)
- **DNS Resolution**: Enabled
- **DNS Hostnames**: Enabled

#### Subnets
- **Public Subnets**: At least 2 in different availability zones
- **CIDR Examples**: 10.0.1.0/24, 10.0.2.0/24
- **Auto-assign Public IP**: Enabled

#### Internet Gateway
- Attached to the VPC
- Route table configured for public subnets (0.0.0.0/0 → IGW)

#### Security Group

Create a security group with the following rules:

**Inbound Rules**:
- HTTP (80) from 0.0.0.0/0 (or ALB security group)
- HTTPS (443) from 0.0.0.0/0 (optional)

**Outbound Rules**:
- All traffic to 0.0.0.0/0 (for external dependencies)

### 2. IAM Roles

#### ECS Task Execution Role

Create IAM role `ecsTaskExecutionRole` with the following policy:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "ecr:GetAuthorizationToken",
        "ecr:BatchCheckLayerAvailability",
        "ecr:GetDownloadUrlForLayer",
        "ecr:BatchGetImage",
        "logs:CreateLogStream",
        "logs:PutLogEvents",
        "logs:CreateLogGroup"
      ],
      "Resource": "*"
    }
  ]
}
```

Trust relationship:
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

Create IAM role `ecsTaskRole` for application-specific permissions (e.g., S3, DynamoDB access).

### 3. Database Setup

#### Amazon RDS SQL Server

1. **Create RDS Instance**:
   - Engine: Microsoft SQL Server
   - Version: SQL Server 2019 or higher
   - Instance class: db.t3.medium (or appropriate size)
   - Storage: 20 GB (minimum)
   - VPC: Same VPC as ECS tasks
   - Subnet group: Private subnets
   - Publicly accessible: No
   - Security group: Allow inbound on port 1433 from ECS security group

2. **Create Database**:
   ```sql
   CREATE DATABASE OrbitAOS;
   ```

3. **Note Connection Details**:
   - Endpoint: `mydb.abc123.us-east-1.rds.amazonaws.com`
   - Port: 1433
   - Username: `admin`
   - Password: `<your-password>`

### 4. CloudWatch Log Group

The deployment script automatically creates the log group, but you can create it manually:

```bash
aws logs create-log-group \
  --log-group-name /ecs/orbitaos-v6 \
  --region us-east-1
```

---

## AWS ECS Deployment

### Deployment Script

The project includes deployment scripts for AWS ECS Fargate.

#### Linux/macOS

```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

#### Windows

```cmd
scripts\deploy-image.bat
```

### Deployment Workflow

1. **Prompts for Configuration**:
   - AWS region
   - ECS cluster name
   - Docker image URI
   - VPC and networking details
   - Database connection details
   - Load balancer requirements

2. **Resource Creation**:
   - Creates ECS cluster if it doesn't exist
   - Creates Application Load Balancer (if requested)
   - Creates Target Group with health checks
   - Configures listener for HTTP traffic

3. **Task and Service Deployment**:
   - Registers ECS task definition
   - Creates or updates ECS service
   - Waits for service stability
   - Displays deployment summary

### Manual Deployment Steps

#### Step 1: Register Task Definition

```bash
# Replace placeholders in task definition
cp ecs/task-definition.json /tmp/task-def.json

# Edit the file to replace:
# - {{IMAGE_URI}}
# - {{AWS_REGION}}
# - {{ACCOUNT_ID}}
# - {{DB_SERVER}}, {{DB_NAME}}, {{DB_USER}}, {{DB_PASSWORD}}

# Register task definition
aws ecs register-task-definition \
  --cli-input-json file:///tmp/task-def.json \
  --region us-east-1
```

#### Step 2: Create or Update Service

**Create New Service**:
```bash
# Replace placeholders in service definition
cp ecs/service-definition.json /tmp/service-def.json

# Edit the file to replace:
# - {{CLUSTER_NAME}}
# - {{SUBNET_1}}, {{SUBNET_2}}
# - {{SECURITY_GROUP}}
# - {{TARGET_GROUP_ARN}}

# Create service
aws ecs create-service \
  --cli-input-json file:///tmp/service-def.json \
  --region us-east-1
```

**Update Existing Service**:
```bash
aws ecs update-service \
  --cluster my-cluster \
  --service orbitaos-v6-service \
  --task-definition orbitaos-v6-task:1 \
  --desired-count 2 \
  --region us-east-1
```

#### Step 3: Monitor Deployment

```bash
# Wait for service to stabilize
aws ecs wait services-stable \
  --cluster my-cluster \
  --services orbitaos-v6-service \
  --region us-east-1

# Check service status
aws ecs describe-services \
  --cluster my-cluster \
  --services orbitaos-v6-service \
  --region us-east-1
```

---

## Configuration Management

### Environment Variables

The application uses environment variables for configuration:

| Variable | Description | Example |
|----------|-------------|----------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `ASPNETCORE_URLS` | Kestrel binding URLs | `http://+:80` |
| `DB_SERVER` | Database server hostname | `mydb.abc123.us-east-1.rds.amazonaws.com` |
| `DB_NAME` | Database name | `OrbitAOS` |
| `DB_USER` | Database username | `admin` |
| `DB_PASSWORD` | Database password | `SecurePassword123!` |

### Connection String Format

The application builds connection strings from environment variables:

```
Server=${DB_SERVER};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD};MultipleActiveResultSets=true;TrustServerCertificate=true
```

### Secrets Management

For production deployments, use AWS Secrets Manager:

1. **Store Database Password**:
   ```bash
   aws secretsmanager create-secret \
     --name orbitaos-v6/db-password \
     --secret-string "SecurePassword123!" \
     --region us-east-1
   ```

2. **Update Task Definition**:
   ```json
   {
     "name": "DB_PASSWORD",
     "valueFrom": "arn:aws:secretsmanager:us-east-1:123456789012:secret:orbitaos-v6/db-password"
   }
   ```

3. **Update Task Execution Role**:
   Add permissions to retrieve secrets:
   ```json
   {
     "Effect": "Allow",
     "Action": [
       "secretsmanager:GetSecretValue"
     ],
     "Resource": "arn:aws:secretsmanager:us-east-1:123456789012:secret:orbitaos-v6/*"
   }
   ```

---

## Monitoring and Logging

### CloudWatch Logs

**View Logs**:
```bash
# Tail logs in real-time
aws logs tail /ecs/orbitaos-v6 --follow --region us-east-1

# View logs from the last hour
aws logs tail /ecs/orbitaos-v6 --since 1h --region us-east-1

# Filter logs by pattern
aws logs tail /ecs/orbitaos-v6 --filter-pattern "ERROR" --region us-east-1
```

### CloudWatch Metrics

Monitor ECS metrics in CloudWatch:

- **CPU Utilization**: `AWS/ECS` → `CPUUtilization`
- **Memory Utilization**: `AWS/ECS` → `MemoryUtilization`
- **Running Tasks Count**: `AWS/ECS` → `RunningTasksCount`

### Health Checks

The application provides a health check endpoint:

- **URL**: `/health`
- **Success Response**: 200 OK with "Healthy" status
- **Checks**: Database connectivity via Entity Framework

**Test Health Endpoint**:
```bash
curl http://<alb-dns>/health
```

### Application Insights (Optional)

For enhanced monitoring, integrate Application Insights:

1. **Install NuGet Package**:
   ```bash
   dotnet add package Microsoft.ApplicationInsights.AspNetCore
   ```

2. **Configure in Program.cs**:
   ```csharp
   builder.Services.AddApplicationInsightsTelemetry();
   ```

3. **Set Instrumentation Key**:
   ```json
   {
     "ApplicationInsights": {
       "InstrumentationKey": "your-key-here"
     }
   }
   ```

---

## Troubleshooting

### Common Issues

#### 1. Task Fails to Start

**Symptoms**: Tasks repeatedly start and stop

**Possible Causes**:
- Invalid CPU/memory combination
- Image pull errors
- Health check failures
- Application crashes

**Solutions**:
```bash
# Check stopped tasks
aws ecs list-tasks \
  --cluster my-cluster \
  --service-name orbitaos-v6-service \
  --desired-status STOPPED \
  --region us-east-1

# Describe stopped task
aws ecs describe-tasks \
  --cluster my-cluster \
  --tasks <task-arn> \
  --region us-east-1

# Check CloudWatch logs for application errors
aws logs tail /ecs/orbitaos-v6 --since 30m --region us-east-1
```

#### 2. Database Connection Issues

**Symptoms**: Application logs show database connection errors

**Solutions**:
- Verify security group allows traffic from ECS tasks to RDS on port 1433
- Check database credentials in environment variables
- Ensure RDS instance is in the same VPC
- Test connectivity from ECS task:
  ```bash
  # Execute command in running task
  aws ecs execute-command \
    --cluster my-cluster \
    --task <task-id> \
    --container orbitaos-v6 \
    --command "/bin/sh" \
    --interactive
  ```

#### 3. Load Balancer Health Check Failures

**Symptoms**: Target group shows unhealthy targets

**Solutions**:
- Verify health check path is `/health`
- Check application is listening on port 80
- Increase health check grace period
- Review application logs for startup issues

#### 4. Invalid CPU/Memory Combination

**Error**: "Invalid CPU or memory value specified"

**Solution**: Use valid Fargate combinations:
- CPU: 512 → Memory: 1024, 2048
- CPU: 1024 → Memory: 2048, 3072, 4096
- CPU: 2048 → Memory: 4096-16384 (increments of 1024)

#### 5. Image Pull Errors

**Symptoms**: Tasks fail with "CannotPullContainerError"

**Solutions**:
- Verify image URI is correct
- Ensure task execution role has ECR permissions
- Check image exists in ECR:
  ```bash
  aws ecr describe-images \
    --repository-name orbitaos-v6 \
    --region us-east-1
  ```

### Debug Commands

```bash
# List all tasks
aws ecs list-tasks --cluster my-cluster --region us-east-1

# Describe service
aws ecs describe-services \
  --cluster my-cluster \
  --services orbitaos-v6-service \
  --region us-east-1

# View service events
aws ecs describe-services \
  --cluster my-cluster \
  --services orbitaos-v6-service \
  --region us-east-1 \
  --query 'services[0].events[0:10]'

# Check target health
aws elbv2 describe-target-health \
  --target-group-arn <target-group-arn> \
  --region us-east-1
```

---

## Security Considerations

### 1. Container Security

- **Non-root User**: Dockerfile creates and uses a non-root user (`appuser`)
- **Minimal Base Image**: Uses Alpine-based runtime image
- **No Secrets in Image**: All sensitive data passed via environment variables

### 2. Network Security

- **Private Subnets**: Consider using private subnets for ECS tasks with NAT Gateway
- **Security Groups**: Restrict inbound traffic to necessary ports only
- **VPC Endpoints**: Use VPC endpoints for ECR and S3 to avoid internet traffic

### 3. IAM Security

- **Least Privilege**: Task execution and task roles have minimal required permissions
- **Role Separation**: Separate roles for task execution vs. application permissions
- **Credential Rotation**: Rotate AWS credentials regularly

### 4. Data Security

- **Encryption in Transit**: Use HTTPS for all external communication
- **Encryption at Rest**: Enable encryption for RDS and EBS volumes
- **Secrets Manager**: Store sensitive data in AWS Secrets Manager
- **Connection String Encryption**: Use TrustServerCertificate=false in production

### 5. Application Security

- **HTTPS Redirection**: Enable HTTPS redirection in production
- **Security Headers**: Add security headers middleware
- **CORS Configuration**: Configure CORS policies appropriately
- **Authentication**: Use ASP.NET Core Identity with strong password policies

---

## Scaling and Performance

### Auto Scaling

**Configure Service Auto Scaling**:
```bash
# Register scalable target
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --resource-id service/my-cluster/orbitaos-v6-service \
  --scalable-dimension ecs:service:DesiredCount \
  --min-capacity 2 \
  --max-capacity 10 \
  --region us-east-1

# Create scaling policy
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --resource-id service/my-cluster/orbitaos-v6-service \
  --scalable-dimension ecs:service:DesiredCount \
  --policy-name cpu-scaling-policy \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration '{
    "TargetValue": 70.0,
    "PredefinedMetricSpecification": {
      "PredefinedMetricType": "ECSServiceAverageCPUUtilization"
    },
    "ScaleInCooldown": 300,
    "ScaleOutCooldown": 60
  }' \
  --region us-east-1
```

### Performance Optimization

1. **Response Caching**: Enable response caching for static content
2. **Database Connection Pooling**: Configure appropriate connection pool sizes
3. **ReadyToRun Images**: Consider using ReadyToRun for faster startup
4. **Task CPU/Memory**: Monitor and adjust based on actual usage

---

## Maintenance

### Update Application

1. Build and push new image with a new tag
2. Update task definition with new image URI
3. Update service to use new task definition
4. ECS will perform a rolling update

### Database Migrations

Run migrations during deployment:

```bash
# Option 1: Run migration task
aws ecs run-task \
  --cluster my-cluster \
  --task-definition orbitaos-v6-task \
  --overrides '{
    "containerOverrides": [{
      "name": "orbitaos-v6",
      "command": ["dotnet", "ef", "database", "update"]
    }]
  }' \
  --region us-east-1

# Option 2: Include migration in startup (Program.cs)
# Add this code to automatically run migrations:
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}
```

---

## Additional Resources

- [AWS ECS Documentation](https://docs.aws.amazon.com/ecs/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [AWS Fargate Pricing](https://aws.amazon.com/fargate/pricing/)

---

## Support

For issues or questions:
1. Check CloudWatch logs for application errors
2. Review ECS service events
3. Consult AWS ECS troubleshooting guide
4. Contact your DevOps team or AWS support

---

**Document Version**: 1.0  
**Last Updated**: 2026-03-10  
**Deployment Target**: AWS ECS Fargate