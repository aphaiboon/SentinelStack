variable "project_name" {
  description = "Name of the project"
  type        = string
  default     = "sentinelstack"
}

variable "environment" {
  description = "Environment name (dev, prod)"
  type        = string
}

variable "domain_name" {
  description = "Primary domain name for the certificate"
  type        = string
}

variable "subject_alternative_names" {
  description = "Additional domain names for the certificate"
  type        = list(string)
  default     = []
}

variable "hosted_zone_name" {
  description = "Route53 hosted zone name (e.g., sentinelstack.io)"
  type        = string
}

variable "create_route53_record" {
  description = "Create Route53 records for DNS validation and app"
  type        = bool
  default     = true
}

variable "alb_dns_name" {
  description = "ALB DNS name for Route53 alias record"
  type        = string
  default     = ""
}

variable "alb_zone_id" {
  description = "ALB hosted zone ID for Route53 alias record"
  type        = string
  default     = ""
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
