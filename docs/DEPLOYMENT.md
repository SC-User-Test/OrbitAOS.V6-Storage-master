# CompTestOrbit001 - AWS ECS Fargate Deployment Guide

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Local Development Setup](#local-development-setup)
4. [Docker Deployment](#docker-deployment)
5. [AWS ECS Fargate Prerequisites](#aws-ecs-fargate-prerequisites)
6. [ECS Fargate Setup](#ecs-fargate-setup)
7. [ECS Task Definition Explained](#ecs-task-definition-explained)
8. [ECS Service Configuration](#ecs-service-configuration)
9. [Deployment Walkthrough](#deployment-walkthrough)
10. [Troubleshooting](#troubleshooting)
11. [Scaling and Management](#scaling-and-management)
12. [Security Considerations](#security-considerations)

---

## Overview

This guide provides comprehensive instructions for deploying the **CompTestOrbit001** ASP.NET Core 8.0 application to AWS ECS Fargate. The application is containerized using Docker and deployed using AWS ECS with Fargate launch type for serverless container orchestration.

**Application Details:**
- **Framework**: ASP.NET Core 8.0
- **Application Type**: Web API
- **Application Port**: 8080
- **Health Endpoint**: /health
- **Target Platform**: AWS ECS Fargate

---

## Prerequisites

### Required Software
1. **Docker Desktop** (version 20.10 or later)
   - Download: https://www.docker.com/products/docker-desktop
   - Verify installation: `docker --version`

2. **AWS CLI** (version 2.x or later)
   - Download: https://aws.amazon.com/cli/
   - Verify installation: `aws --version`
   - Configure credentials: `aws configure`

3. **.NET SDK** (version 8.0 or later) - for local development
   - Download: https://dotnet.microsoft.com/download
   - Verify installation: `dotnet --version`

### AWS Account Requirements
1. Active AWS account with appropriate permissions
2. IAM user with programmatic access (Access Key ID and Secret Access Key)
3. Required IAM permissions:
   - ECS full access (ecs:*)
   - ECR full access (ecr:*)
   - IAM role creation (iam:CreateRole, iam:AttachRolePolicy)
   - VPC and networking (ec2:DescribeVpcs, ec2:DescribeSubnets, ec2:DescribeSecurityGroups)
   - CloudWatch Logs (logs:CreateLogGroup, logs:CreateLogStream, logs:PutLogEvents)
   - Elastic Load Balancing (elasticloadbalancing:*)

---

## Local Development Setup

### Running the Application Locally

1. **Navigate to project directory**:
   ```bash
   cd /modernize-data/studio-data/TNT1001/APP1319/transformed-code/436/studio-workspace/CompTestOrbit001
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build the application**:
   ```bash
   dotnet build -c Release
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

5. **Test the application**:
   - Application: http://localhost:8080
   - Health endpoint: http://localhost:8080/health

---

## Docker Deployment

### Building the Docker Image Locally

1. **Build Docker image**:
   ```bash
   docker build -t comptestorbit001:latest .
   ```

2. **Run container locally**:
   ```bash
   docker run -d -p 8080:8080 --name comptestorbit001-app comptestorbit001:latest
   ```

3. **Test containerized application**:
   ```bash
   curl http://localhost:8080/health
   ```

4. **View container logs**:
   ```bash
   docker logs comptestorbit001-app
   ```

5. **Stop and remove container**:
   ```bash
   docker stop comptestorbit001-app
   docker rm comptestorbit001-app
   ```

### Using Docker Compose

1. **Start application with Docker Compose**:
   ```bash
   docker-compose up -d
   ```

2. **View logs**:
   ```bash
   docker-compose logs -f
   ```

3. **Stop application**:
   ```bash
   docker-compose down
   ```

---

## AWS ECS Fargate Prerequisites

### 1. Configure AWS CLI

```bash
aws configure
```

Provide:
- AWS Access Key ID
- AWS Secret Access Key
- Default region (e.g., us-east-1)
- Default output format (json)

### 2. Create IAM Roles

#### ECS Task Execution Role

This role allows ECS to pull container images from ECR and send logs to CloudWatch.

```bash
# Create trust policy document
cat > ecs-task-execution-trust-policy.json <<EOF
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
EOF

# Create IAM role
aws iam create-role \
  --role-name ecsTaskExecutionRole \
  --assume-role-policy-document file://ecs-task-execution-trust-policy.json

# Attach AWS managed policy
aws iam attach-role-policy \
  --role-name ecsTaskExecutionRole \
  --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy
```

#### ECS Task Role (Optional)

This role provides permissions for your application to access other AWS services.

```bash
# Create task role
aws iam create-role \
  --role-name ecsTaskRole \
  --assume-role-policy-document file://ecs-task-execution-trust-policy.json

# Attach custom policies as needed
# Example: Allow S3 access
aws iam attach-role-policy \
  --role-name ecsTaskRole \
  --policy-arn arn:aws:iam::aws:policy/AmazonS3ReadOnlyAccess
```

### 3. Configure VPC and Networking

#### Create Security Group

```bash
# Get default VPC ID
VPC_ID=$(aws ec2 describe-vpcs --filters "Name=isDefault,Values=true" --query "Vpcs[0].VpcId" --output text)

# Create security group
SG_ID=$(aws ec2 create-security-group \
  --group-name comptestorbit001-sg \
  --description "Security group for CompTestOrbit001 ECS service" \
  --vpc-id $VPC_ID \
  --query 'GroupId' \
  --output text)

# Allow inbound traffic on port 8080
aws ec2 authorize-security-group-ingress \
  --group-id $SG_ID \
  --protocol tcp \
  --port 8080 \
  --cidr 0.0.0.0/0

# Allow inbound traffic on port 80 (for ALB)
aws ec2 authorize-security-group-ingress \
  --group-id $SG_ID \
  --protocol tcp \
  --port 80 \
  --cidr 0.0.0.0/0

echo "Security Group ID: $SG_ID"
```

#### Get Subnet IDs

```bash
# List available subnets in default VPC
aws ec2 describe-subnets \
  --filters "Name=vpc-id,Values=$VPC_ID" \
  --query "Subnets[*].[SubnetId,AvailabilityZone,CidrBlock]" \
  --output table

# Select at least 2 subnets in different availability zones
```

### 4. Create CloudWatch Log Group

```bash
aws logs create-log-group --log-group-name /ecs/comptestorbit001

aws logs put-retention-policy \
  --log-group-name /ecs/comptestorbit001 \
  --retention-in-days 7
```

---

## ECS Fargate Setup

### Understanding ECS Fargate

AWS Fargate is a serverless compute engine for containers that removes the need to provision and manage servers. Key benefits:

- **Serverless**: No EC2 instances to manage
- **Automatic Scaling**: Scale based on demand
- **Pay-per-use**: Only pay for resources consumed
- **Integrated**: Works seamlessly with ECS, ECR, CloudWatch

### Fargate Resource Requirements

**Valid CPU and Memory Combinations** (task-level):

| CPU (vCPU) | Memory (MB) Options |
|------------|---------------------|
| 0.25 (256) | 512, 1024, 2048 |
| 0.5 (512)  | 1024, 2048, 3072, 4096 |
| 1 (1024)   | 2048-8192 (increments of 1024) |
| 2 (2048)   | 4096-16384 (increments of 1024) |
| 4 (4096)   | 8192-30720 (increments of 1024) |

**Default Configuration for this application**:
- CPU: 512 (0.5 vCPU)
- Memory: 1024 MB (1 GB)

---

## ECS Task Definition Explained

The task definition is a blueprint for your application. Key components:

### Launch Type Configuration
```json
"requiresCompatibilities": ["FARGATE"],
"networkMode": "awsvpc"
```
- **requiresCompatibilities**: Specifies Fargate launch type
- **networkMode**: awsvpc is required for Fargate (provides each task with its own ENI)

### CPU and Memory
```json
"cpu": "512",
"memory": "1024"
```
- Task-level resource allocation
- Must use valid Fargate combinations

### Container Definitions
```json
"containerDefinitions": [{
  "name": "comptestorbit001",
  "image": "{{IMAGE_URI}}",
  "essential": true,
  "portMappings": [{"containerPort": 8080, "protocol": "tcp"}]
}]
```
- **name**: Container identifier
- **image**: ECR or Docker Hub image URI
- **essential**: If true, task stops if container stops
- **portMappings**: Only containerPort needed for Fargate

### Logging Configuration
```json
"logConfiguration": {
  "logDriver": "awslogs",
  "options": {
    "awslogs-group": "/ecs/comptestorbit001",
    "awslogs-region": "us-east-1",
    "awslogs-stream-prefix": "ecs"
  }
}
```
- Sends container logs to CloudWatch Logs
- Log stream format: ecs/{container-name}/{task-id}

### Health Check
```json
"healthCheck": {
  "command": ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"],
  "interval": 30,
  "timeout": 5,
  "retries": 3,
  "startPeriod": 60
}
```
- **startPeriod**: Grace period for application startup
- **interval**: Time between health checks
- **timeout**: Time to wait for health check response
- **retries**: Consecutive failures before unhealthy

---

## ECS Service Configuration

The service definition manages running tasks and integrates with load balancers.

### Service Parameters
```json
"desiredCount": 2,
"launchType": "FARGATE"
```
- **desiredCount**: Number of tasks to run (high availability: >= 2)
- **launchType**: Fargate for serverless execution

### Network Configuration
```json
"networkConfiguration": {
  "awsvpcConfiguration": {
    "subnets": ["subnet-xxx", "subnet-yyy"],
    "securityGroups": ["sg-xxx"],
    "assignPublicIp": "ENABLED"
  }
}
```
- **subnets**: Must span at least 2 availability zones
- **securityGroups**: Control inbound/outbound traffic
- **assignPublicIp**: ENABLED for internet access (without NAT Gateway)

### Deployment Configuration
```json
"deploymentConfiguration": {
  "maximumPercent": 200,
  "minimumHealthyPercent": 50,
  "deploymentCircuitBreaker": {
    "enable": true,
    "rollback": true
  }
}
```
- **maximumPercent**: Max tasks during deployment (200 = 2x desired count)
- **minimumHealthyPercent**: Min healthy tasks during deployment (50%)
- **deploymentCircuitBreaker**: Automatic rollback on failed deployments

### Load Balancer Integration
```json
"loadBalancers": [{
  "targetGroupArn": "arn:aws:elasticloadbalancing:...",
  "containerName": "comptestorbit001",
  "containerPort": 8080
}],
"healthCheckGracePeriodSeconds": 300
```
- **targetGroupArn**: Application Load Balancer target group
- **healthCheckGracePeriodSeconds**: Time before ALB health checks start

---

## Deployment Walkthrough

### Step 1: Build and Push Docker Image

#### Linux/macOS
```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

#### Windows
```cmd
scripts\build-push.bat
```

**Interactive Prompts**:
1. Select registry type (1=AWS ECR, 2=Docker Hub)
2. Enter image tag (default: latest)
3. Provide registry-specific credentials
4. Script will build and push image

**Example Output**:
```
Image: 123456789.dkr.ecr.us-east-1.amazonaws.com/comptestorbit001:latest
```

### Step 2: Deploy to ECS Fargate

#### Linux/macOS
```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

#### Windows
```cmd
scripts\deploy-image.bat
```

**Interactive Prompts**:
1. AWS region (e.g., us-east-1)
2. ECS cluster name
3. VPC ID
4. Subnet IDs (comma-separated, at least 2)
5. Security Group ID
6. Docker image URI (from Step 1)
7. Load balancer requirement (y/n)

**Deployment Process**:
1. Validates AWS credentials
2. Creates/verifies ECS cluster
3. Optionally creates Application Load Balancer and Target Group
4. Registers task definition
5. Creates or updates ECS service
6. Waits for service stability
7. Displays service details and access URLs

### Step 3: Verify Deployment

1. **Check service status**:
   ```bash
   aws ecs describe-services \
     --cluster comptestorbit001-cluster \
     --services comptestorbit001-service \
     --query 'services[0].[serviceName,status,runningCount,desiredCount]' \
     --output table
   ```

2. **List running tasks**:
   ```bash
   aws ecs list-tasks \
     --cluster comptestorbit001-cluster \
     --service-name comptestorbit001-service
   ```

3. **View CloudWatch logs**:
   ```bash
   aws logs tail /ecs/comptestorbit001 --follow
   ```

4. **Test application** (if using load balancer):
   ```bash
   curl http://<ALB_DNS_NAME>/health
   ```

---

## Troubleshooting

### Common Issues

#### 1. Task Fails to Start

**Symptom**: Tasks transition to STOPPED state immediately

**Diagnosis**:
```bash
aws ecs describe-tasks \
  --cluster comptestorbit001-cluster \
  --tasks <task-id> \
  --query 'tasks[0].containers[0].[lastStatus,reason,exitCode]'
```

**Common Causes**:
- Invalid CPU/memory combination
- Image pull errors (check ECR permissions)
- Application crashes on startup
- Health check failures

**Solutions**:
- Verify task definition CPU/memory values
- Check executionRoleArn has ECR permissions
- Review CloudWatch logs for application errors
- Increase health check startPeriod

#### 2. Unable to Pull Image from ECR

**Error**: "CannotPullContainerError"

**Solutions**:
```bash
# Verify ECR repository exists
aws ecr describe-repositories --repository-names comptestorbit001

# Check executionRoleArn permissions
aws iam get-role --role-name ecsTaskExecutionRole

# Verify image exists
aws ecr list-images --repository-name comptestorbit001
```

#### 3. Network Connectivity Issues

**Symptom**: Tasks cannot reach external services

**Solutions**:
- Verify security group allows outbound traffic
- Check subnet has internet gateway (public subnet) or NAT gateway (private subnet)
- Ensure assignPublicIp is ENABLED if using public subnets without NAT

#### 4. Health Check Failures

**Symptom**: Tasks continuously restart

**Diagnosis**:
```bash
# Check container logs
aws logs tail /ecs/comptestorbit001 --follow --filter-pattern "health"
```

**Solutions**:
- Verify health endpoint is accessible: `curl http://localhost:8080/health`
- Increase startPeriod in health check configuration
- Check application startup time
- Verify curl is available in container image

#### 5. Service Not Reaching Desired Count

**Symptom**: runningCount < desiredCount

**Diagnosis**:
```bash
aws ecs describe-services \
  --cluster comptestorbit001-cluster \
  --services comptestorbit001-service \
  --query 'services[0].events[0:10]'
```

**Common Causes**:
- Insufficient Fargate capacity (rare)
- Task definition errors
- Service limit reached

---

## Scaling and Management

### Manual Scaling

```bash
# Scale to 5 tasks
aws ecs update-service \
  --cluster comptestorbit001-cluster \
  --service comptestorbit001-service \
  --desired-count 5
```

### Auto Scaling with Application Auto Scaling

#### 1. Register Scalable Target
```bash
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --resource-id service/comptestorbit001-cluster/comptestorbit001-service \
  --scalable-dimension ecs:service:DesiredCount \
  --min-capacity 2 \
  --max-capacity 10
```

#### 2. Create Scaling Policy (Target Tracking - CPU)
```bash
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --resource-id service/comptestorbit001-cluster/comptestorbit001-service \
  --scalable-dimension ecs:service:DesiredCount \
  --policy-name cpu-target-tracking \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration '{
    "TargetValue": 70.0,
    "PredefinedMetricSpecification": {
      "PredefinedMetricType": "ECSServiceAverageCPUUtilization"
    },
    "ScaleOutCooldown": 60,
    "ScaleInCooldown": 300
  }'
```

#### 3. Create Scaling Policy (Target Tracking - Memory)
```bash
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --resource-id service/comptestorbit001-cluster/comptestorbit001-service \
  --scalable-dimension ecs:service:DesiredCount \
  --policy-name memory-target-tracking \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration '{
    "TargetValue": 80.0,
    "PredefinedMetricSpecification": {
      "PredefinedMetricType": "ECSServiceAverageMemoryUtilization"
    },
    "ScaleOutCooldown": 60,
    "ScaleInCooldown": 300
  }'
```

### Blue/Green Deployments with CodeDeploy

For zero-downtime deployments:

1. Create CodeDeploy application and deployment group
2. Configure ALB with two target groups (blue and green)
3. Update service to use CODE_DEPLOY deployment controller
4. Deploy new task definitions through CodeDeploy

### Monitoring with CloudWatch

#### Create CloudWatch Dashboard
```bash
aws cloudwatch put-dashboard \
  --dashboard-name comptestorbit001-dashboard \
  --dashboard-body file://dashboard-config.json
```

#### Key Metrics to Monitor
- **CPUUtilization**: Average CPU usage across tasks
- **MemoryUtilization**: Average memory usage across tasks
- **TargetResponseTime**: ALB response time
- **HealthyHostCount**: Number of healthy targets
- **UnHealthyHostCount**: Number of unhealthy targets

---

## Security Considerations

### 1. Container Security

- **Non-root user**: Dockerfile runs application as non-root user
- **Minimal base image**: Uses official Microsoft ASP.NET Core runtime
- **No secrets in environment variables**: Use AWS Secrets Manager

### 2. Network Security

- **Security groups**: Restrict inbound traffic to necessary ports only
- **Private subnets**: Deploy tasks in private subnets with NAT Gateway
- **VPC Flow Logs**: Enable for network traffic monitoring

### 3. IAM Security

- **Least privilege**: Grant only necessary permissions
- **Separate roles**: Use distinct executionRoleArn and taskRoleArn
- **Audit**: Regularly review IAM policies and access logs

### 4. Secrets Management

#### Using AWS Secrets Manager
```json
"secrets": [
  {
    "name": "DATABASE_CONNECTION_STRING",
    "valueFrom": "arn:aws:secretsmanager:region:account-id:secret:secret-name"
  }
]
```

#### Update Task Role
```bash
aws iam attach-role-policy \
  --role-name ecsTaskRole \
  --policy-arn arn:aws:iam::aws:policy/SecretsManagerReadWrite
```

### 5. Image Scanning

```bash
# Enable ECR image scanning
aws ecr put-image-scanning-configuration \
  --repository-name comptestorbit001 \
  --image-scanning-configuration scanOnPush=true

# View scan results
aws ecr describe-image-scan-findings \
  --repository-name comptestorbit001 \
  --image-id imageTag=latest
```

---

## Additional Resources

- **AWS ECS Documentation**: https://docs.aws.amazon.com/ecs/
- **AWS Fargate Documentation**: https://docs.aws.amazon.com/AmazonECS/latest/developerguide/AWS_Fargate.html
- **ASP.NET Core Documentation**: https://docs.microsoft.com/en-us/aspnet/core/
- **Docker Documentation**: https://docs.docker.com/
- **AWS CLI Reference**: https://docs.aws.amazon.com/cli/latest/reference/ecs/

---

## Support and Maintenance

For issues or questions:
1. Review this deployment guide
2. Check CloudWatch logs for application errors
3. Review ECS service events
4. Consult AWS documentation
5. Contact your DevOps team

**Document Version**: 1.0
**Last Updated**: 2026-02-06
**Application**: CompTestOrbit001
**Platform**: AWS ECS Fargate
