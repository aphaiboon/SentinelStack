# Dev Environment Configuration
# Copy this file and fill in your values

project_name = "sentinelstack"
environment  = "dev"
aws_region   = "us-east-1"

# Domain configuration (update these for your domain)
domain_name           = "dev.sentinelstack.io"
hosted_zone_name      = "sentinelstack.io"
create_route53_record = true

# VPC
vpc_cidr = "10.0.0.0/16"

# RDS (dev sizing)
rds_instance_class    = "db.t3.micro"
rds_allocated_storage = 20

# ECS (dev sizing)
ecs_cpu           = 256
ecs_memory        = 512
ecs_desired_count = 1

# Secrets - DO NOT commit real values!
# Use environment variables or a secrets file:
# export TF_VAR_db_password="your-secure-password"
# export TF_VAR_jwt_secret="your-32-char-jwt-secret-here123"
# export TF_VAR_hangfire_password="your-hangfire-password"

# Or create a terraform.tfvars.local file (gitignored) with:
# db_password       = "your-secure-password"
# jwt_secret        = "your-32-char-jwt-secret-here123"
# hangfire_password = "your-hangfire-password"
