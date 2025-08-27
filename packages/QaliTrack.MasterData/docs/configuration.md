# Configuration

The QaliTrack Master Data Service supports flexible configuration through multiple sources including appsettings files, environment variables, and Docker Compose configurations.

## Configuration Sources (Priority Order)

1. **Command Line Arguments** (highest priority)
2. **Environment Variables**
3. **Docker Compose Environment**
4. **appsettings.{Environment}.json**
5. **appsettings.json** (lowest priority)

## Database Configuration

### Connection Strings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=masterdata;Username=postgres;Password=password;Pooling=true;MinPoolSize=5;MaxPoolSize=100"
  }
}
```

### Environment Variables

```bash
# Docker/Production
ConnectionStrings__DefaultConnection="Host=postgres;Database=masterdata;Username=masterdata;Password=masterdata123;Pooling=true;MinPoolSize=5;MaxPoolSize=100;Include Error Detail=true;Command Timeout=60"

# Development
ASPNETCORE_ENVIRONMENT=Development
```

### Supported Databases

- **PostgreSQL** (Production)
- **SQLite** (Development/Testing)
- **SQL Server** (Enterprise)

## Logging Configuration

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "QaliTrack.MasterData": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/masterdata-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

## API Configuration

### Swagger/OpenAPI

```json
{
  "Swagger": {
    "Title": "QaliTrack Master Data API",
    "Version": "v1",
    "Description": "Consolidated Master Data Service API",
    "Contact": {
      "Name": "QaliTrack Team",
      "Email": "support@qalitrack.com"
    }
  }
}
```

### CORS Settings

```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "https://app.qalitrack.com"],
    "AllowedMethods": ["GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"],
    "AllowedHeaders": ["*"],
    "AllowCredentials": true
  }
}
```

## Authentication & Authorization

### JWT Configuration

```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-jwt-signing-key-min-256-bits",
    "Issuer": "QaliTrack.MasterData",
    "Audience": "QaliTrack.Clients",
    "ExpirationMinutes": 60
  }
}
```

### Environment Variables for JWT

```bash
# Production - use environment variables for security
JWT__SECRETKEY="production-secret-key-from-key-vault"
JWT__ISSUER="QaliTrack.Production"
JWT__AUDIENCE="QaliTrack.Production.Clients"
```

## Performance Configuration

### Entity Framework

```json
{
  "EntityFramework": {
    "CommandTimeout": 30,
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "MaxRetryCount": 3,
    "MaxRetryDelay": "00:00:30"
  }
}
```

### Caching Configuration

```json
{
  "Caching": {
    "DefaultExpiration": "00:15:00",
    "SlidingExpiration": "00:05:00",
    "AbsoluteExpirationRelativeToNow": "01:00:00"
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "Database": 0,
    "InstanceName": "QaliTrackMasterData"
  }
}
```

## Health Checks Configuration

```json
{
  "HealthChecks": {
    "UI": {
      "HealthCheckDatabaseConnectionString": "Host=localhost;Database=healthchecksdb;Username=healthcheck;Password=healthcheck123",
      "MaximumHistoryEntriesPerEndpoint": 50
    }
  }
}
```

## Module-Specific Configuration

### Business Entities

```json
{
  "BusinessEntities": {
    "EnableCustomerProfiles": true,
    "EnableSupplierProfiles": true,
    "EnableTransporterProfiles": true,
    "RequireContactValidation": true
  }
}
```

### Vehicles

```json
{
  "Vehicles": {
    "RequireInsurance": true,
    "RequireRegistration": true,
    "EnableMaintenanceScheduling": true,
    "DefaultMaintenanceInterval": 90
  }
}
```

## Environment-Specific Configurations

### Development (`appsettings.Development.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "QaliTrack.MasterData": "Trace"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=masterdata_dev.db"
  },
  "EnableSwagger": true,
  "EnableDetailedErrors": true
}
```

### Production (`appsettings.Production.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "QaliTrack.MasterData": "Information"
    }
  },
  "EnableSwagger": false,
  "EnableDetailedErrors": false,
  "RequireHttps": true
}
```

## Docker Configuration

### Docker Compose Environment

```yaml
services:
  qalitrack-masterdata:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=masterdata;Username=masterdata;Password=masterdata123;Pooling=true
      - JWT__SECRETKEY=${JWT_SECRET_KEY}
      - Redis__ConnectionString=redis:6379
    depends_on:
      - postgres
      - redis
```

### Environment File (`.env`)

```bash
# Database
DB_HOST=localhost
DB_NAME=masterdata
DB_USER=masterdata
DB_PASSWORD=masterdata123

# Authentication
JWT_SECRET_KEY=your-production-secret-key
JWT_ISSUER=QaliTrack.Production

# External Services
REDIS_CONNECTION_STRING=redis:6379
SMTP_HOST=smtp.example.com
SMTP_PORT=587
SMTP_USERNAME=noreply@qalitrack.com
SMTP_PASSWORD=smtp-password
```

## Configuration Validation

The service validates configuration on startup:

```csharp
public static class ConfigurationValidator
{
    public static void ValidateConfiguration(IConfiguration configuration)
    {
        // Validate required connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Database connection string is required");

        // Validate JWT configuration
        var jwtSection = configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"];
        if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters");
    }
}
```

## Configuration Best Practices

1. **Never store secrets in appsettings.json**
2. **Use environment variables for sensitive data**
3. **Use Azure Key Vault or similar for production secrets**
4. **Validate configuration on startup**
5. **Use typed configuration classes with IOptions<T>**

### Typed Configuration Example

```csharp
public class DatabaseOptions
{
    public const string SectionName = "Database";
    
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public int MaxRetryCount { get; set; } = 3;
    public bool EnableSensitiveDataLogging { get; set; } = false;
}

// Registration
services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

// Usage
public class SomeService
{
    private readonly DatabaseOptions _options;
    
    public SomeService(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }
}
```