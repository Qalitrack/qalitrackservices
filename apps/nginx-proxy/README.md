# Nginx Proxy with Smart SSL Management

Multi-domain nginx reverse proxy with Docker network integration, intelligent SSL certificate management, and automatic configuration.

## ✅ Smart SSL Architecture

```
Internet → nginx-proxy → Multiple App Networks
        (Port 80/443)     ├── Dev Network (qs-frontend-dev, qs-backend-dev)  
                         └── Prod Network (qs-frontend-prod, qs-backend-prod)
```

**Key Features:**
- ✅ **Automatic SSL Detection**: Requests certificates only when needed
- ✅ **Multi-Network Support**: Connects to multiple Docker networks automatically
- ✅ **Intelligent Configuration**: HTTPS if certificate exists, HTTP if not
- ✅ **Template System**: Enable/disable domains via templates
- ✅ **No Manual Intervention**: Smart certificate management

## Usage

### 1. Start Nginx Proxy

```bash
cd apps/nginx-proxy
docker compose up -d
```

The proxy automatically:
- Connects to all configured Docker networks
- Requests SSL certificates for enabled domains without certificates
- Configures nginx based on certificate availability

### 2. Enable SSL for New Domains

```bash
# Copy SSL template to enabled directory
cp templates/available/ssl/qalibrated.co.ke.conf templates/enabled/ssl/qalibrated.co.ke.conf

# Copy HTTP template for fallback
cp templates/available/no-ssl/qalibrated.co.ke.conf templates/enabled/no-ssl/qalibrated.co.ke.conf

# Restart nginx - it will automatically request SSL and configure appropriately
docker compose restart nginx-proxy
```

### 3. Disable SSL for Domains

```bash
# Remove SSL template (keeps HTTP)
rm templates/enabled/ssl/qalibrated.co.ke.conf

# Restart nginx
docker compose restart nginx-proxy
```

## Directory Structure

```
apps/nginx-proxy/
├── docker-compose.yml              # Multi-network nginx with auto-SSL
├── nginx.conf                      # Base nginx configuration
├── entrypoint.sh                   # Smart SSL management script
├── Dockerfile                      # Custom nginx + certbot image
├── templates/
│   ├── available/                  # Available domain templates
│   │   ├── ssl/                   # HTTPS configurations
│   │   │   ├── qalibrated.co.ke.conf
│   │   │   └── dev.qalibrated.co.ke.conf
│   │   └── no-ssl/                # HTTP configurations  
│   │       ├── qalibrated.co.ke.conf
│   │       └── dev.qalibrated.co.ke.conf
│   └── enabled/                    # Active domain templates
│       ├── ssl/                   # Domains that want SSL
│       └── no-ssl/                # HTTP fallback configs
├── ssl-certs/                      # Let's Encrypt certificates
├── certbot-webroot/               # ACME challenge files
└── logs/                          # Nginx access/error logs
```

## Domain Configuration

| Domain | Network | Frontend | Backend | SSL Status |
|--------|---------|----------|---------|------------|
| qalibrated.co.ke | website_qs-network-prod | qs-frontend-prod | qs-backend-prod | Auto-requested |
| dev.qalibrated.co.ke | website_dev_qs-network | qs-frontend-dev | qs-backend-dev | ✅ Active |

## Smart SSL Workflow

### Startup Process:
1. **Check Missing Certificates**: Only request SSL for domains that need it but don't have it
2. **Request Certificates**: Try to obtain missing SSL certificates from Let's Encrypt
3. **Configure Domains**: 
   - SSL template + Certificate exists → **HTTPS config**
   - SSL template + No certificate → **HTTP config** 
   - HTTP-only template → **HTTP config**
4. **Start Nginx**: With appropriate configuration based on certificate availability

### Example Startup Log:
```
🔐 Checking for missing SSL certificates...
✓ Domain dev.qalibrated.co.ke already has SSL certificate
📋 Domain qalibrated.co.ke needs SSL certificate - requesting...
❌ SSL certificate request failed for qalibrated.co.ke - will use HTTP
🔧 SSL certificate requests completed. Processing domain configurations...
📋 SSL template exists for dev.qalibrated.co.ke
✓ SSL certificate found for dev.qalibrated.co.ke - using HTTPS config
📋 SSL template exists for qalibrated.co.ke  
○ No SSL certificate for qalibrated.co.ke - using HTTP config
✓ Nginx configuration is valid
Configuration complete. Starting nginx...
```

## Network Configuration

The nginx proxy automatically connects to multiple Docker networks defined in `docker-compose.yml`:

```yaml
networks:
  - proxy-network           # Internal proxy network
  - website_qs-network-prod # Production app network  
  - website_dev_qs-network  # Development app network
```

**External Networks**: App networks are marked as `external: true` so they must exist before starting nginx.

## SSL Certificate Management

### Automatic Features:
- ✅ **Smart Detection**: Only requests certificates for domains that need them
- ✅ **Graceful Fallback**: Uses HTTP config if SSL request fails
- ✅ **Rate Limit Awareness**: Handles Let's Encrypt rate limits gracefully
- ✅ **Permission Management**: Proper certbot webroot permissions

### Certificate Storage:
- **Certificates**: `./ssl-certs` → `/etc/letsencrypt`
- **Challenge Files**: `./certbot-webroot` → `/var/www/certbot`
- **Permissions**: Ensure `certbot-webroot` is owned by nginx user, not root

### Manual Certificate Request:
```bash
# Request certificate manually (if needed)
docker exec nginx-main-proxy certbot certonly \
  --webroot --webroot-path=/var/www/certbot \
  --email admin@qalibrated.co.ke \
  --agree-tos --no-eff-email \
  -d your-domain.com --non-interactive
```

## Troubleshooting

### Check SSL Certificate Status:
```bash
# View all certificates
docker exec nginx-main-proxy certbot certificates

# Check specific domain certificate
docker exec nginx-main-proxy ls -la /etc/letsencrypt/live/
```

### Debug SSL Certificate Requests:
```bash
# Check nginx logs
docker logs nginx-main-proxy

# Check Let's Encrypt logs  
docker exec nginx-main-proxy cat /var/log/letsencrypt/letsencrypt.log

# Test ACME challenge path
curl http://your-domain.com/.well-known/acme-challenge/test
```

### Fix Common Issues:

**Permission Issues:**
```bash
# Fix certbot webroot permissions
sudo chown -R brian:qalitrack /opt/qalitrack/qalitrackservices/apps/nginx-proxy/certbot-webroot
```

**Network Issues:**
```bash
# Check network connections
docker inspect nginx-main-proxy | grep NetworkMode
docker network ls | grep website
```

**Configuration Issues:**
```bash
# Test nginx configuration
docker exec nginx-main-proxy nginx -t

# Reload nginx after manual changes  
docker exec nginx-main-proxy nginx -s reload
```

## Adding New Domains

1. **Create Templates**: Copy existing templates and modify for new domain
   ```bash
   # SSL template
   cp templates/available/ssl/qalibrated.co.ke.conf templates/available/ssl/new-domain.com.conf
   # HTTP template  
   cp templates/available/no-ssl/qalibrated.co.ke.conf templates/available/no-ssl/new-domain.com.conf
   ```

2. **Edit Templates**: Update server_name, container names, and API paths

3. **Enable Domain**: Copy to enabled directory
   ```bash
   cp templates/available/ssl/new-domain.com.conf templates/enabled/ssl/new-domain.com.conf
   cp templates/available/no-ssl/new-domain.com.conf templates/enabled/no-ssl/new-domain.com.conf
   ```

4. **Connect Network**: Add domain's network to docker-compose.yml if needed

5. **Restart**: `docker compose restart nginx-proxy`

## Security Features

- ✅ **HSTS Headers**: Strict-Transport-Security for HTTPS domains
- ✅ **Security Headers**: X-Frame-Options, X-Content-Type-Options, X-XSS-Protection
- ✅ **SSL Optimization**: Modern TLS protocols and ciphers
- ✅ **CORS Support**: Proper CORS headers for API endpoints
- ✅ **Network Isolation**: Apps only accessible through proxy, no direct port exposure

## Why This Approach

**Before (Manual SSL):**
- Manual certificate requests
- Complex nginx reloads
- No fallback for SSL failures
- Difficult to manage multiple domains

**Now (Smart SSL):**
- Automatic certificate management
- Graceful fallback to HTTP
- Template-based configuration
- Multi-network support
- Zero-downtime SSL deployment