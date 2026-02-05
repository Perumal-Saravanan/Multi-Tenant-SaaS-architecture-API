# Setup Instructions

## Prerequisites

1. **Install Node.js** (required for Angular frontend)
   - Download from: https://nodejs.org/ (LTS version recommended)
   - Verify installation: `node --version` and `npm --version`

2. **Install Angular CLI**
   ```powershell
   npm install -g @angular/cli@17
   ```

3. **SQL Server** (LocalDB, Express, or Docker)
   - Update connection strings in `backend/MultiTenantSaaS.API/appsettings.json`

## Quick Start

### 1. Create Database Migrations

```powershell
cd backend

# Main database migration
dotnet ef migrations add InitialCreate `
  --project MultiTenantSaaS.Infrastructure `
  --startup-project MultiTenantSaaS.API `
  --context ApplicationDbContext `
  --output-dir Migrations/Main

# Audit database migration
dotnet ef migrations add InitialAudit `
  --project MultiTenantSaaS.Infrastructure `
  --startup-project MultiTenantSaaS.API `
  --context AuditDbContext `
  --output-dir Migrations/Audit
```

### 2. Apply Migrations

```powershell
# Create main database
dotnet ef database update `
  --project MultiTenantSaaS.Infrastructure `
  --startup-project MultiTenantSaaS.API `
  --context ApplicationDbContext

# Create audit database  
dotnet ef database update `
  --project MultiTenantSaaS.Infrastructure `
  --startup-project MultiTenantSaaS.API `
  --context AuditDbContext
```

### 3. Run Backend

```powershell
cd backend
dotnet run --project MultiTenantSaaS.API
```

API will be available at:
- HTTPS: https://localhost:7001
- HTTP: http://localhost:5001
- Swagger: https://localhost:7001/swagger

### 4. Create Angular Frontend (after Node.js installation)

```powershell
cd ..
ng new frontend --routing --style=css --standalone
cd frontend
ng serve
```

Frontend will run at: http://localhost:4200

## Testing the Multi-Tenant System

### 1. Register First Tenant (Company A)

```http
POST https://localhost:7001/api/auth/register
Content-Type: application/json

{
  "email": "admin@companya.com",
  "password": "Password123!",
  "firstName": "John",
  "lastName": "Doe",
  "companyName": "Company A",
  "companyCode": "COMPA"
}
```

### 2. Register Second Tenant (Company B)

```http
POST https://localhost:7001/api/auth/register
Content-Type: application/json

{
  "email": "admin@companyb.com",
  "password": "Password123!",
  "firstName": "Jane",
  "lastName": "Smith",
  "companyName": "Company B",
  "companyCode": "COMPB"
}
```

### 3. Login and Get JWT Token

```http
POST https://localhost:7001/api/auth/login
Content-Type: application/json

{
  "email": "admin@companya.com",
  "password": "Password123!"
}
```

Copy the `token` from response.

### 4. Create Tasks (Tenant Isolated)

```http
POST https://localhost:7001/api/tasks
Authorization: Bearer YOUR_TOKEN_HERE
Content-Type: application/json

{
  "title": "My First Task",
  "description": "This task belongs to my tenant only",
  "priority": 2,
  "dueDate": "2026-02-10T00:00:00Z"
}
```

### 5. Verify Tenant Isolation

- Login as Company A admin
- Create tasks
- Login as Company B admin  
- Try to get tasks - you'll only see Company B's tasks!

**The Global Query Filter automatically prevents cross-tenant data access.**

### 6. View Audit Logs (Admin Only)

```http
GET https://localhost:7001/api/audit?pageNumber=1&pageSize=50
Authorization: Bearer YOUR_ADMIN_TOKEN_HERE
```

## Docker Deployment (Optional)

Docker files will be created to run the entire stack with:
```powershell
docker-compose up
```

This will start:
- SQL Server (main database)
- SQL Server (audit database)
- .NET API
- Angular frontend
