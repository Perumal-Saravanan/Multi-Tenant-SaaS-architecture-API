# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["MultiTenantSaaS.sln", "./"]
COPY ["MultiTenantSaaS.API/MultiTenantSaaS.API.csproj", "MultiTenantSaaS.API/"]
COPY ["MultiTenantSaaS.Core/MultiTenantSaaS.Core.csproj", "MultiTenantSaaS.Core/"]
COPY ["MultiTenantSaaS.Infrastructure/MultiTenantSaaS.Infrastructure.csproj", "MultiTenantSaaS.Infrastructure/"]
COPY ["MultiTenantSaaS.Tests/MultiTenantSaaS.Tests.csproj", "MultiTenantSaaS.Tests/"]

# Restore dependencies
RUN dotnet restore "MultiTenantSaaS.sln"

# Copy all source files
COPY . .

# Build the solution
WORKDIR "/src/MultiTenantSaaS.API"
RUN dotnet build "MultiTenantSaaS.API.csproj" -c Release -o /app/build

# Stage 2: Test
FROM build AS test
WORKDIR /src
RUN dotnet test "MultiTenantSaaS.Tests/MultiTenantSaaS.Tests.csproj" \
    -c Release \
    --logger "trx;LogFileName=test-results.trx" \
    --collect:"XPlat Code Coverage" \
    --no-build \
    --no-restore

# Stage 3: Publish
FROM build AS publish
WORKDIR "/src/MultiTenantSaaS.API"
RUN dotnet publish "MultiTenantSaaS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create a non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published files
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Configure environment
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/api/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "MultiTenantSaaS.API.dll"]
