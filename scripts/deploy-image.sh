#!/bin/bash
set -e
set -o pipefail

# OrbitAOS.V6 - Deploy to AWS ECS Fargate
# This script deploys the application to AWS ECS Fargate

echo "========================================="
echo "OrbitAOS.V6 - Deploy to AWS ECS Fargate"
echo "========================================="
echo ""

# Configuration
PROJECT_NAME="orbitaos-v6"
TASK_FAMILY="${PROJECT_NAME}-task"
SERVICE_NAME="${PROJECT_NAME}-service"

# Prompt for AWS configuration
echo "=== AWS Configuration ==="
read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
export AWS_DEFAULT_REGION="$AWS_REGION"

read -p "Enter ECS cluster name (e.g., orbitaos-cluster): " CLUSTER_NAME

read -p "Enter Docker image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/orbitaos-v6:latest): " IMAGE_URI

echo ""
echo "=== Network Configuration ==="
read -p "Enter VPC ID (e.g., vpc-0abc123def456): " VPC_ID
read -p "Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): " SUBNETS_INPUT
read -p "Enter Security Group ID (e.g., sg-0abc123def): " SECURITY_GROUP

# Split subnets
IFS=',' read -ra SUBNETS <<< "$SUBNETS_INPUT"
SUBNET_1="${SUBNETS[0]}"
SUBNET_2="${SUBNETS[1]:-$SUBNET_1}"

echo ""
echo "=== Database Configuration ==="
read -p "Enter Database Server (e.g., mydb.abc123.us-east-1.rds.amazonaws.com): " DB_SERVER
read -p "Enter Database Name (default: OrbitAOS): " DB_NAME
DB_NAME=${DB_NAME:-OrbitAOS}
read -p "Enter Database User (default: admin): " DB_USER
DB_USER=${DB_USER:-admin}
read -sp "Enter Database Password: " DB_PASSWORD
echo ""

echo ""
echo "=== Load Balancer Configuration ==="
read -p "Do you need a load balancer for this service? (y/n): " NEED_LB

if [ "$NEED_LB" = "y" ] || [ "$NEED_LB" = "Y" ]; then
    USE_LOAD_BALANCER=true
    echo "Creating Application Load Balancer and Target Group..."
    
    # Create Application Load Balancer
    ALB_NAME="${PROJECT_NAME}-alb"
    echo "Creating ALB: $ALB_NAME"
    
    ALB_ARN=$(aws elbv2 create-load-balancer \
        --name "$ALB_NAME" \
        --subnets "$SUBNET_1" "$SUBNET_2" \
        --security-groups "$SECURITY_GROUP" \
        --scheme internet-facing \
        --type application \
        --ip-address-type ipv4 \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].LoadBalancerArn' \
        --output text 2>/dev/null || echo "")
    
    if [ -z "$ALB_ARN" ]; then
        echo "ALB may already exist or creation failed. Attempting to find existing ALB..."
        ALB_ARN=$(aws elbv2 describe-load-balancers \
            --names "$ALB_NAME" \
            --region "$AWS_REGION" \
            --query 'LoadBalancers[0].LoadBalancerArn' \
            --output text 2>/dev/null || echo "")
    fi
    
    if [ -z "$ALB_ARN" ] || [ "$ALB_ARN" = "None" ]; then
        echo "ERROR: Failed to create or find Application Load Balancer"
        exit 1
    fi
    
    echo "ALB ARN: $ALB_ARN"
    
    # Get ALB DNS name
    ALB_DNS=$(aws elbv2 describe-load-balancers \
        --load-balancer-arns "$ALB_ARN" \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].DNSName' \
        --output text)
    
    # Create Target Group with target-type ip (required for Fargate)
    TG_NAME="${PROJECT_NAME}-tg"
    echo "Creating Target Group: $TG_NAME"
    
    TARGET_GROUP_ARN=$(aws elbv2 create-target-group \
        --name "$TG_NAME" \
        --protocol HTTP \
        --port 80 \
        --vpc-id "$VPC_ID" \
        --target-type ip \
        --health-check-enabled \
        --health-check-path "/health" \
        --health-check-interval-seconds 30 \
        --health-check-timeout-seconds 5 \
        --healthy-threshold-count 2 \
        --unhealthy-threshold-count 3 \
        --region "$AWS_REGION" \
        --query 'TargetGroups[0].TargetGroupArn' \
        --output text 2>/dev/null || echo "")
    
    if [ -z "$TARGET_GROUP_ARN" ]; then
        echo "Target Group may already exist. Attempting to find existing TG..."
        TARGET_GROUP_ARN=$(aws elbv2 describe-target-groups \
            --names "$TG_NAME" \
            --region "$AWS_REGION" \
            --query 'TargetGroups[0].TargetGroupArn' \
            --output text 2>/dev/null || echo "")
    fi
    
    if [ -z "$TARGET_GROUP_ARN" ] || [ "$TARGET_GROUP_ARN" = "None" ]; then
        echo "ERROR: Failed to create or find Target Group"
        exit 1
    fi
    
    echo "Target Group ARN: $TARGET_GROUP_ARN"
    
    # Create Listener
    LISTENER_ARN=$(aws elbv2 create-listener \
        --load-balancer-arn "$ALB_ARN" \
        --protocol HTTP \
        --port 80 \
        --default-actions Type=forward,TargetGroupArn="$TARGET_GROUP_ARN" \
        --region "$AWS_REGION" \
        --query 'Listeners[0].ListenerArn' \
        --output text 2>/dev/null || echo "")
    
    if [ -z "$LISTENER_ARN" ]; then
        echo "Listener may already exist. Continuing..."
    else
        echo "Listener ARN: $LISTENER_ARN"
    fi
else
    USE_LOAD_BALANCER=false
    TARGET_GROUP_ARN=""
    echo "Skipping load balancer creation."
fi

echo ""
echo "=== Getting AWS Account ID ==="
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
echo "Account ID: $ACCOUNT_ID"

echo ""
echo "=== Checking ECS Cluster ==="
aws ecs describe-clusters --clusters "$CLUSTER_NAME" --region "$AWS_REGION" >/dev/null 2>&1 || {
    echo "Cluster does not exist. Creating cluster: $CLUSTER_NAME"
    aws ecs create-cluster --cluster-name "$CLUSTER_NAME" --region "$AWS_REGION"
}

echo "Cluster $CLUSTER_NAME is ready"

echo ""
echo "=== Preparing Task Definition ==="

# Create temporary task definition file with replacements
TASK_DEF_TEMPLATE="ecs/task-definition.json"
TASK_DEF_FILE="/tmp/${TASK_FAMILY}-$(date +%s).json"

cp "$TASK_DEF_TEMPLATE" "$TASK_DEF_FILE"

# Replace placeholders
sed -i "s|{{IMAGE_URI}}|$IMAGE_URI|g" "$TASK_DEF_FILE"
sed -i "s|{{AWS_REGION}}|$AWS_REGION|g" "$TASK_DEF_FILE"
sed -i "s|{{ACCOUNT_ID}}|$ACCOUNT_ID|g" "$TASK_DEF_FILE"
sed -i "s|{{DB_SERVER}}|$DB_SERVER|g" "$TASK_DEF_FILE"
sed -i "s|{{DB_NAME}}|$DB_NAME|g" "$TASK_DEF_FILE"
sed -i "s|{{DB_USER}}|$DB_USER|g" "$TASK_DEF_FILE"
sed -i "s|{{DB_PASSWORD}}|$DB_PASSWORD|g" "$TASK_DEF_FILE"

echo "Registering task definition..."
TASK_DEF_ARN=$(aws ecs register-task-definition \
    --cli-input-json file://"$TASK_DEF_FILE" \
    --region "$AWS_REGION" \
    --query 'taskDefinition.taskDefinitionArn' \
    --output text)

if [ -z "$TASK_DEF_ARN" ] || [ "$TASK_DEF_ARN" = "None" ]; then
    echo "ERROR: Failed to register task definition"
    exit 1
fi

echo "Task Definition ARN: $TASK_DEF_ARN"

echo ""
echo "=== Preparing Service Definition ==="

# Create temporary service definition file with replacements
SERVICE_DEF_TEMPLATE="ecs/service-definition.json"
SERVICE_DEF_FILE="/tmp/${SERVICE_NAME}-$(date +%s).json"

cp "$SERVICE_DEF_TEMPLATE" "$SERVICE_DEF_FILE"

# Replace placeholders
sed -i "s|{{CLUSTER_NAME}}|$CLUSTER_NAME|g" "$SERVICE_DEF_FILE"
sed -i "s|{{SUBNET_1}}|$SUBNET_1|g" "$SERVICE_DEF_FILE"
sed -i "s|{{SUBNET_2}}|$SUBNET_2|g" "$SERVICE_DEF_FILE"
sed -i "s|{{SECURITY_GROUP}}|$SECURITY_GROUP|g" "$SERVICE_DEF_FILE"

# Handle load balancer configuration
if [ "$USE_LOAD_BALANCER" = true ]; then
    sed -i "s|{{TARGET_GROUP_ARN}}|$TARGET_GROUP_ARN|g" "$SERVICE_DEF_FILE"
else
    # Remove loadBalancers section if no LB
    jq 'del(.loadBalancers) | del(.healthCheckGracePeriodSeconds)' "$SERVICE_DEF_FILE" > "${SERVICE_DEF_FILE}.tmp"
    mv "${SERVICE_DEF_FILE}.tmp" "$SERVICE_DEF_FILE"
fi

echo ""
echo "=== Checking if Service Exists ==="

# Check if service exists
SERVICE_EXISTS=$(aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[0].serviceName' \
    --output text 2>/dev/null || echo "None")

if [ "$SERVICE_EXISTS" = "None" ] || [ -z "$SERVICE_EXISTS" ]; then
    echo "Service does not exist. Creating new service..."
    
    aws ecs create-service \
        --cli-input-json file://"$SERVICE_DEF_FILE" \
        --region "$AWS_REGION"
    
    if [ $? -ne 0 ]; then
        echo "ERROR: Failed to create service"
        exit 1
    fi
    
    echo "Service created successfully"
else
    echo "Service exists. Updating service..."
    
    UPDATE_CMD="aws ecs update-service \
        --cluster $CLUSTER_NAME \
        --service $SERVICE_NAME \
        --task-definition $TASK_DEF_ARN \
        --region $AWS_REGION"
    
    if [ "$USE_LOAD_BALANCER" = true ]; then
        UPDATE_CMD="$UPDATE_CMD --health-check-grace-period-seconds 300"
    fi
    
    eval "$UPDATE_CMD"
    
    if [ $? -ne 0 ]; then
        echo "ERROR: Failed to update service"
        exit 1
    fi
    
    echo "Service updated successfully"
fi

echo ""
echo "=== Waiting for Service Stability ==="
echo "This may take a few minutes..."

aws ecs wait services-stable \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION"

if [ $? -ne 0 ]; then
    echo "WARNING: Service did not stabilize within the expected time"
    echo "Check the ECS console for more details"
else
    echo "Service is stable"
fi

echo ""
echo "=== Deployment Summary ==="
echo "Cluster: $CLUSTER_NAME"
echo "Service: $SERVICE_NAME"
echo "Task Definition: $TASK_DEF_ARN"
echo "Region: $AWS_REGION"

if [ "$USE_LOAD_BALANCER" = true ]; then
    echo "Load Balancer DNS: $ALB_DNS"
    echo "Access your application at: http://$ALB_DNS"
fi

echo "CloudWatch Log Group: /ecs/orbitaos-v6"
echo ""
echo "To view service details:"
echo "aws ecs describe-services --cluster $CLUSTER_NAME --services $SERVICE_NAME --region $AWS_REGION"
echo ""
echo "To view logs:"
echo "aws logs tail /ecs/orbitaos-v6 --follow --region $AWS_REGION"
echo ""
echo "========================================="
echo "Deployment Complete!"
echo "========================================="

# Cleanup
rm -f "$TASK_DEF_FILE" "$SERVICE_DEF_FILE"