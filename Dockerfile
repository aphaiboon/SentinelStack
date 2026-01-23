# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY SentinelStack.sln .
COPY src/SentinelStack.Domain/SentinelStack.Domain.csproj src/SentinelStack.Domain/
COPY src/SentinelStack.Application/SentinelStack.Application.csproj src/SentinelStack.Application/
COPY src/SentinelStack.Infrastructure/SentinelStack.Infrastructure.csproj src/SentinelStack.Infrastructure/
COPY src/SentinelStack.Api/SentinelStack.Api.csproj src/SentinelStack.Api/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY src/ src/

# Build and publish
WORKDIR /src/src/SentinelStack.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser

# Copy published files
COPY --from=build /app/publish .

# Set ownership and switch to non-root user
RUN chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "SentinelStack.Api.dll"]
