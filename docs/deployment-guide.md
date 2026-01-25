# Deployment Guide — SentinelStack

> Generated: 2026-01-21 | Target: AWS Cloud

## Target Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        AWS Cloud                             │
│                                                              │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐  │
│  │   Route 53   │───▶│     ALB      │───▶│   ECS/EKS    │  │
│  │    (DNS)     │    │ (Load Bal.)  │    │ (Container)  │  │
│  └──────────────┘    └──────────────┘    └──────┬───────┘  │
│                                                  │          │
│                                                  ▼          │
│                                          ┌──────────────┐  │
│                                          │  PostgreSQL  │  │
│                                          │    (RDS)     │  │
│                                          └──────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## Prerequisites

| Requirement | Version | Purpose |
|-------------|---------|---------|
| .NET SDK | 10.0+ | Build and publish |
| Docker | 24.0+ | Container builds |
| AWS CLI | 2.x | AWS deployment |
| Terraform | 1.5+ | Infrastructure as Code |

---

## Environment Configuration

### appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=${DB_HOST};Database=sentinelstack;Username=${DB_USER};Password=${DB_PASSWORD};SSL Mode=Require"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `DB_HOST` | PostgreSQL host | `sentinelstack.xxx.rds.amazonaws.com` |
| `DB_USER` | Database username | `sentinel_app` |
| `DB_PASSWORD` | Database password | (from Secrets Manager) |
| `ConnectionStrings__DefaultConnection` | Full connection string | (alternative to components) |

---

## Build & Publish

### Local Build

```bash
# Restore and build
dotnet restore
dotnet build --configuration Release

# Publish for deployment
dotnet publish src/SentinelStack.Api \
  --configuration Release \
  --output ./publish
```

### Docker Build

**Dockerfile** *(to be created)*:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish src/SentinelStack.Api \
    -c Release \
    -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SentinelStack.Api.dll"]
```

```bash
# Build Docker image
docker build -t sentinelstack-api:latest .

# Run locally
docker run -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  sentinelstack-api:latest
```

---

## Database Setup

### AWS RDS PostgreSQL

```bash
# Create RDS instance (via AWS CLI)
aws rds create-db-instance \
  --db-instance-identifier sentinelstack-db \
  --db-instance-class db.t3.micro \
  --engine postgres \
  --engine-version 14 \
  --master-username sentinel_admin \
  --master-user-password <password> \
  --allocated-storage 20 \
  --vpc-security-group-ids <sg-id> \
  --db-subnet-group-name <subnet-group>
```

### Apply Migrations

```bash
# Set connection string
export ConnectionStrings__DefaultConnection="Host=...;Database=sentinelstack;..."

# Apply migrations
dotnet ef database update \
  --project src/SentinelStack.Infrastructure \
  --startup-project src/SentinelStack.Api
```

---

## AWS Deployment Options

### Option 1: ECS Fargate (Recommended)

```bash
# Push to ECR
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin <account>.dkr.ecr.us-east-1.amazonaws.com

docker tag sentinelstack-api:latest <account>.dkr.ecr.us-east-1.amazonaws.com/sentinelstack-api:latest
docker push <account>.dkr.ecr.us-east-1.amazonaws.com/sentinelstack-api:latest
```

### Option 2: AWS App Runner

```bash
# Deploy via App Runner (simple option)
aws apprunner create-service \
  --service-name sentinelstack-api \
  --source-configuration '{
    "ImageRepository": {
      "ImageIdentifier": "<account>.dkr.ecr.us-east-1.amazonaws.com/sentinelstack-api:latest",
      "ImageRepositoryType": "ECR"
    }
  }'
```

### Option 3: Elastic Beanstalk

```bash
# Initialize EB
eb init sentinelstack-api --platform "Docker" --region us-east-1

# Deploy
eb create production --single --instance-type t3.micro
```

---

## Infrastructure as Code

**Location:** `infra/terraform/` *(placeholder)*

**Planned Resources:**

| Resource | Type | Purpose |
|----------|------|---------|
| VPC | `aws_vpc` | Network isolation |
| RDS | `aws_db_instance` | PostgreSQL database |
| ECS Cluster | `aws_ecs_cluster` | Container orchestration |
| ALB | `aws_lb` | Load balancing |
| ECR | `aws_ecr_repository` | Container registry |
| Secrets Manager | `aws_secretsmanager_secret` | Credentials storage |

---

## Health Checks

### Application Health

```bash
# Check API health
curl https://api.sentinelstack.io/api/v1/health
```

### Load Balancer Health Check

| Setting | Value |
|---------|-------|
| Path | `/api/v1/health` |
| Protocol | HTTPS |
| Interval | 30 seconds |
| Timeout | 5 seconds |
| Healthy threshold | 2 |
| Unhealthy threshold | 3 |

---

## CI/CD Pipeline *(Planned)*

**GitHub Actions** workflow to be created:

```yaml
# .github/workflows/deploy.yml
name: Deploy to AWS

on:
  push:
    branches: [main]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Build
        run: dotnet build --configuration Release
      - name: Test
        run: dotnet test
      - name: Publish
        run: dotnet publish src/SentinelStack.Api -c Release -o ./publish
      - name: Deploy to AWS
        # Add deployment steps
```

---

## Security Considerations

| Area | Recommendation |
|------|----------------|
| **Secrets** | Use AWS Secrets Manager for credentials |
| **Network** | Deploy in private subnet with NAT Gateway |
| **SSL/TLS** | Terminate at ALB with ACM certificate |
| **Database** | Enable encryption at rest and in transit |
| **IAM** | Use least-privilege roles for ECS tasks |

---

## Current Status

| Component | Status |
|-----------|--------|
| Dockerfile | ⏳ Not created |
| Terraform | ⏳ Placeholder only |
| CI/CD | ⏳ Not configured |
| Production environment | ⏳ Not deployed |
