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
- Separate database for audit trail
- Automatic tracking of INSERT and DELETE operations
- EF Core interceptor captures all changes
- JSON change tracking with user context

### 4. **Clean Architecture**
- **Core Layer**: Domain entities and interfaces
- **Infrastructure Layer**: Data access, services, middleware
- **API Layer**: Controllers, DTOs, configuration

### 5. **Dual Database Architecture**
- Main database: Application data with tenant isolation
- Audit database: Compliance and security tracking

## 🏗️ Technology Stack

- **Backend**: .NET 8 Web API
- **Frontend**: Angular 17+ with Signals (to be created)
- **Database**: SQL Server (dual databases)
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

### 1. Setup Databases

```powershell
# Create migrations
dotnet ef migrations add InitialCreate --project MultiTenantSaaS.Infrastructure --startup-project MultiTenantSaaS.API --context ApplicationDbContext --output-dir Migrations/Main

dotnet ef migrations add InitialAudit --project MultiTenantSaaS.Infrastructure --startup-project MultiTenantSaaS.API --context AuditDbContext --output-dir Migrations/Audit

# Apply migrations
dotnet ef database update --project MultiTenantSaaS.Infrastructure --startup-project MultiTenantSaaS.API --context ApplicationDbContext

dotnet ef database update --project MultiTenantSaaS.Infrastructure --startup-project MultiTenantSaaS.API --context AuditDbContext
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

## 🐳 Docker Support

Docker configuration files will be added to run the entire stack:
- SQL Server (main + audit databases)
- .NET 8 API
- Angular frontend