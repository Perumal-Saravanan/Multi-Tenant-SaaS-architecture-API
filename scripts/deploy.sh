#!/bin/bash

# Multi-Tenant SaaS API Deployment Script for EC2
# This script handles zero-downtime deployment with health checks

set -e  # Exit on error

# Configuration
APP_NAME="multitenant-api"
DOCKER_IMAGE="${DOCKER_IMAGE:-multitenant-saas-api:latest}"
CONTAINER_PORT=8080
HOST_PORT=8080
HEALTH_CHECK_URL="http://localhost:${HOST_PORT}/api/health"
MAX_HEALTH_CHECK_ATTEMPTS=10
HEALTH_CHECK_INTERVAL=5

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Multi-Tenant SaaS Deployment Script${NC}"
echo -e "${GREEN}========================================${NC}"

# Function to print colored messages
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if container is running
is_container_running() {
    docker ps --filter "name=$1" --filter "status=running" --format '{{.Names}}' | grep -q "^$1$"
}

# Function to check health
check_health() {
    local attempt=1
    log_info "Checking application health..."
    
    while [ $attempt -le $MAX_HEALTH_CHECK_ATTEMPTS ]; do
        if curl -f -s -o /dev/null "$HEALTH_CHECK_URL"; then
            log_info "✅ Health check passed!"
            return 0
        fi
        
        log_warn "Health check attempt $attempt/$MAX_HEALTH_CHECK_ATTEMPTS failed. Retrying in ${HEALTH_CHECK_INTERVAL}s..."
        sleep $HEALTH_CHECK_INTERVAL
        ((attempt++))
    done
    
    log_error "❌ Health check failed after $MAX_HEALTH_CHECK_ATTEMPTS attempts"
    return 1
}

# Pull the latest Docker image
log_info "Pulling latest Docker image: ${DOCKER_IMAGE}"
if ! docker pull "${DOCKER_IMAGE}"; then
    log_error "Failed to pull Docker image"
    exit 1
fi

# Backup current container if exists
if is_container_running "${APP_NAME}"; then
    log_info "Creating backup of current container..."
    docker rename "${APP_NAME}" "${APP_NAME}-backup"
    BACKUP_EXISTS=true
else
    BACKUP_EXISTS=false
fi

# Start new container
log_info "Starting new container..."
docker run -d \
    --name "${APP_NAME}" \
    --restart unless-stopped \
    -p ${HOST_PORT}:${CONTAINER_PORT} \
    -e ASPNETCORE_ENVIRONMENT=Production \
    -e "ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}" \
    -e "Jwt__Secret=${JWT_SECRET}" \
    -e "Jwt__Issuer=MultiTenantSaaSAPI" \
    -e "Jwt__Audience=MultiTenantSaaSClient" \
    "${DOCKER_IMAGE}"

# Wait for container to be ready
log_info "Waiting for container to start..."
sleep 10

# Perform health check
if check_health; then
    log_info "✅ Deployment successful!"
    
    # Clean up backup container
    if [ "$BACKUP_EXISTS" = true ]; then
        log_info "Removing backup container..."
        docker stop "${APP_NAME}-backup" || true
        docker rm "${APP_NAME}-backup" || true
    fi
    
    # Clean up old images
    log_info "Cleaning up old Docker images..."
    docker image prune -af
    
    log_info "🚀 Deployment completed successfully!"
    exit 0
else
    log_error "❌ Deployment failed! Rolling back..."
    
    # Stop and remove failed container
    docker stop "${APP_NAME}" || true
    docker rm "${APP_NAME}" || true
    
    # Restore backup if exists
    if [ "$BACKUP_EXISTS" = true ]; then
        log_info "Restoring previous version..."
        docker rename "${APP_NAME}-backup" "${APP_NAME}"
        docker start "${APP_NAME}"
        
        if check_health; then
            log_info "✅ Rollback successful. Previous version is running."
        else
            log_error "❌ Rollback failed. Manual intervention required!"
        fi
    fi
    
    exit 1
fi
