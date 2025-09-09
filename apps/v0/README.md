# QaliTrack Microservices Platform

A comprehensive microservices platform for QaliTrack with integrated secret management, object storage, and search capabilities.

## 🚀 Quick Start

```bash
# Clone the repository
git clone <repository-url>
cd qalitrackservices/apps/v0

# Start all services
docker-compose up -d

# Check service status
docker-compose ps
```

## 📋 Services Overview

### Application Services
- **Dashboard**: http://localhost:3000 - Microservices Control Hub
- **QaliTrack MasterData**: http://localhost:7001 - Consolidated master data service
- **Transaction Service**: http://localhost:7002 - Transaction processing
- **Weight Data Service**: http://localhost:7003 - Weight data management
- **Backup Service**: http://localhost:7005 - Data backup and recovery

### Infrastructure Services
- **PostgreSQL**: localhost:5432 (admin/password)
- **Redis**: localhost:6379
- **RabbitMQ Management**: http://localhost:15672 (admin/password)
- **Mailpit**: http://localhost:8025 - Email testing
- **HashiCorp Vault**: http://localhost:8200 - Secret management
- **MinIO Console**: http://localhost:9001 (admin/password123) - Object storage
- **MinIO API**: http://localhost:9000 (admin/password123)
- **ZincSearch**: http://localhost:4080 (admin/password123) - Search engine

## 🔐 Vault Secret Management

### Overview

HashiCorp Vault is integrated to securely manage all secrets including database passwords, API keys, and service credentials. Vault automatically initializes with all required secrets when the platform starts.

### Vault Setup and Configuration

#### 1. Automatic Initialization

When you run `docker-compose up -d`, Vault automatically:
- Starts in development mode with root token `myroot`
- Runs the initialization script (`vault-init.sh`)
- Creates AppRole authentication method
- Sets up policies for service access
- Stores all infrastructure secrets

#### 2. Manual Vault Operations

Access Vault UI: http://localhost:8200
- **Token**: `myroot` (development only)

```bash
# Access Vault CLI in container
docker-compose exec vault vault status

# List all secrets
docker-compose exec vault vault kv list secret/

# Read a specific secret
docker-compose exec vault vault kv get secret/database/masterdata
```

### Secrets Stored in Vault

#### Database Secrets
- `secret/database/masterdata` - MasterData service connection
- `secret/database/backup` - Backup service connection
- `secret/database/postgres` - PostgreSQL admin credentials

#### Email Configuration
- `secret/email/smtp` - SMTP server configuration

#### Infrastructure Services
- `secret/services/rabbitmq` - RabbitMQ credentials
- `secret/services/minio` - MinIO access keys
- `secret/services/zincsearch` - ZincSearch credentials

#### Authentication
- `secret/auth/approle` - AppRole credentials for services

### Using Vault in .NET Services

#### 1. Add VaultSharp NuGet Package

```bash
dotnet add package VaultSharp
```

#### 2. Configure Vault in Startup.cs/Program.cs

```csharp
using QaliTrack.Services.Vault;

// In ConfigureServices or Program.cs
services.AddVaultIntegration(configuration);
```

#### 3. Environment Variables (Already Configured)

Each service automatically receives:
```bash
VAULT__ADDRESS=http://vault:8200
VAULT__ROLENAME=qalitrack-services
```

#### 4. Use Vault in Your Services

```csharp
public class MyService
{
    private readonly VaultService _vaultService;
    
    public MyService(VaultService vaultService)
    {
        _vaultService = vaultService;
    }
    
    public async Task<string> GetDatabaseConnectionAsync()
    {
        // Get single secret value
        var connectionString = await _vaultService.GetSecretAsync("database/masterdata");
        
        // Or get all secrets from a path
        var secrets = await _vaultService.GetSecretsAsync("database/masterdata");
        return secrets["connection_string"].ToString();
    }
}
```

#### 5. Replace Hardcoded Secrets

Instead of:
```csharp
var connectionString = "Host=postgres;Database=masterdata;Username=user;Password=pass";
```

Use:
```csharp
var secrets = await _vaultService.GetSecretsAsync("database/masterdata");
var connectionString = secrets["connection_string"].ToString();
```

### Managing Infrastructure Secrets

#### Database Credentials
```bash
# Update PostgreSQL password
docker-compose exec vault vault kv put secret/database/postgres \
    admin_user="admin" \
    admin_password="new_secure_password"

# Update MasterData database connection
docker-compose exec vault vault kv put secret/database/masterdata \
    connection_string="Host=postgres;Database=masterdata;Username=masterdata;Password=new_password;..."
```

#### Email Configuration
```bash
# Update SMTP settings
docker-compose exec vault vault kv put secret/email/smtp \
    from_email="noreply@yourdomain.com" \
    smtp_host="your-smtp-server" \
    smtp_port="587" \
    smtp_username="your-username" \
    smtp_password="your-password" \
    enable_ssl="true"
```

#### Service Credentials
```bash
# Update MinIO credentials
docker-compose exec vault vault kv put secret/services/minio \
    root_user="admin" \
    root_password="your_secure_password"

# Update RabbitMQ credentials  
docker-compose exec vault vault kv put secret/services/rabbitmq \
    default_user="admin" \
    default_password="your_secure_password"
```

### Vault Security Best Practices

#### For Development
- Root token `myroot` is acceptable
- Auto-unseal is enabled
- Data is stored in Docker volumes

#### For Production
1. **Remove development mode**:
   ```yaml
   # Update vault service in docker-compose.yml
   command: ["vault", "server", "-config=/vault/config"]
   ```

2. **Use proper authentication**:
   - Replace root token with proper auth methods
   - Implement token rotation
   - Use TLS certificates

3. **Secure storage**:
   - Use external storage backend (Consul, cloud storage)
   - Enable encryption at rest
   - Regular backups

4. **Network security**:
   - Restrict Vault access to internal network
   - Use TLS for all connections
   - Implement proper firewall rules

### Vault Policies and Access Control

#### Service Policy (qalitrack-services)
```hcl
# Allows services to read secrets
path "secret/data/database/*" {
  capabilities = ["read"]
}
path "secret/data/email/*" {
  capabilities = ["read"]  
}
path "secret/data/services/*" {
  capabilities = ["read"]
}
```

#### Creating Custom Policies
```bash
# Create a policy for a specific service
docker-compose exec vault vault policy write backup-service - <<EOF
path "secret/data/database/backup" {
  capabilities = ["read"]
}
path "secret/data/email/*" {
  capabilities = ["read"]
}
EOF

# Create AppRole for the service
docker-compose exec vault vault write auth/approle/role/backup-service \
    token_policies="backup-service" \
    token_ttl=1h \
    token_max_ttl=4h
```

## 🗄️ Object Storage with MinIO

MinIO provides S3-compatible object storage for file uploads, backups, and media.

### Accessing MinIO
- **Console**: http://localhost:9001 (admin/password123)
- **API**: http://localhost:9000 (admin/password123)

### Using MinIO in Services

```csharp
// Add MinIO client
services.AddMinio(options =>
{
    var minioSecrets = vaultService.GetSecretsAsync("services/minio").Result;
    options.WithEndpoint("minio:9000")
           .WithCredentials(minioSecrets["root_user"], minioSecrets["root_password"])
           .WithSSL(false);
});
```

## 🔍 Search with ZincSearch

ZincSearch provides full-text search capabilities for your applications.

### Accessing ZincSearch
- **Web UI**: http://localhost:4080 (admin/password123)

### Using ZincSearch in Services

```csharp
public class SearchService
{
    private readonly HttpClient _httpClient;
    private readonly VaultService _vaultService;

    public async Task IndexDocumentAsync(string index, object document)
    {
        var secrets = await _vaultService.GetSecretsAsync("services/zincsearch");
        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{secrets["admin_user"]}:{secrets["admin_password"]}")
        );
        
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Basic", credentials);
            
        var json = JsonSerializer.Serialize(document);
        await _httpClient.PostAsync($"http://zincsearch:4080/api/{index}/_doc", 
            new StringContent(json, Encoding.UTF8, "application/json"));
    }
}
```

## 🔧 Development

### Adding New Services

1. **Add service to docker-compose.yml**
2. **Configure Vault integration**:
   ```yaml
   environment:
     - Vault__Address=http://vault:8200
     - Vault__RoleName=qalitrack-services
   depends_on:
     - vault-init
   ```

3. **Add service-specific secrets to Vault**:
   ```bash
   docker-compose exec vault vault kv put secret/services/myservice \
       api_key="secret_key" \
       database_url="connection_string"
   ```

### Environment Management

#### Development
```bash
docker-compose up -d
```

#### Production
```bash
# Use production overrides
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## 📊 Monitoring and Troubleshooting

### Health Checks
```bash
# Check all services
docker-compose ps

# Check specific service logs
docker-compose logs -f qalitrack-masterdata

# Check Vault status
docker-compose exec vault vault status
```

### Common Issues

#### Vault Authentication Failures
```bash
# Check vault-init logs
docker-compose logs vault-init

# Manually get AppRole credentials
docker-compose exec vault vault read auth/approle/role/qalitrack-services/role-id
```

#### Service Startup Issues
1. Ensure vault-init completed successfully
2. Check service logs for authentication errors
3. Verify secrets exist in Vault
4. Confirm network connectivity between services

#### Database Connection Issues
```bash
# Test PostgreSQL connection
docker-compose exec postgres psql -U admin -d microservices

# Check database secrets in Vault
docker-compose exec vault vault kv get secret/database/masterdata
```

## 🚀 Deployment

### Docker Swarm
```bash
# Initialize swarm
docker swarm init

# Deploy stack
docker stack deploy -c docker-compose.yml qalitrack
```

### Kubernetes
1. Convert docker-compose to Kubernetes manifests
2. Set up Vault operator for Kubernetes
3. Configure secrets management with Vault Agent

## 📝 Configuration Files

- `docker-compose.yml` - Main services configuration
- `vault-init.sh` - Vault initialization script
- `get-vault-credentials.sh` - Service authentication helper
- `VaultIntegrationExample.cs` - .NET integration example

## 🔒 Security Notes

- **Development mode**: Vault uses root token `myroot`
- **Production**: Replace with proper authentication and TLS
- **Secrets rotation**: Implement regular credential rotation
- **Network security**: Use internal Docker networks
- **Backup**: Regular Vault data backups required

## 📄 License

[Your License Here]

## 🤝 Contributing

[Contributing Guidelines Here]