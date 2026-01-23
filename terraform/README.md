# SentinelStack Terraform Infrastructure

This directory contains Terraform configurations for deploying SentinelStack to AWS.

## Architecture

```
Internet → Route53 → ALB (HTTPS:443) → ECS Fargate (8080) → RDS PostgreSQL (5432)
              ↓            ↓                    ↓
           ACM Cert    Target Group       Secrets Manager
                                               ↓
                                         CloudWatch Logs
```

## Prerequisites

1. **AWS CLI** configured with appropriate credentials
2. **Terraform** >= 1.0
3. **Route53 Hosted Zone** for your domain
4. **Docker** for building and pushing images

## Directory Structure

```
terraform/
├── environments/
│   ├── dev/          # Development environment
│   └── prod/         # Production environment
├── modules/
│   ├── alb/          # Application Load Balancer
│   ├── dns/          # ACM & Route53
│   ├── ecr/          # Container Registry
│   ├── ecs/          # ECS Fargate
│   ├── rds/          # PostgreSQL
│   ├── secrets/      # Secrets Manager
│   └── vpc/          # VPC & Networking
└── README.md
```

## Quick Start

### 1. Configure Your Domain

Update `terraform.tfvars` in your environment directory:

```hcl
domain_name      = "dev.yourdomain.com"
hosted_zone_name = "yourdomain.com"
```

### 2. Set Secrets

Set sensitive values via environment variables:

```bash
export TF_VAR_db_password="your-secure-db-password"
export TF_VAR_jwt_secret="your-32-character-jwt-secret-here"
export TF_VAR_hangfire_password="your-hangfire-dashboard-password"
```

Or create a `terraform.tfvars.local` file (gitignored):

```hcl
db_password       = "your-secure-db-password"
jwt_secret        = "your-32-character-jwt-secret-here"
hangfire_password = "your-hangfire-dashboard-password"
```

### 3. Deploy

```bash
# Navigate to environment
cd terraform/environments/dev

# Initialize Terraform
terraform init

# Review the plan
terraform plan

# Apply (this will take ~10-15 minutes)
terraform apply
```

### 4. Build and Push Docker Image

After initial deploy, push your Docker image:

```bash
# Get ECR login
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin $(terraform output -raw ecr_repository_url | cut -d'/' -f1)

# Build image
docker build -t sentinelstack .

# Tag and push
docker tag sentinelstack:latest $(terraform output -raw ecr_repository_url):latest
docker push $(terraform output -raw ecr_repository_url):latest

# Force ECS to deploy new image
aws ecs update-service \
  --cluster $(terraform output -raw ecs_cluster_name) \
  --service $(terraform output -raw ecs_service_name) \
  --force-new-deployment
```

### 5. Initialize Database

Connect to RDS and run the initialization script:

```bash
# Get RDS endpoint
terraform output rds_endpoint

# Run init script (you'll need psql and network access)
psql -h <rds-endpoint> -U sentinelstack -d sentinelstack -f ../../scripts/init-db.sql
```

## Environment Differences

| Feature | Dev | Prod |
|---------|-----|------|
| NAT Gateway | Single | Per AZ |
| RDS Instance | db.t3.micro | db.t3.small |
| RDS Multi-AZ | No | Yes |
| ECS Tasks | 1 | 2 (min) |
| Auto Scaling | No | Yes (max 10) |
| Backup Retention | 7 days | 30 days |
| Deletion Protection | No | Yes |

## Outputs

After deployment, get important values:

```bash
# Application URL
terraform output app_url

# ECR repository for Docker pushes
terraform output ecr_repository_url

# RDS endpoint for database connections
terraform output rds_endpoint

# CloudWatch log group for debugging
terraform output cloudwatch_log_group
```

## Costs

Estimated monthly costs (us-east-1):

### Dev Environment (~$50-80/month)
- NAT Gateway: ~$32
- RDS db.t3.micro: ~$13
- ALB: ~$16
- ECS Fargate: ~$10
- Other (ECR, CloudWatch, etc.): ~$5

### Prod Environment (~$200-400/month)
- NAT Gateways (2): ~$64
- RDS db.t3.small Multi-AZ: ~$50
- ALB: ~$16
- ECS Fargate (2+ tasks): ~$40+
- Other: ~$10

## Destroy

To tear down the infrastructure:

```bash
# Destroy (type 'yes' to confirm)
terraform destroy
```

**Note:** Production has deletion protection enabled. You must manually disable it first:
- RDS: `aws rds modify-db-instance --db-instance-identifier sentinelstack-prod --no-deletion-protection`
- ALB: Disable in AWS Console or via CLI

## Troubleshooting

### ACM Certificate Not Validating

Ensure your Route53 hosted zone is properly configured and the domain's NS records point to AWS.

### ECS Tasks Failing Health Checks

1. Check CloudWatch logs: `aws logs tail /ecs/sentinelstack-dev --follow`
2. Verify the database connection string is correct
3. Ensure secrets are properly configured

### Database Connection Issues

1. Verify security groups allow traffic from ECS to RDS
2. Check the connection string format in Secrets Manager
3. Ensure the database has been initialized with `init-db.sql`

## Security Notes

- All secrets stored in AWS Secrets Manager
- RDS only accessible from private subnets
- ALB enforces HTTPS (HTTP redirects)
- ECS tasks run with minimal IAM permissions
- VPC Flow Logs enabled for network monitoring
