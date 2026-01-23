# Dev Environment for SentinelStack

terraform {
  required_version = ">= 1.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }

  # Uncomment to use S3 backend for state
  # backend "s3" {
  #   bucket         = "sentinelstack-terraform-state"
  #   key            = "dev/terraform.tfstate"
  #   region         = "us-east-1"
  #   encrypt        = true
  #   dynamodb_table = "sentinelstack-terraform-locks"
  # }
}

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = {
      Project     = var.project_name
      Environment = var.environment
      ManagedBy   = "terraform"
    }
  }
}

locals {
  tags = {
    Project     = var.project_name
    Environment = var.environment
  }
}

# ECR Repository
module "ecr" {
  source = "../../modules/ecr"

  project_name = var.project_name
  environment  = var.environment
  tags         = local.tags
}

# VPC
module "vpc" {
  source = "../../modules/vpc"

  project_name       = var.project_name
  environment        = var.environment
  vpc_cidr           = var.vpc_cidr
  enable_nat_gateway = true
  single_nat_gateway = true # Cost savings for dev
  enable_flow_logs   = true
  tags               = local.tags
}

# Secrets (created after RDS to get connection string)
module "secrets" {
  source = "../../modules/secrets"

  project_name         = var.project_name
  environment          = var.environment
  db_password          = var.db_password
  db_connection_string = "Host=${module.rds.address};Port=${module.rds.port};Database=${module.rds.db_name};Username=${module.rds.username};Password=${var.db_password}"
  jwt_secret           = var.jwt_secret
  hangfire_password    = var.hangfire_password
  smtp_username        = var.smtp_username
  smtp_password        = var.smtp_password
  tags                 = local.tags

  depends_on = [module.rds]
}

# RDS PostgreSQL
module "rds" {
  source = "../../modules/rds"

  project_name      = var.project_name
  environment       = var.environment
  subnet_ids        = module.vpc.private_subnet_ids
  security_group_id = module.vpc.rds_security_group_id
  instance_class    = var.rds_instance_class
  allocated_storage = var.rds_allocated_storage
  db_password       = var.db_password
  multi_az          = false
  tags              = local.tags
}

# DNS (ACM Certificate)
module "dns" {
  source = "../../modules/dns"

  project_name          = var.project_name
  environment           = var.environment
  domain_name           = var.domain_name
  hosted_zone_name      = var.hosted_zone_name
  create_route53_record = var.create_route53_record
  alb_dns_name          = module.alb.alb_dns_name
  alb_zone_id           = module.alb.alb_zone_id
  tags                  = local.tags
}

# ALB
module "alb" {
  source = "../../modules/alb"

  project_name      = var.project_name
  environment       = var.environment
  vpc_id            = module.vpc.vpc_id
  subnet_ids        = module.vpc.public_subnet_ids
  security_group_id = module.vpc.alb_security_group_id
  certificate_arn   = module.dns.certificate_arn
  tags              = local.tags
}

# ECS
module "ecs" {
  source = "../../modules/ecs"

  project_name         = var.project_name
  environment          = var.environment
  aws_region           = var.aws_region
  vpc_id               = module.vpc.vpc_id
  subnet_ids           = module.vpc.private_subnet_ids
  security_group_id    = module.vpc.ecs_security_group_id
  target_group_arn     = module.alb.target_group_arn
  ecr_repository_url   = module.ecr.repository_url
  cpu                  = var.ecs_cpu
  memory               = var.ecs_memory
  desired_count        = var.ecs_desired_count
  enable_autoscaling   = false
  app_base_url         = "https://${var.domain_name}"
  secret_arns                  = module.secrets.all_secret_arns
  db_connection_secret_arn     = module.secrets.db_connection_secret_arn
  jwt_secret_arn               = module.secrets.jwt_secret_arn
  hangfire_password_secret_arn = module.secrets.hangfire_password_secret_arn
  tags                 = local.tags
}

# Update DNS with ALB (after ALB is created)
resource "aws_route53_record" "app" {
  count   = var.create_route53_record ? 1 : 0
  zone_id = data.aws_route53_zone.main[0].zone_id
  name    = var.domain_name
  type    = "A"

  alias {
    name                   = module.alb.alb_dns_name
    zone_id                = module.alb.alb_zone_id
    evaluate_target_health = true
  }
}

data "aws_route53_zone" "main" {
  count = var.create_route53_record ? 1 : 0
  name  = var.hosted_zone_name
}
