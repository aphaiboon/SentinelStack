variable "project_name" {
  description = "Name of the project"
  type        = string
  default     = "sentinelstack"
}

variable "environment" {
  description = "Environment name (dev, prod)"
  type        = string
}

variable "db_password" {
  description = "PostgreSQL database password"
  type        = string
  sensitive   = true
}

variable "db_connection_string" {
  description = "PostgreSQL connection string"
  type        = string
  sensitive   = true
}

variable "jwt_secret" {
  description = "JWT signing secret (min 32 characters)"
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

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
