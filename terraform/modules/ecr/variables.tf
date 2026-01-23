variable "project_name" {
  description = "Name of the project"
  type        = string
  default     = "sentinelstack"
}

variable "environment" {
  description = "Environment name (dev, prod)"
  type        = string
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
