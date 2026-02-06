# GitHub Secrets Configuration Guide

This document lists all required GitHub Secrets for the CI/CD pipeline.

## Required Secrets

### AWS Credentials (for EC2 Deployment)
```
AWS_ACCESS_KEY_ID
  Description: AWS access key with EC2 permissions
  Example: AKIAIOSFODNN7EXAMPLE

AWS_SECRET_ACCESS_KEY
  Description: AWS secret access key
  Example: wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY

AWS_REGION
  Description: AWS region where EC2 instance is located
  Example: us-east-1
```

### EC2 Instance Configuration
```
EC2_HOST
  Description: Public IP or hostname of EC2 instance
  Example: 54.123.456.789 or api.yourdomain.com

EC2_USERNAME
  Description: SSH username for EC2 instance
  Example: ubuntu (for Ubuntu) or ec2-user (for Amazon Linux)

EC2_SSH_KEY
  Description: Private SSH key for EC2 access (entire key content)
  Example:
  -----BEGIN RSA PRIVATE KEY-----
  MIIEpAIBAAKCAQEA...
  -----END RSA PRIVATE KEY-----
```

### Docker Hub Credentials
```
DOCKER_USERNAME
  Description: Docker Hub username
  Example: yourusername

DOCKER_PASSWORD
  Description: Docker Hub password or access token
  Note: Use access token for better security
```

### SonarCloud Configuration
```
SONAR_TOKEN
  Description: SonarCloud authentication token
  How to get: https://sonarcloud.io/account/security
  Example: sqp_1234567890abcdef
```

### Database Configuration
```
DB_CONNECTION_STRING
  Description: Production database connection string
  Example: Server=prod-db.example.com;Database=MultiTenantSaaS;User Id=sa;Password=YourPassword;TrustServerCertificate=True

AUDIT_DB_CONNECTION_STRING
  Description: Production audit database connection string
  Example: Server=prod-db.example.com;Database=MultiTenantSaaSAudit;User Id=sa;Password=YourPassword;TrustServerCertificate=True
```

### Application Configuration
```
JWT_SECRET
  Description: Secret key for JWT token generation (min 32 characters)
  Example: YourSuperSecretKeyForJWTTokenGeneration12345!
```

## How to Add Secrets to GitHub

1. Go to your GitHub repository
2. Click on **Settings** tab
3. In the left sidebar, click **Secrets and variables** → **Actions**
4. Click **New repository secret**
5. Enter the secret name and value
6. Click **Add secret**

## Security Best Practices

- ✅ Use access tokens instead of passwords where possible
- ✅ Rotate secrets regularly (every 90 days recommended)
- ✅ Use separate credentials for production and staging
- ✅ Never commit secrets to the repository
- ✅ Use environment-specific secrets
- ✅ Review and audit secret access regularly

## Validation

After adding all secrets, the CI/CD pipeline will fail with clear error messages if any required secret is missing.
