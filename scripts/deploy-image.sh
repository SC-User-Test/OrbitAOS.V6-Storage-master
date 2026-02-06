#!/bin/bash
set -e
set -o pipefail

echo "======================================"
echo "AWS ECS Fargate Deployment Script"
echo "======================================"
echo ""

# Configuration
PROJECT_NAME="comptestorbit001"
TASK_FAMILY="${PROJECT_NAME}-task"
SERVICE_NAME="${PROJECT_NAME}-service"

# Prompt for AWS configuration
read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
export AWS_DEFAULT_REGION="$AWS_REGION"

read -p "Enter ECS cluster name (e.g., my-ecs-cluster): " CLUSTER_NAME

echo ""
echo "--- Network Configuration ---"
read -p "Enter VPC ID (e.g., vpc-0abc123def456): " VPC_ID
read -p "Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): " SUBNETS_INPUT
read -p "Enter Security Group ID (e.g., sg-0abc123def): " SECURITY_GROUP

# Parse subnets
IFS=',' read -ra SUBNETS <<< "$SUBNETS_INPUT"
SUBNET_1="${SUBNETS[0]}"
SUBNET_2="${SUBNETS[1]:-$SUBNET_1}"

echo ""
read -p "Enter Docker image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/comptestorbit001:latest): " IMAGE_URI

echo ""
echo "Getting AWS Account ID..."
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
echo "Account ID: $ACCOUNT_ID"

echo ""
echo "Checking if ECS cluster exists..."
aws ecs describe-clusters --clusters "$CLUSTER_NAME" --region "$AWS_REGION" >/dev/null 2>&1 || {
  echo "Cluster does not exist. Creating ECS cluster: $CLUSTER_NAME"
  aws ecs create-cluster --cluster-name "$CLUSTER_NAME" --region "$AWS_REGION"
}

echo ""
read -p "Do you need a load balancer for this service? (y/n): " NEED_LB

if [[ "$NEED_LB" =~ ^[Yy]$ ]]; then
  echo ""
  echo "--- Creating Application Load Balancer ---"
  
  # Create ALB
  echo "Creating Application Load Balancer..."
  ALB_NAME="${PROJECT_NAME}-alb"
  ALB_ARN=$(aws elbv2 create-load-balancer \
    --name "$ALB_NAME" \
    --subnets "$SUBNET_1" "$SUBNET_2" \
    --security-groups "$SECURITY_GROUP" \
    --scheme internet-facing \
    --type application \
    --ip-address-type ipv4 \
    --region "$AWS_REGION" \
    --query 'LoadBalancers[0].LoadBalancerArn' \
    --output text 2>/dev/null || aws elbv2 describe-load-balancers --names "$ALB_NAME" --query 'LoadBalancers[0].LoadBalancerArn' --output text)
  
  echo "Load Balancer ARN: $ALB_ARN"
  
  # Get ALB DNS name
  ALB_DNS=$(aws elbv2 describe-load-balancers --load-balancer-arns "$ALB_ARN" --query 'LoadBalancers[0].DNSName' --output text)
  
  # Create Target Group with ip target type (required for Fargate awsvpc mode)
  echo "Creating Target Group..."
  TG_NAME="${PROJECT_NAME}-tg"
  TARGET_GROUP_ARN=$(aws elbv2 create-target-group \
    --name "$TG_NAME" \
    --protocol HTTP \
    --port 8080 \
    --vpc-id "$VPC_ID" \
    --target-type ip \
    --health-check-enabled \
    --health-check-protocol HTTP \
    --health-check-path /health \
    --health-check-interval-seconds 30 \
    --health-check-timeout-seconds 5 \
    --healthy-threshold-count 2 \
    --unhealthy-threshold-count 3 \
    --region "$AWS_REGION" \
    --query 'TargetGroups[0].TargetGroupArn' \
    --output text 2>/dev/null || aws elbv2 describe-target-groups --names "$TG_NAME" --query 'TargetGroups[0].TargetGroupArn' --output text)
  
  echo "Target Group ARN: $TARGET_GROUP_ARN"
  
  # Create Listener
  echo "Creating ALB Listener..."
  aws elbv2 create-listener \
    --load-balancer-arn "$ALB_ARN" \
    --protocol HTTP \
    --port 80 \
    --default-actions Type=forward,TargetGroupArn="$TARGET_GROUP_ARN" \
    --region "$AWS_REGION" >/dev/null 2>&1 || echo "Listener may already exist"
  
  USE_LOAD_BALANCER=true
else
  USE_LOAD_BALANCER=false
fi

echo ""
echo "--- Preparing Task Definition ---"

# Copy and update task definition
cp ecs/task-definition.json /tmp/task-definition.json
sed -i "s|{{IMAGE_URI}}|$IMAGE_URI|g" /tmp/task-definition.json
sed -i "s|{{AWS_REGION}}|$AWS_REGION|g" /tmp/task-definition.json
sed -i "s|{{ACCOUNT_ID}}|$ACCOUNT_ID|g" /tmp/task-definition.json

echo "Registering ECS task definition..."
TASK_DEF_ARN=$(aws ecs register-task-definition \
  --cli-input-json file:///tmp/task-definition.json \
  --region "$AWS_REGION" \
  --query 'taskDefinition.taskDefinitionArn' \
  --output text)

echo "Task Definition ARN: $TASK_DEF_ARN"

echo ""
echo "--- Preparing Service Definition ---"

# Copy and update service definition
cp ecs/service-definition.json /tmp/service-definition.json
sed -i "s|{{CLUSTER_NAME}}|$CLUSTER_NAME|g" /tmp/service-definition.json
sed -i "s|{{SUBNET_1}}|$SUBNET_1|g" /tmp/service-definition.json
sed -i "s|{{SUBNET_2}}|$SUBNET_2|g" /tmp/service-definition.json
sed -i "s|{{SECURITY_GROUP}}|$SECURITY_GROUP|g" /tmp/service-definition.json

if [ "$USE_LOAD_BALANCER" = true ]; then
  sed -i "s|{{TARGET_GROUP_ARN}}|$TARGET_GROUP_ARN|g" /tmp/service-definition.json
else
  # Remove loadBalancers section if not using load balancer
  jq 'del(.loadBalancers, .healthCheckGracePeriodSeconds)' /tmp/service-definition.json > /tmp/service-definition-temp.json
  mv /tmp/service-definition-temp.json /tmp/service-definition.json
fi

echo ""
echo "--- Checking if Service Exists ---"
SERVICE_EXISTS=$(aws ecs describe-services \
  --cluster "$CLUSTER_NAME" \
  --services "$SERVICE_NAME" \
  --region "$AWS_REGION" \
  --query 'services[0].serviceName' \
  --output text 2>/dev/null)

if [ "$SERVICE_EXISTS" = "$SERVICE_NAME" ]; then
  echo "Service exists. Updating service..."
  aws ecs update-service \
    --cluster "$CLUSTER_NAME" \
    --service "$SERVICE_NAME" \
    --task-definition "$TASK_DEF_ARN" \
    --force-new-deployment \
    --region "$AWS_REGION"
else
  echo "Service does not exist. Creating new service..."
  aws ecs create-service \
    --cli-input-json file:///tmp/service-definition.json \
    --region "$AWS_REGION"
fi

echo ""
echo "Waiting for service to become stable..."
aws ecs wait services-stable \
  --cluster "$CLUSTER_NAME" \
  --services "$SERVICE_NAME" \
  --region "$AWS_REGION"

echo ""
echo "======================================"
echo "Deployment Completed Successfully"
echo "======================================"

# Get service details
RUNNING_COUNT=$(aws ecs describe-services \
  --cluster "$CLUSTER_NAME" \
  --services "$SERVICE_NAME" \
  --region "$AWS_REGION" \
  --query 'services[0].runningCount' \
  --output text)

echo "Cluster: $CLUSTER_NAME"
echo "Service: $SERVICE_NAME"
echo "Task Definition: $TASK_DEF_ARN"
echo "Running Tasks: $RUNNING_COUNT"

if [ "$USE_LOAD_BALANCER" = true ]; then
  echo "Load Balancer DNS: http://$ALB_DNS"
fi

echo "CloudWatch Log Group: /ecs/$PROJECT_NAME"
echo ""
echo "To view logs:"
echo "aws logs tail /ecs/$PROJECT_NAME --follow --region $AWS_REGION"
echo ""
