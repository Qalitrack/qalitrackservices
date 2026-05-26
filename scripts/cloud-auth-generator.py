#!/usr/bin/env python3
"""
QaliTrack Authorization Configuration Generator

This tool generates authorization configurations for the QaliTrack API Gateway,
converting YAML authorization rules into Ocelot-compatible JSON configurations
that the gateway uses for role-based access control enforcement.

Usage:
    python scripts/cloud-auth-generator.py --rules=<auth-rules.yml> [options]
    
Examples:
    # Generate gateway config from auth rules
    python scripts/cloud-auth-generator.py --rules=configs/auth/auth-rules-template.yml
    
    # Generate for specific client deployment
    python scripts/cloud-auth-generator.py --client=babumri-cement --rules=configs/auth/custom-rules.yml
    
    # Apply to gateway directly
    python scripts/cloud-auth-generator.py --rules=auth-rules.yml --apply-to-gateway
"""

import argparse
import json
import yaml
import os
from typing import Dict, List, Any
from datetime import datetime, timezone
import sys

class QaliTrackAuthGenerator:
    """Generates authorization configurations for QaliTrack client deployments."""
    
    # Default role hierarchy (can be customized per client)
    DEFAULT_ROLE_HIERARCHY = {
        "SuperAdmin": 5,
        "Admin": 4,
        "SiteManager": 3,
        "Operator": 2,
        "Auditor": 2,
        "ClientAdmin": 2,
        "User": 1
    }
    
    # Service endpoint role requirements
    SERVICE_ROLE_REQUIREMENTS = {
        "user-service": {
            "/api/users/profile": ["User"],
            "/api/users": ["User"],
            "/api/users/admin": ["Admin"],
            "/api/users/roles": ["Admin"],
            "/api/users/permissions": ["Admin"]
        },
        "organization-service": {
            "/api/organizations": ["Admin"],
            "/api/organizations/admin": ["Admin"],
            "/api/organizations/sites": ["SiteManager"]
        },
        "vehicle-service": {
            "/api/vehicles": ["Operator"],
            "/api/vehicles/admin": ["Admin"],
            "/api/vehicles/assignments": ["SiteManager"]
        },
        "driver-service": {
            "/api/drivers": ["Operator"],
            "/api/drivers/admin": ["Admin"],
            "/api/drivers/licenses": ["SiteManager"]
        },
        "product-service": {
            "/api/products": ["Operator"],
            "/api/products/admin": ["Admin"],
            "/api/products/catalog": ["User"]
        },
        "customer-service": {
            "/api/customers": ["Operator"],
            "/api/customers/admin": ["Admin"],
            "/api/customers/contracts": ["SiteManager"]
        },
        "supplier-service": {
            "/api/suppliers": ["Operator"],
            "/api/suppliers/admin": ["Admin"],
            "/api/suppliers/contracts": ["SiteManager"]
        },
        "transporter-service": {
            "/api/transporters": ["Operator"],
            "/api/transporters/admin": ["Admin"]
        },
        "route-service": {
            "/api/routes": ["Operator"],
            "/api/routes/admin": ["Admin"],
            "/api/routes/optimization": ["SiteManager"]
        },
        "weighbridge-service": {
            "/api/weighbridges": ["Operator"],
            "/api/weighbridges/admin": ["Admin"],
            "/api/weighbridges/calibration": ["SiteManager"]
        },
        "sacco-service": {
            "/api/saccos": ["Operator"],
            "/api/saccos/admin": ["Admin"]
        },
        "weight-data-service": {
            "/api/weight-measurements": ["Operator"],
            "/api/weight-corrections": ["SiteManager"],
            "/api/weight-analytics": ["SiteManager"]
        },
        "compliance-service": {
            "/api/compliance/checks": ["Auditor"],
            "/api/compliance/violations": ["Auditor"],
            "/api/compliance/reports": ["Auditor"],
            "/api/compliance/admin": ["Admin"]
        },
        "operational-data-service": {
            "/api/operational/capacity": ["SiteManager"],
            "/api/operational/schedules": ["Operator"],
            "/api/operational/maintenance": ["SiteManager"]
        },
        "transaction-service": {
            "/api/transactions": ["Operator"],
            "/api/transactions/charges": ["SiteManager"],
            "/api/transactions/reports": ["Auditor"]
        },
        "analytics-service": {
            "/api/analytics/dashboard": ["SiteManager"],
            "/api/analytics/reports": ["SiteManager"],
            "/api/analytics/metrics": ["Auditor"]
        },
        "data-sync-service": {
            "/api/sync/sessions": ["Admin"],
            "/api/sync/conflicts": ["Admin"],
            "/api/sync/sites": ["SiteManager"]
        },
        "archive-service": {
            "/api/archive": ["Admin"],
            "/api/archive/policies": ["Admin"],
            "/api/archive/search": ["Auditor"]
        }
    }
    
    def __init__(self):
        self.config_dir = "configs/clients"
        self.output_dir = "configs/auth"
        os.makedirs(self.output_dir, exist_ok=True)
    
    def load_client_config(self, client_name: str) -> Dict[str, Any]:
        """Load client configuration from YAML file."""
        config_path = os.path.join(self.config_dir, f"{client_name}.yml")
        if not os.path.exists(config_path):
            raise FileNotFoundError(f"Client configuration not found: {config_path}")
        
        with open(config_path, 'r') as f:
            return yaml.safe_load(f)
    
    def get_enabled_services(self, client_config: Dict[str, Any]) -> List[str]:
        """Get list of enabled services for the client."""
        enabled_services = []
        services = client_config.get('services', {})
        
        for service_name, service_config in services.items():
            if service_config.get('enabled', False):
                enabled_services.append(service_name)
        
        return enabled_services
    
    def generate_jwt_config(self, client_config: Dict[str, Any]) -> Dict[str, Any]:
        """Generate JWT configuration for the client."""
        client_name = client_config['client']['code']
        
        return {
            "issuer": f"QaliTrack-{client_name}",
            "audience": f"QaliTrack-{client_name}",
            "secret_key": f"${{{client_name.upper()}_JWT_SECRET}}",
            "expiry_minutes": 15,
            "refresh_expiry_days": 7,
            "algorithm": "HS256"
        }
    
    def generate_role_hierarchy(self, client_config: Dict[str, Any]) -> Dict[str, int]:
        """Generate role hierarchy for the client (can be customized)."""
        # Check if client has custom roles
        custom_roles = client_config.get('authorization', {}).get('roles', {})
        
        if custom_roles:
            return custom_roles
        
        # Use default hierarchy
        return self.DEFAULT_ROLE_HIERARCHY.copy()
    
    def generate_service_permissions(self, enabled_services: List[str]) -> Dict[str, Dict[str, List[str]]]:
        """Generate service-specific permission mappings."""
        permissions = {}
        
        for service in enabled_services:
            if service in self.SERVICE_ROLE_REQUIREMENTS:
                permissions[service] = self.SERVICE_ROLE_REQUIREMENTS[service].copy()
        
        return permissions
    
    def generate_gateway_routes(self, enabled_services: List[str]) -> List[Dict[str, Any]]:
        """Generate Ocelot gateway route configurations with role requirements."""
        routes = []
        port_mapping = {
            "user-service": 7001,
            "organization-service": 7002,
            "vehicle-service": 7003,
            "driver-service": 7004,
            "product-service": 7005,
            "customer-service": 7006,
            "supplier-service": 7007,
            "transporter-service": 7008,
            "route-service": 7009,
            "weighbridge-service": 7010,
            "sacco-service": 7011,
            "weight-data-service": 7012,
            "compliance-service": 7013,
            "operational-data-service": 7014,
            "transaction-service": 7015,
            "analytics-service": 7016,
            "data-sync-service": 7017,
            "archive-service": 7018
        }
        
        for service in enabled_services:
            if service in port_mapping and service in self.SERVICE_ROLE_REQUIREMENTS:
                service_name = service.replace('-', '_').title().replace('_', '')
                
                # Get minimum required role for this service
                service_endpoints = self.SERVICE_ROLE_REQUIREMENTS[service]
                min_roles = set()
                for endpoint_roles in service_endpoints.values():
                    min_roles.update(endpoint_roles)
                
                route = {
                    "UpstreamPathTemplate": f"/api/{service.replace('-service', '')}s/{{everything}}",
                    "DownstreamPathTemplate": "/api/{everything}",
                    "DownstreamHostAndPorts": [
                        {
                            "Host": "localhost",
                            "Port": port_mapping[service]
                        }
                    ],
                    "DownstreamScheme": "http",
                    "Metadata": {
                        "ServiceName": service_name,
                        "RequiredRoles": list(min_roles),
                        "Description": f"Routes to {service_name} microservice"
                    }
                }
                routes.append(route)
        
        return routes
    
    def generate_auth_config(self, client_name: str, output_format: str = "json") -> str:
        """Generate complete authorization configuration for a client."""
        try:
            # Load client configuration
            client_config = self.load_client_config(client_name)
            
            # Get enabled services
            enabled_services = self.get_enabled_services(client_config)
            
            # Generate configuration components
            auth_config = {
                "metadata": {
                    "client": client_config['client']['name'],
                    "client_code": client_config['client']['code'],
                    "generated_at": datetime.now(timezone.utc).isoformat(),
                    "generator_version": "1.0.0"
                },
                "jwt": self.generate_jwt_config(client_config),
                "roles": {
                    "hierarchy": self.generate_role_hierarchy(client_config),
                    "descriptions": {
                        "SuperAdmin": "System-wide administrative access",
                        "Admin": "Full administrative access to organization",
                        "SiteManager": "Management access to assigned sites",
                        "Operator": "Operational access for daily tasks",
                        "Auditor": "Read-only access for compliance and auditing",
                        "ClientAdmin": "Organization-specific administrative access",
                        "User": "Basic user access"
                    }
                },
                "services": {
                    "enabled": enabled_services,
                    "permissions": self.generate_service_permissions(enabled_services)
                },
                "gateway": {
                    "routes": self.generate_gateway_routes(enabled_services),
                    "public_endpoints": [
                        "/api/auth/*",
                        "/health",
                        "/api/swagger",
                        "/swagger"
                    ]
                }
            }
            
            # Generate output filename
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            if output_format.lower() == "yaml":
                filename = f"{client_name}_auth_config_{timestamp}.yml"
                output_path = os.path.join(self.output_dir, filename)
                with open(output_path, 'w') as f:
                    yaml.dump(auth_config, f, default_flow_style=False, indent=2)
            else:
                filename = f"{client_name}_auth_config_{timestamp}.json"
                output_path = os.path.join(self.output_dir, filename)
                with open(output_path, 'w') as f:
                    json.dump(auth_config, f, indent=2)
            
            return output_path
            
        except Exception as e:
            raise RuntimeError(f"Failed to generate auth config for {client_name}: {str(e)}")
    
    def validate_config(self, config_path: str) -> bool:
        """Validate generated authorization configuration."""
        try:
            with open(config_path, 'r') as f:
                if config_path.endswith('.yml') or config_path.endswith('.yaml'):
                    config = yaml.safe_load(f)
                else:
                    config = json.load(f)
            
            # Basic validation checks
            required_sections = ['metadata', 'jwt', 'roles', 'services', 'gateway']
            for section in required_sections:
                if section not in config:
                    print(f"❌ Missing required section: {section}")
                    return False
            
            # Validate role hierarchy
            if not isinstance(config['roles']['hierarchy'], dict):
                print("❌ Role hierarchy must be a dictionary")
                return False
            
            # Validate services
            if not config['services']['enabled']:
                print("❌ No services enabled")
                return False
            
            print("✅ Configuration validation passed")
            return True
            
        except Exception as e:
            print(f"❌ Validation failed: {str(e)}")
            return False

def main():
    parser = argparse.ArgumentParser(
        description="Generate QaliTrack authorization configurations for client deployments",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  %(prog)s --client=babumri-cement
  %(prog)s --client=testing --format=yaml
  %(prog)s --client=national-weighing --format=json --validate
        """
    )
    
    parser.add_argument(
        '--client', '-c',
        help='Client name (must match YAML file in configs/clients/)'
    )
    
    parser.add_argument(
        '--format', '-f',
        choices=['json', 'yaml'],
        default='json',
        help='Output format (default: json)'
    )
    
    parser.add_argument(
        '--validate', '-v',
        action='store_true',
        help='Validate generated configuration'
    )
    
    parser.add_argument(
        '--list-clients', '-l',
        action='store_true',
        help='List available client configurations'
    )
    
    args = parser.parse_args()
    
    generator = QaliTrackAuthGenerator()
    
    # List available clients
    if args.list_clients:
        print("📋 Available client configurations:")
        try:
            config_files = [f for f in os.listdir(generator.config_dir) if f.endswith('.yml')]
            for config_file in sorted(config_files):
                client_name = config_file.replace('.yml', '')
                print(f"  • {client_name}")
        except FileNotFoundError:
            print("❌ No client configurations found")
        return
    
    # Validate arguments
    if not args.client and not args.list_clients:
        parser.error("--client is required unless using --list-clients")
    
    # Generate authorization configuration
    try:
        print(f"🔐 Generating authorization configuration for: {args.client}")
        print(f"📄 Output format: {args.format}")
        
        output_path = generator.generate_auth_config(args.client, args.format)
        
        print(f"✅ Authorization configuration generated successfully!")
        print(f"📁 Output file: {output_path}")
        
        # Validate if requested
        if args.validate:
            print("\n🔍 Validating configuration...")
            generator.validate_config(output_path)
        
        # Show summary
        with open(output_path, 'r') as f:
            if args.format == 'yaml':
                config = yaml.safe_load(f)
            else:
                config = json.load(f)
        
        print(f"\n📊 Configuration Summary:")
        print(f"   Client: {config['metadata']['client']}")
        print(f"   Services: {len(config['services']['enabled'])} enabled")
        print(f"   Roles: {len(config['roles']['hierarchy'])} defined")
        print(f"   Gateway Routes: {len(config['gateway']['routes'])} configured")
        
    except Exception as e:
        print(f"❌ Error: {str(e)}")
        sys.exit(1)

if __name__ == "__main__":
    main()