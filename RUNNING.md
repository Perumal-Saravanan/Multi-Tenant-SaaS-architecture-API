# Running the Multi-Tenant SaaS Application

## Quick Start (Visual Studio)

### 1. Open the Solution
```
backend\MultiTenantSaaS.sln
```

### 2. Configure Multi-Startup Projects
1. Right-click on the solution in Solution Explorer
2. Select **Properties**
3. Choose **Multiple startup projects**
4. Set the following projects to **Start**:
   - `MultiTenantSaaS.API` (Backend API)
   - `MultiTenantSaaS.Web` (Frontend Host)
5. Click **OK**

### 3. Install Node.js (If Not Already Installed)
- Download from: https://nodejs.org/ (LTS version)
- Verify: `node --version` and `npm --version`

### 4. First Time Setup
The first time you build `MultiTenantSaaS.Web`, it will automatically run `npm install` in the frontend folder.

### 5. Run the Application
Press **F5** in Visual Studio

This will start:
- **Backend API**: https://localhost:7001/swagger
- **Frontend**: http://localhost:4200 (Angular dev server via SPA proxy)

## Alternative: Command Line

### Terminal 1 - Backend API
```powershell
cd backend
dotnet run --project MultiTenantSaaS.API
```

### Terminal 2 - Frontend
```powershell
cd frontend
npm install  # First time only
npm start
```

## Database Setup

Before running the application, you need to create the databases:

```powershell
cd backend

# Create main database migration
dotnet ef migrations add InitialCreate `
  --project MultiTenantSaaS.Infrastructure `
  --startup-project MultiTenantSaaS.API `
  --context ApplicationDbContext `
  --output-dir Migrations/Main

# Create audit database migration
dotnet ef migrations add InitialAudit `
  --project MultiTenantSaaS.Infrastructure `
  --startup-project MultiTenantSaaS.API `
  --context AuditDbContext `
  --output-dir Migrations/Audit

# Apply migrations
dotnet ef database update `
  --project MultiTenantSaaS.Infrastructure `
  --startup-project MultiTenantSaaS.API `
  --context ApplicationDbContext

dotnet ef database update `
  --project MultiTenantSaaS.Infrastructure `
  --startup-project MultiTenantSaaS.API `
  --context AuditDbContext
```

## Testing the Application

### 1. Register a New Tenant
Navigate to http://localhost:4200 and you'll be redirected to login.

Click "Register here" and create an account:
- Email: admin@companya.com
- Password: Password123!
- First Name: John
- Last Name: Doe
- Company Name: Company A
- Company Code: COMPA

### 2. Login
Use the credentials you just created to login.

### 3. Explore Features
- **Dashboard**: View task statistics with computed signals
- **Tasks**: Create, update, complete, and delete tasks
- **Multi-Tenant Isolation**: All data is automatically filtered by your tenant ID
- **Role-Based Access**: Admin users can delete tasks and view audit logs

### 4. Test Multi-Tenancy
1. Register a second tenant with different company details
2. Login as the second tenant
3. Create tasks - you won't see the first tenant's tasks!

## Ports Reference

| Service | URL | Purpose |
|---------|-----|---------|
| Backend API | https://localhost:7001 | REST API with Swagger |
| Frontend (Dev) | http://localhost:4200 | Angular dev server |
| Frontend (Prod) | https://localhost:7002 | .NET hosted SPA |

## Troubleshooting

### Node.js Not Found
If you get "Node.js is required" error:
1. Install Node.js from https://nodejs.org/
2. Restart Visual Studio
3. Rebuild the solution

### Port Already in Use
If ports 7001, 7002, or 4200 are in use:
1. Edit `launchSettings.json` in each project
2. Change the port numbers
3. Update `environment.ts` in Angular to match the new API port

### Database Connection Failed
1. Check SQL Server is running
2. Update connection strings in `appsettings.json`
3. Run migrations as shown above

## Production Build

To create a production build:

```powershell
cd backend
dotnet publish -c Release -o ../publish
```

This will:
1. Build the .NET API
2. Run `npm run build` for Angular
3. Include the built Angular app in the publish output
4. Create a single deployable package in the `publish` folder
