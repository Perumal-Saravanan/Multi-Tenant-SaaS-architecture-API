# Docker Deployment Guide

This guide covers local development, building, and deploying the Multi-Tenant SaaS application using Docker.

## Local Development

### Using Docker Compose (Recommended)

1. **Start all services**:
   ```bash
   docker-compose up --build
   ```

2. **Access the application**:
   - API: http://localhost:8080
   - Swagger UI: http://localhost:8080/swagger
   - Health Check: http://localhost:8080/api/health

3. **Database connections**:
   - Main DB: `localhost:1433` (SA password: `YourStrong@Passw0rd`)
   - Audit DB: `localhost:1434` (SA password: `YourStrong@Passw0rd`)

4. **Stop services**:
   ```bash
   docker-compose down
   ```

5. **Stop and remove volumes** (clean slate):
   ```bash
   docker-compose down -v
   ```

## Building Docker Image

### Multi-Stage Build

The Dockerfile uses a multi-stage build for optimization:

1. **Build stage**: Compiles the .NET application
2. **Test stage**: Runs unit tests
3. **Publish stage**: Creates production binaries
4. **Runtime stage**: Minimal runtime image

### Build Manually

```bash
# Build the image
docker build -t multitenant-saas-api:latest .

# Build with specific tag
docker build -t yourusername/multitenant-saas-api:v1.0.0 .

# Build without cache
docker build --no-cache -t multitenant-saas-api:latest .
```

## Running Docker Container

### Basic Run

```bash
docker run -d \
  --name multitenant-api \
  -p 8080:8080 \
  multitenant-saas-api:latest
```

### Production Run with Environment Variables

```bash
docker run -d \
  --name multitenant-api \
  --restart unless-stopped \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e "ConnectionStrings__DefaultConnection=Server=your-db;Database=MultiTenantSaaS;User Id=sa;Password=YourPassword;TrustServerCertificate=True" \
  -e "ConnectionStrings__AuditConnection=Server=your-db;Database=MultiTenantSaaSAudit;User Id=sa;Password=YourPassword;TrustServerCertificate=True" \
  -e "Jwt__Secret=YourSuperSecretKeyForJWTTokenGeneration12345!" \
  -e "Jwt__Issuer=MultiTenantSaaSAPI" \
  -e "Jwt__Audience=MultiTenantSaaSClient" \
  multitenant-saas-api:latest
```

### Using Environment File

Create `.env` file:
```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=your-db;Database=MultiTenantSaaS;...
ConnectionStrings__AuditConnection=Server=your-db;Database=MultiTenantSaaSAudit;...
Jwt__Secret=YourSuperSecretKeyForJWTTokenGeneration12345!
Jwt__Issuer=MultiTenantSaaSAPI
Jwt__Audience=MultiTenantSaaSClient
```

Run with env file:
```bash
docker run -d --name multitenant-api -p 8080:8080 --env-file .env multitenant-saas-api:latest
```

## EC2 Deployment

### Prerequisites

1. EC2 instance with Docker installed
2. Security group allowing inbound traffic on port 8080
3. SSH access to the instance

### Manual Deployment

1. **SSH into EC2**:
   ```bash
   ssh -i your-key.pem ubuntu@your-ec2-ip
   ```

2. **Pull the image**:
   ```bash
   docker pull yourusername/multitenant-saas-api:latest
   ```

3. **Run the container**:
   ```bash
   chmod +x deploy.sh
   ./deploy.sh
   ```

### Automated Deployment

The GitHub Actions pipeline automatically deploys to EC2 on push to `main` branch.

See `.github/workflows/ci-cd.yml` for details.

## Useful Docker Commands

### Container Management

```bash
# View running containers
docker ps

# View all containers
docker ps -a

# Stop container
docker stop multitenant-api

# Start container
docker start multitenant-api

# Restart container
docker restart multitenant-api

# View logs
docker logs multitenant-api

# Follow logs
docker logs -f multitenant-api

# Remove container
docker rm multitenant-api
```

### Image Management

```bash
# List images
docker images

# Remove image
docker rmi multitenant-saas-api:latest

# Remove unused images
docker image prune

# Remove all unused images
docker image prune -a
```

### Debugging

```bash
# Execute command in running container
docker exec -it multitenant-api /bin/bash

# Inspect container
docker inspect multitenant-api

# View container resource usage
docker stats multitenant-api
```

## Health Checks

### Using curl

```bash
curl http://localhost:8080/api/health
```

### Using Docker healthcheck

```bash
docker inspect --format='{{json .State.Health}}' multitenant-api
```

## Troubleshooting

### Container won't start

1. Check logs:
   ```bash
   docker logs multitenant-api
   ```

2. Verify environment variables:
   ```bash
   docker inspect multitenant-api | grep -A 20 Env
   ```

### Database connection issues

1. Ensure connection strings are correct
2. Check network connectivity from container to database
3. Verify database credentials

### Port already in use

```bash
# Find process using port 8080
netstat -ano | findstr :8080  # Windows
lsof -i :8080                 # Linux/Mac

# Use different port
docker run -p 8081:8080 multitenant-saas-api:latest
```

## Best Practices

1. ✅ Use specific image tags instead of `latest` in production
2. ✅ Store secrets in environment variables, never in the image
3. ✅ Use multi-stage builds to minimize image size
4. ✅ Run containers as non-root user (already configured)
5. ✅ Implement health checks for container orchestration
6. ✅ Use volume mounts for persistent data
7. ✅ Configure proper logging drivers
8. ✅ Set resource limits (CPU, memory)

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [ASP.NET Core Docker Best Practices](https://learn.microsoft.com/en-us/dotnet/core/docker/build-container)
- [AWS EC2 Docker Installation](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/docker-basics.html)
