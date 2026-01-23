# Secrets Manager Module for SentinelStack

# Database Connection String
resource "aws_secretsmanager_secret" "db_connection" {
  name                    = "${var.project_name}/${var.environment}/db-connection"
  description             = "PostgreSQL connection string for SentinelStack"
  recovery_window_in_days = var.environment == "prod" ? 30 : 0

  tags = var.tags
}

resource "aws_secretsmanager_secret_version" "db_connection" {
  secret_id = aws_secretsmanager_secret.db_connection.id
  secret_string = jsonencode({
    connection_string = var.db_connection_string
    password          = var.db_password
  })
}

# JWT Secret
resource "aws_secretsmanager_secret" "jwt_secret" {
  name                    = "${var.project_name}/${var.environment}/jwt-secret"
  description             = "JWT signing secret for SentinelStack"
  recovery_window_in_days = var.environment == "prod" ? 30 : 0

  tags = var.tags
}

resource "aws_secretsmanager_secret_version" "jwt_secret" {
  secret_id = aws_secretsmanager_secret.jwt_secret.id
  secret_string = jsonencode({
    secret = var.jwt_secret
  })
}

# Hangfire Dashboard Password
resource "aws_secretsmanager_secret" "hangfire_password" {
  name                    = "${var.project_name}/${var.environment}/hangfire-password"
  description             = "Hangfire dashboard password for SentinelStack"
  recovery_window_in_days = var.environment == "prod" ? 30 : 0

  tags = var.tags
}

resource "aws_secretsmanager_secret_version" "hangfire_password" {
  secret_id = aws_secretsmanager_secret.hangfire_password.id
  secret_string = jsonencode({
    password = var.hangfire_password
  })
}

# SMTP Credentials (optional)
resource "aws_secretsmanager_secret" "smtp_credentials" {
  count                   = var.smtp_username != "" ? 1 : 0
  name                    = "${var.project_name}/${var.environment}/smtp-credentials"
  description             = "SMTP credentials for SentinelStack email notifications"
  recovery_window_in_days = var.environment == "prod" ? 30 : 0

  tags = var.tags
}

resource "aws_secretsmanager_secret_version" "smtp_credentials" {
  count     = var.smtp_username != "" ? 1 : 0
  secret_id = aws_secretsmanager_secret.smtp_credentials[0].id
  secret_string = jsonencode({
    username = var.smtp_username
    password = var.smtp_password
  })
}
