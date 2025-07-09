#!/usr/bin/env python3
"""
QaliTrack Deployment Generator

Generates Docker Compose files and deployment configurations 
based on client configuration files.
"""

import yaml
import os
import sys
import argparse
import subprocess
import json
from pathlib import Path
from typing import Dict, Any, List

class DeploymentGenerator:
    def __init__(self, config_path: str, output_dir: str = "deployments"):
        self.config_path = config_path
        self.output_dir = output_dir
        self.config = self._load_config()
        
    def _load_config(self) -> Dict[str, Any]:
        """Load client configuration from YAML file"""
        try:
            with open(self.config_path, 'r') as file:
                return yaml.safe_load(file)
        except Exception as e:
            print(f"Error loading configuration: {e}")
            sys.exit(1)
    
    def generate_docker_compose(self) -> str:
        """Generate Docker Compose configuration"""
        client_code = self.config['client']['code']
        
        compose = {
            'version': '3.8',
            'services': {},
            'volumes': {},
            'networks': {
                f'qalitrack-{client_code}': {
                    'driver': 'bridge'
                }
            }
        }
        
        # Add enabled services
        for service_name, service_config in self.config['services'].items():
            if service_config.get('enabled', False):
                compose['services'][service_name] = self._generate_service_config(
                    service_name, service_config
                )
                
                # Add volume if service needs persistence
                if self._service_needs_volume(service_name):
                    volume_name = f"{service_name.replace('-', '_')}_data"
                    compose['volumes'][volume_name] = None
        
        return yaml.dump(compose, default_flow_style=False, sort_keys=False)
    
    def _generate_service_config(self, service_name: str, service_config: Dict[str, Any]) -> Dict[str, Any]:
        """Generate Docker Compose service configuration"""
        client_code = self.config['client']['code']
        environment = self.config['deployment']['environment']
        
        build_config = {
            'context': self._get_service_context(service_name),
            'dockerfile': 'Dockerfile'
        }
        
        # Special case for user-service with different dockerfile path
        if service_name == 'user-service':
            build_config['dockerfile'] = 'UserModule/Dockerfile'
        
        service_def = {
            'build': build_config,
            'ports': [f"{service_config['port']}:80"],
            'environment': self._get_service_environment(service_name, environment),
            'networks': [f'qalitrack-{client_code}'],
            'restart': 'unless-stopped'
        }
        
        # Add health check (disabled for testing - services don't have health endpoints)
        # service_def['healthcheck'] = {
        #     'test': ['CMD', 'wget', '--quiet', '--tries=1', '--spider', 'http://localhost:80/health'],
        #     'interval': '30s',
        #     'timeout': '10s',
        #     'retries': 3,
        #     'start_period': '40s'
        # }
        
        # Add volume mounts if needed
        volumes = []
        if self._service_needs_volume(service_name):
            volume_name = f"{service_name.replace('-', '_')}_data"
            volumes.extend([
                f"{volume_name}:/data",
                f"./{service_name}/logs:/app/logs"
            ])
        
        # Add config file mounting for services that need client configuration
        if service_name in ['gateway', 'swagger-aggregator']:
            client_code = self.config['client']['code']
            volumes.append(f"../../configs/clients/{client_code}.yml:/app/configs/clients/{client_code}.yml:ro")
        
        # Add auth config mounting for gateway
        if service_name == 'gateway':
            client_code = self.config['client']['code']
            volumes.append(f"../../configs/auth/{client_code}-ocelot.json:/app/ocelot.json:ro")
        
        if volumes:
            service_def['volumes'] = volumes
        
        # Add dependencies (without health checks for now)
        dependencies = self._get_service_dependencies(service_name)
        if dependencies:
            service_def['depends_on'] = {}
            for dep in dependencies:
                if self.config['services'].get(dep, {}).get('enabled', False):
                    service_def['depends_on'][dep] = {'condition': 'service_started'}
        
        # Add scaling
        if service_config.get('replicas', 1) > 1:
            service_def['deploy'] = {
                'replicas': service_config['replicas']
            }
        
        return service_def
    
    def _get_service_context(self, service_name: str) -> str:
        """Get build context path for service (relative to project root)"""
        if service_name == 'gateway':
            return '../../packages/qalitrack-gateway'
        elif service_name == 'service-discovery':
            return '../../packages/service-discovery'
        elif service_name == 'swagger-aggregator':
            return '../../packages/swagger-aggregator'
        elif service_name == 'user-service':
            # User service has a different structure
            return '../../packages/microservices/masterdata/user-service'
        elif service_name in ['organization-service', 'vehicle-service', 
                            'driver-service', 'product-service', 'route-service', 
                            'weighbridge-service', 'customer-service', 'supplier-service', 
                            'transporter-service', 'sacco-service']:
            return f'../../packages/microservices/masterdata/{service_name}'
        else:
            return f'../../packages/microservices/datamanager/{service_name}'
    
    def _get_service_environment(self, service_name: str, environment: str) -> List[str]:
        """Get environment variables for service"""
        env_vars = [
            f'ASPNETCORE_ENVIRONMENT={environment.title()}',
            f'CLIENT_CODE={self.config["client"]["code"]}'
        ]
        
        # Override port binding for .NET 8 services
        if service_name == 'swagger-aggregator':
            env_vars.extend([
                'ASPNETCORE_URLS=http://+:80',
                'ASPNETCORE_HTTP_PORTS=80'
            ])
        
        # Database connection
        if self._service_needs_database(service_name):
            db_name = service_name.replace('-', '')
            env_vars.append(f'ConnectionStrings__DefaultConnection=Data Source=/data/{db_name}.db')
        
        # JWT configuration for auth services
        if service_name in ['gateway', 'user-service']:
            env_vars.extend([
                'Jwt__SecretKey=ProductionSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!',
                'Jwt__Issuer=UserService',
                'Jwt__Audience=UserService'
            ])
        
        return env_vars
    
    def _service_needs_volume(self, service_name: str) -> bool:
        """Check if service needs persistent volume"""
        return service_name not in ['gateway', 'swagger-aggregator']
    
    def _service_needs_database(self, service_name: str) -> bool:
        """Check if service needs database connection"""
        return service_name not in ['gateway', 'swagger-aggregator']
    
    def _get_service_dependencies(self, service_name: str) -> List[str]:
        """Get service dependencies"""
        dependencies = {
            'gateway': ['user-service'],
            'swagger-aggregator': [],
            'user-service': [],
            'organization-service': ['user-service'],
            'vehicle-service': ['user-service', 'organization-service'],
            'driver-service': ['user-service', 'organization-service'],
            'product-service': ['user-service', 'organization-service'],
            'route-service': ['user-service', 'organization-service'],
            'weighbridge-service': ['user-service', 'organization-service'],
            'customer-service': ['user-service', 'organization-service'],
            'supplier-service': ['user-service', 'organization-service', 'product-service'],
            'transporter-service': ['user-service', 'organization-service', 'vehicle-service'],
            'sacco-service': ['user-service', 'organization-service'],
            'weight-data-service': ['user-service', 'weighbridge-service', 'vehicle-service'],
            'compliance-service': ['user-service', 'vehicle-service', 'driver-service'],
            'operational-data-service': ['user-service', 'product-service', 'route-service'],
            'transaction-service': ['user-service', 'customer-service', 'product-service'],
            'analytics-service': ['user-service', 'transaction-service', 'weight-data-service'],
            'data-sync-service': ['user-service'],
            'archive-service': ['user-service']
        }
        
        return dependencies.get(service_name, [])
    
    def generate_auth_config(self):
        """Generate client-specific authorization configuration"""
        client_code = self.config['client']['code']
        
        # Create auth configs directory if it doesn't exist
        auth_config_dir = Path("configs/auth")
        auth_config_dir.mkdir(parents=True, exist_ok=True)
        
        # Get enabled services for this client
        enabled_services = [name for name, config in self.config['services'].items() 
                          if config.get('enabled', False)]
        
        # Load base auth rules template
        auth_rules_file = Path("configs/auth/auth-rules-template.yml")
        if not auth_rules_file.exists():
            print(f"⚠️  Warning: Auth rules template not found at {auth_rules_file}")
            print("   Creating minimal auth configuration...")
            self._create_minimal_auth_config(client_code, enabled_services)
            return
        
        try:
            with open(auth_rules_file, 'r') as f:
                auth_rules = yaml.safe_load(f)
        except Exception as e:
            print(f"⚠️  Warning: Could not load auth rules template: {e}")
            print("   Creating minimal auth configuration...")
            self._create_minimal_auth_config(client_code, enabled_services)
            return
        
        # Generate client-specific Ocelot configuration
        ocelot_config = self._generate_ocelot_config(auth_rules, enabled_services)
        
        # Save client-specific auth config
        auth_config_file = auth_config_dir / f"{client_code}-ocelot.json"
        with open(auth_config_file, 'w') as f:
            json.dump(ocelot_config, f, indent=2)
        
        print(f"✅ Generated auth configuration: {auth_config_file}")
    
    def _create_minimal_auth_config(self, client_code: str, enabled_services: List[str]):
        """Create minimal auth configuration when template is not available"""
        # Basic Ocelot configuration with minimal auth
        minimal_config = {
            "Routes": [],
            "GlobalConfiguration": {
                "BaseUrl": "http://localhost:7000",
                "ServiceDiscoveryProvider": {
                    "Type": "ConfigurationServiceProvider",
                    "PollingInterval": 1000
                }
            }
        }
        
        # Add routes for enabled services with basic auth
        service_ports = {
            'user-service': 7001,
            'organization-service': 7002,
            'vehicle-service': 7003,
            'driver-service': 7004,
            'product-service': 7005,
            'route-service': 7006,
            'weighbridge-service': 7007,
            'customer-service': 7008,
            'supplier-service': 7009,
            'transporter-service': 7010,
            'sacco-service': 7011,
            'weight-data-service': 7012,
            'compliance-service': 7013,
            'operational-data-service': 7014,
            'transaction-service': 7015,
            'analytics-service': 7016,
            'data-sync-service': 7017,
            'archive-service': 7018
        }
        
        for service_name in enabled_services:
            if service_name == 'gateway':
                continue
                
            port = service_ports.get(service_name, 7000)
            service_path = service_name.replace('-', '')
            
            route = {
                "UpstreamPathTemplate": f"/api/{service_path}/{{everything}}",
                "DownstreamPathTemplate": f"/api/{service_path}/{{everything}}",
                "DownstreamScheme": "http",
                "DownstreamHostAndPorts": [
                    {"Host": service_name, "Port": 8080 if service_name == "user-service" else 80}
                ],
                "Metadata": {
                    "RequiredRoles": ["User"],
                    "ServiceName": service_name,
                    "Description": f"{service_name} endpoints - requires User+ role"
                }
            }
            
            # Add auth for non-public services
            if service_name != 'service-discovery':
                route["AuthenticationOptions"] = {
                    "AuthenticationProviderKey": "Bearer"
                }
            
            minimal_config["Routes"].append(route)
        
        # Add public auth routes
        auth_route = {
            "UpstreamPathTemplate": "/api/auth/{everything}",
            "DownstreamPathTemplate": "/api/auth/{everything}",
            "DownstreamScheme": "http",
            "DownstreamHostAndPorts": [
                {"Host": "user-service", "Port": 8080}
            ],
            "Metadata": {
                "ServiceName": "user-service",
                "Description": "Authentication endpoints - public access"
            }
        }
        minimal_config["Routes"].append(auth_route)
        
        # Save minimal config
        auth_config_dir = Path("configs/auth")
        auth_config_file = auth_config_dir / f"{client_code}-ocelot.json"
        with open(auth_config_file, 'w') as f:
            json.dump(minimal_config, f, indent=2)
    
    def _generate_ocelot_config(self, auth_rules: Dict[str, Any], enabled_services: List[str]) -> Dict[str, Any]:
        """Generate Ocelot configuration from auth rules for enabled services only"""
        
        ocelot_config = {
            "Routes": [],
            "GlobalConfiguration": {
                "BaseUrl": "http://localhost:7000",
                "ServiceDiscoveryProvider": {
                    "Type": "ConfigurationServiceProvider",
                    "PollingInterval": 1000
                }
            }
        }
        
        # Process only enabled services
        for service_name, service_rules in auth_rules.get('authorization_rules', {}).items():
            if service_name not in enabled_services and service_name != 'public':
                continue
                
            # Get port from config or use default mapping
            port = service_rules.get('port')
            if not port:
                # Use service config port if available
                service_config = self.config['services'].get(service_name, {})
                port = service_config.get('port', 7000)
            
            for rule in service_rules.get('rules', []):
                path = rule['path']
                required_roles = rule.get('roles', ['User'])
                description = rule.get('description', f"{service_name} endpoint")
                
                # Convert path format for Ocelot
                ocelot_path = path.replace('{everything}', '{everything}')
                
                route = {
                    "UpstreamPathTemplate": ocelot_path,
                    "DownstreamPathTemplate": ocelot_path,
                    "DownstreamScheme": "http",
                    "DownstreamHostAndPorts": [
                        {"Host": service_name, "Port": 8080 if service_name == "user-service" else 80}
                    ],
                    "Metadata": {
                        "RequiredRoles": required_roles,
                        "ServiceName": service_name,
                        "Description": description
                    }
                }
                
                # Add authentication unless it's a public endpoint
                if not path.startswith('/api/auth') and not path.startswith('/health'):
                    route["AuthenticationOptions"] = {
                        "AuthenticationProviderKey": "Bearer"
                    }
                
                ocelot_config["Routes"].append(route)
        
        return ocelot_config
    
    def generate_start_script(self) -> str:
        """Generate start script for the deployment"""
        client_name = self.config['client']['name']
        client_code = self.config['client']['code']
        
        enabled_services = [name for name, config in self.config['services'].items() 
                          if config.get('enabled', False)]
        
        script = f'''#!/bin/bash

# Start script for {client_name}
set -e

echo "🚀 Starting {client_name} QaliTrack Services..."

# Colors for output
RED='\\033[0;31m'
GREEN='\\033[0;32m'
YELLOW='\\033[1;33m'
BLUE='\\033[0;34m'
NC='\\033[0m' # No Color

print_status() {{
    echo -e "${{BLUE}}[INFO]${{NC}} $1"
}}

print_success() {{
    echo -e "${{GREEN}}[SUCCESS]${{NC}} $1"
}}

print_error() {{
    echo -e "${{RED}}[ERROR]${{NC}} $1"
}}

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    print_error "Docker is not running. Please start Docker first."
    exit 1
fi

print_status "Starting {len(enabled_services)} services for {client_name}..."

# Set client configuration
export CLIENT_CODE={client_code}

# Start services
if docker compose -f docker-compose.{client_code}.yml up -d --build; then
    print_success "All services started successfully!"
    
    echo ""
    print_status "Service URLs:"
    echo "  🌐 API Gateway:           http://localhost:{self.config['deployment']['ports']['gateway']}"
    echo "  📚 API Documentation:    http://localhost:{self.config['deployment']['ports']['swagger_aggregator']}"
    echo "  💓 Health Check:         http://localhost:{self.config['deployment']['ports']['gateway']}/health"
'''

        # Add individual service URLs
        for service_name, service_config in self.config['services'].items():
            if service_config.get('enabled', False):
                service_display = service_name.replace('-', ' ').title()
                script += f'    echo "  🔧 {service_display}: http://localhost:{service_config["port"]}"\n'

        script += f'''
    
    echo ""
    print_status "Enabled Services ({len(enabled_services)}):"
'''

        for service_name in enabled_services:
            script += f'    echo "  ✅ {service_name}"\n'

        script += '''
    
    echo ""
    print_status "Useful commands:"
    echo "  📋 View logs:            docker compose -f docker-compose.''' + client_code + '''.yml logs -f"
    echo "  🔄 Restart service:      docker compose -f docker-compose.''' + client_code + '''.yml restart [service-name]"
    echo "  🛑 Stop all services:    docker compose -f docker-compose.''' + client_code + '''.yml down"
    echo "  🗑️  Remove all data:      docker compose -f docker-compose.''' + client_code + '''.yml down -v"
    
else
    print_error "Failed to start services"
    exit 1
fi'''

        return script
    
    def generate_all_files(self):
        """Generate all deployment files"""
        client_code = self.config['client']['code']
        output_path = Path("apps") / client_code
        output_path.mkdir(parents=True, exist_ok=True)
        
        # Generate client-specific authorization configuration first
        print(f"🔐 Generating authorization configuration for {self.config['client']['name']}...")
        self.generate_auth_config()
        
        # Generate Docker Compose file
        compose_content = self.generate_docker_compose()
        compose_file = output_path / f"docker-compose.{client_code}.yml"
        with open(compose_file, 'w') as f:
            f.write(compose_content)
        
        # Generate start script
        start_script_content = self.generate_start_script()
        start_script_file = output_path / f"start-{client_code}.sh"
        with open(start_script_file, 'w') as f:
            f.write(start_script_content)
        
        # Make start script executable
        os.chmod(start_script_file, 0o755)
        
        # Generate configuration summary
        self._generate_summary(output_path)
        
        print(f"✅ Generated deployment files for {self.config['client']['name']}:")
        print(f"   📁 Output directory: {output_path}")
        print(f"   🐳 Docker Compose: {compose_file}")
        print(f"   🚀 Start script: {start_script_file}")
        print(f"   📊 Summary: {output_path}/README.md")
        print(f"   🔐 Auth config: configs/auth/{client_code}-ocelot.json")
    
    def _generate_summary(self, output_path: Path):
        """Generate deployment summary documentation"""
        client_name = self.config['client']['name']
        client_code = self.config['client']['code']
        
        enabled_services = [name for name, config in self.config['services'].items() 
                          if config.get('enabled', False)]
        disabled_services = [name for name, config in self.config['services'].items() 
                           if not config.get('enabled', False)]
        
        summary = f"""# {client_name} - QaliTrack Deployment

## Overview
- **Client**: {self.config['client']['name']}
- **Code**: {self.config['client']['code']}
- **Description**: {self.config['client']['description']}
- **Environment**: {self.config['deployment']['environment']}
- **Domain**: {self.config['deployment']['domain']}

## Services ({len(enabled_services)} enabled)

### Enabled Services
"""
        
        for service_name in enabled_services:
            service_config = self.config['services'][service_name]
            summary += f"- **{service_name}** (Port {service_config['port']})"
            if service_config.get('replicas', 1) > 1:
                summary += f" - {service_config['replicas']} replicas"
            summary += "\n"
        
        if disabled_services:
            summary += f"\n### Disabled Services ({len(disabled_services)})\n"
            for service_name in disabled_services:
                service_config = self.config['services'][service_name]
                reason = service_config.get('reason', 'Not required for this deployment')
                summary += f"- **{service_name}** - {reason}\n"
        
        summary += f"""
## Quick Start

```bash
# Start all services
./start-{client_code}.sh

# View logs
docker-compose -f docker-compose.{client_code}.yml logs -f

# Stop services
docker-compose -f docker-compose.{client_code}.yml down
```

## Service URLs

- **API Gateway**: http://localhost:{self.config['deployment']['ports']['gateway']}
- **Swagger Documentation**: http://localhost:{self.config['deployment']['ports']['swagger_aggregator']}

## Features
"""
        
        for feature, enabled in self.config.get('features', {}).items():
            status = "✅" if enabled else "❌"
            feature_name = feature.replace('_', ' ').title()
            summary += f"- {status} {feature_name}\n"
        
        readme_file = output_path / "README.md"
        with open(readme_file, 'w') as f:
            f.write(summary)

def main():
    parser = argparse.ArgumentParser(description='Generate QaliTrack deployment configurations')
    parser.add_argument('config', help='Path to client configuration file')
    parser.add_argument('--output', '-o', default='deployments', 
                       help='Output directory (default: deployments)')
    
    args = parser.parse_args()
    
    if not os.path.exists(args.config):
        print(f"Error: Configuration file '{args.config}' not found")
        sys.exit(1)
    
    generator = DeploymentGenerator(args.config, args.output)
    generator.generate_all_files()

if __name__ == '__main__':
    main()