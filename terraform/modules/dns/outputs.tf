output "certificate_arn" {
  description = "ACM certificate ARN"
  value       = aws_acm_certificate.main.arn
}

output "certificate_domain_name" {
  description = "Certificate domain name"
  value       = aws_acm_certificate.main.domain_name
}

output "certificate_status" {
  description = "Certificate status"
  value       = aws_acm_certificate.main.status
}

output "domain_validation_options" {
  description = "Domain validation options for manual DNS setup"
  value       = aws_acm_certificate.main.domain_validation_options
}

output "validated_certificate_arn" {
  description = "Validated ACM certificate ARN (use this for ALB)"
  value       = var.create_route53_record ? aws_acm_certificate_validation.main[0].certificate_arn : aws_acm_certificate.main.arn
}
