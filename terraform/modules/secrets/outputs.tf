output "db_connection_secret_arn" {
  description = "ARN of the database connection secret"
  value       = aws_secretsmanager_secret.db_connection.arn
}

output "jwt_secret_arn" {
  description = "ARN of the JWT secret"
  value       = aws_secretsmanager_secret.jwt_secret.arn
}

output "hangfire_password_secret_arn" {
  description = "ARN of the Hangfire password secret"
  value       = aws_secretsmanager_secret.hangfire_password.arn
}

output "smtp_credentials_secret_arn" {
  description = "ARN of the SMTP credentials secret"
  value       = var.smtp_username != "" ? aws_secretsmanager_secret.smtp_credentials[0].arn : null
}

output "all_secret_arns" {
  description = "List of all secret ARNs for IAM policy"
  value = compact([
    aws_secretsmanager_secret.db_connection.arn,
    aws_secretsmanager_secret.jwt_secret.arn,
    aws_secretsmanager_secret.hangfire_password.arn,
    var.smtp_username != "" ? aws_secretsmanager_secret.smtp_credentials[0].arn : null
  ])
}
