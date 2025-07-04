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
        
        service_def = {
            'build': {
                'context': self._get_service_context(service_name),
                'dockerfile': 'Dockerfile'
            },
            'ports': [f"{service_config['port']}:80"],
            'environment': self._get_service_environment(service_name, environment),
            'networks': [f'qalitrack-{client_code}'],
            'restart': 'unless-stopped'
        }
        
        # Add health check
        service_def['healthcheck'] = {
            'test': ['CMD', 'curl', '-f', 'http://localhost:80/health'],
            'interval': '30s',
            'timeout': '10s',
            'retries': 3,
            'start_period': '40s'
        }
        
        # Add volume mounts if needed
        if self._service_needs_volume(service_name):
            volume_name = f"{service_name.replace('-', '_')}_data"
            service_def['volumes'] = [
                f"{volume_name}:/data",
                f"./{service_name}/logs:/app/logs"
            ]
        
        # Add dependencies
        dependencies = self._get_service_dependencies(service_name)
        if dependencies:
            service_def['depends_on'] = {}
            for dep in dependencies:
                if self.config['services'].get(dep, {}).get('enabled', False):
                    service_def['depends_on'][dep] = {'condition': 'service_healthy'}
        
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
        elif service_name in ['organization-service', 'user-service', 'vehicle-service', 
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