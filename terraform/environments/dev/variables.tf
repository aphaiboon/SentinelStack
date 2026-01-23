variable "project_name" {
  description = "Name of the project"
  type        = string
  default     = "sentinelstack"
}

variable "environment" {
  description = "Environment name"
  type        = string
  default     = "dev"
}

variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

variable "vpc_cidr" {
  description = "VPC CIDR block"
  type        = string
  default     = "10.0.0.0/16"
}

# Domain configuration
variable "domain_name" {
  description = "Domain name for the application"
  type        = string
}

variable "hosted_zone_name" {
  description = "Route53 hosted zone name"
  type        = string
}

variable "create_route53_record" {
  description = "Create Route53 records"
  type        = bool
  default     = true
}

# RDS configuration
variable "rds_instance_class" {
  description = "RDS instance class"
  type        = string
  default     = "db.t3.micro"
}

variable "rds_allocated_storage" {
  description = "RDS allocated storage in GB"
  type        = number
  default     = 20
}

# ECS configuration
variable "ecs_cpu" {
  description = "ECS task CPU units"
  type        = number
  default     = 256
}

variable "ecs_memory" {
  description = "ECS task memory in MB"
  type        = number
  default     = 512
}

variable "ecs_desired_count" {
  description = "ECS desired task count"
  type        = number
  default     = 1
}

# Secrets (sensitive)
variable "db_password" {
  description = "Database password"
  type        = string
  sensitive   = true
}

variable "jwt_secret" {
  description = "JWT signing secret"
  type        = string
  sensitive   = true
}

variable "hangfire_password" {
  description = "Hangfire dashboard password"
  type        = string
  sensitive   = true
}

variable "smtp_username" {
  description = "SMTP username (optional)"
  type        = string
  default     = ""
  sensitive   = true
}

variable "smtp_password" {
  description = "SMTP password (optional)"
  type        = string
  default     = ""
  sensitive   = true
}
