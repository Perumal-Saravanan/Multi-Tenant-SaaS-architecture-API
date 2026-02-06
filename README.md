# Multi-Tenant SaaS Architecture

Enterprise-grade Multi-Tenant Task Management System built with .NET 8 and Angular 17+, demonstrating logical tenant isolation, role-based access control, and comprehensive audit logging.

## 🎯 Key Features

### 1. **Logical Tenant Isolation**
- EF Core Global Query Filters automatically prevent cross-tenant data access
- Custom `tenantId` JWT claim for seamless tenant identification
- Zero manual filtering required in application code

### 2. **Role-Based Access Control (RBAC)**
- Three-tier role system: Admin, Manager, User
- JWT-based authentication with role claims
- Controller and action-level authorization

### 3. **Comprehensive Audit Logging**
- Integrated audit trail within the main database
- Automatic tracking of INSERT and DELETE operations
- EF Core interceptor captures all changes
- JSON change tracking with user context

### 4. **Clean Architecture**
- **Core Layer**: Domain entities and interfaces
- **Infrastructure Layer**: Data access, services, middleware
- **API Layer**: Controllers, DTOs, configuration

### 5. **Single Database Architecture**
- Unified database for Application data and Audit logs
- Simplified deployment and maintenance
- Transactional consistency between operations and audits

## 🏗️ Technology Stack

- **Backend**: .NET 8 Web API
- **Frontend**: Angular 17+ with Signals (to be created)
- **Database**: SQL Server
- **Authentication**: JWT Bearer tokens
- **ORM**: Entity Framework Core 8
- **API Documentation**: Swagger/OpenAPI

**Frontend Repository**: `Multi-Tenant-SaaS-architecture-Web` (separate repository)

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB, Express, or Docker)
- Node.js 18+ (for Angular frontend)
- Angular CLI 17+

### 1. Setup Database

```powershell
# Create migration
dotnet ef migrations add InitialCreate --project MultiTenantSaaS.Infrastructure --startup-project MultiTenantSaaS.API --context ApplicationDbContext --output-dir Migrations

# Apply migrations
dotnet ef database update --project MultiTenantSaaS.Infrastructure --startup-project MultiTenantSaaS.API --context ApplicationDbContext
```

### 2. Run Backend

```powershell
dotnet run --project MultiTenantSaaS.API
```

API available at: https://localhost:7001/swagger

### 3. Test Multi-Tenant Isolation

See [SETUP.md](SETUP.md) for detailed API testing examples.

## 🔑 Core Implementation Highlights

### Global Query Filter (Tenant Isolation)
```csharp
// ApplicationDbContext.cs
modelBuilder.Entity<TaskItem>()
    .HasQueryFilter(t => t.TenantId == _tenantService.GetCurrentTenantId());
```

### JWT with Custom Tenant Claim
```csharp
// AuthController.cs
new Claim("tenantId", tenantId.ToString())
```

### Tenant Middleware
```csharp
// TenantMiddleware.cs
var tenantIdClaim = context.User.FindFirst("tenantId")?.Value;
tenantService.SetTenantContext(tenantId, userId);
```

### Audit Interceptor
```csharp
// AuditInterceptor.cs
public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(...)
{
    await CaptureAuditLogs(eventData.Context);
    return await base.SavingChangesAsync(...);
}
```

## 📚 Documentation

- [SETUP.md](SETUP.md) - Detailed setup and testing instructions
- [Walkthrough](walkthrough.md) - Complete implementation walkthrough
- [API Documentation](https://localhost:7001/swagger) - Interactive API docs

## 🐳 Docker & CI/CD

### Local Development with Docker

Run the complete stack locally with Docker Compose:

```bash
# Build and start all services
docker-compose up --build

# API will be available at http://localhost:8080
# SQL Server at localhost:1433
```

### Building Docker Image Manually

```bash
# Build the image
docker build -t multitenant-saas-api:latest .

# Run the container
docker run -d -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="your-connection-string" \
  -e Jwt__Secret="your-secret-key" \
  multitenant-saas-api:latest
```

### GitHub Actions CI/CD Pipeline

The project includes a complete CI/CD pipeline (`.github/workflows/ci-cd.yml`) with:

**Build Stage**: 
- .NET 8 SDK setup
- Dependency restoration with caching
- Solution build

**Test Stage**:
- Automated unit tests
- Code coverage reports
- Test result publishing

**Security Scan**: 
- SonarCloud static code analysis
- Security vulnerability scanning
- Code quality metrics

**Docker Stage**:
- Multi-stage Docker build
- Push to Docker Hub
- Image tagging with commit SHA

**Deploy Stage** (main branch only):
- Automated deployment to AWS EC2
- Zero-downtime deployment with health checks
- Automatic rollback on failure

#### Required GitHub Secrets

Configure these secrets in your GitHub repository:

```
AWS_ACCESS_KEY_ID          # AWS credentials for EC2
AWS_SECRET_ACCESS_KEY      # AWS secret key
AWS_REGION                 # AWS region (e.g., us-east-1)
EC2_HOST                   # EC2 instance public IP
EC2_USERNAME               # SSH username (e.g., ubuntu)
EC2_SSH_KEY                # Private SSH key for EC2 access
DOCKER_USERNAME            # Docker Hub username
DOCKER_PASSWORD            # Docker Hub password/token
SONAR_TOKEN                # SonarCloud token
DB_CONNECTION_STRING       # Production DB connection
JWT_SECRET                 # Production JWT secret key
```

#### SonarCloud Setup

1. Sign up at [SonarCloud.io](https://sonarcloud.io) (free for public repos)
2. Create a new project and get your token
3. Update `sonarcloud.properties` with your organization name
4. Add `SONAR_TOKEN` to GitHub Secrets

### Health Check Endpoint

Monitor application health:

```bash
curl http://localhost:8080/api/health
```

Response:
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "service": "Multi-Tenant SaaS API",
  "version": "1.0.0",
  "checks": {
    "database": "Healthy",
  }
}
```