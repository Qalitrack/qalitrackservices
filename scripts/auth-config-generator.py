#!/usr/bin/env python3
"""
QaliTrack Authorization Configuration Generator

Generates Ocelot gateway configurations with role-based authorization metadata
from YAML authorization rule definitions.

Usage:
    python scripts/auth-config-generator.py --rules=<auth-rules.yml> [options]
    
Examples:
    # Generate gateway ocelot.json from auth rules
    python scripts/auth-config-generator.py --rules=configs/auth/auth-rules-template.yml
    
    # Generate for specific environment
    python scripts/auth-config-generator.py --rules=auth-rules.yml --env=production
    
    # Apply directly to gateway
    python scripts/auth-config-generator.py --rules=auth-rules.yml --apply-to-gateway
    
    # Validate existing gateway config
    python scripts/auth-config-generator.py --validate-gateway
"""

import argparse
import json
import yaml
import os
import shutil
from typing import Dict, List, Any, Optional
from datetime import datetime, timezone
import sys

class AuthConfigGenerator:
    """Generates gateway authorization configurations from YAML rules."""
    
    def __init__(self):
        self.auth_rules_dir = "configs/auth"
        self.gateway_config_path = "packages/qalitrack-gateway/src/ocelot.json"
        self.gateway_backup_dir = "packages/qalitrack-gateway/src/backup"
        
        # Ensure directories exist
        os.makedirs(self.auth_rules_dir, exist_ok=True)
        os.makedirs(self.gateway_backup_dir, exist_ok=True)
    
    def load_auth_rules(self, rules_file: str) -> Dict[str, Any]:
        """Load authorization rules from YAML file."""
        if not os.path.exists(rules_file):
            raise FileNotFoundError(f"Authorization rules file not found: {rules_file}")
        
        with open(rules_file, 'r') as f:
            return yaml.safe_load(f)
    
    def generate_ocelot_routes(self, auth_rules: Dict[str, Any], environment: str = "development") -> List[Dict[str, Any]]:
        """Generate Ocelot route configurations with authorization metadata."""
        routes = []
        
        services = auth_rules.get('services', {})
        authorization_rules = auth_rules.get('authorization_rules', {})
        public_endpoints = auth_rules.get('public_endpoints', [])
        
        # Generate routes for each service
        for service_name, service_config in authorization_rules.items():
            if service_name not in services:
                print(f"⚠️  Warning: Service '{service_name}' not found in services mapping")
                continue
            
            port = services[service_name]
            base_path = service_config.get('base_path', f'/api/{service_name.replace("-service", "")}')
            service_display_name = service_config.get('service_name', service_name.title())
            service_description = service_config.get('description', f'{service_display_name} operations')
            
            # Generate routes for each authorization rule
            for rule in service_config.get('rules', []):
                route = self._create_ocelot_route(
                    upstream_path=rule['path'],
                    downstream_port=port,
                    service_name=service_display_name,
                    required_roles=rule['roles'],
                    description=rule.get('description', 'Protected endpoint'),
                    public_endpoints=public_endpoints
                )
                routes.append(route)
        
        # Add public auth routes (no authorization required)
        auth_route = self._create_public_route(
            upstream_path="/api/auth/{everything}",
            downstream_port=services.get('user-service', 7001),
            description="Authentication endpoints - public access"
        )
        routes.append(auth_route)
        
        # Add health check routes
        for service_name, port in services.items():
            health_route = self._create_public_route(
                upstream_path=f"/services/{service_name.replace('-service', '')}/health",
                downstream_path="/health",
                downstream_port=port,
                description=f"{service_name} health check",
                methods=["GET"]
            )
            routes.append(health_route)
        
        # Apply environment-specific overrides
        routes = self._apply_environment_overrides(routes, auth_rules, environment)
        
        return routes
    
    def _create_ocelot_route(self, upstream_path: str, downstream_port: int, 
                           service_name: str, required_roles: List[str], 
                           description: str, public_endpoints: List[str]) -> Dict[str, Any]:
        """Create an Ocelot route configuration with authorization metadata."""
        
        # Check if this is a public endpoint
        is_public = any(self._path_matches_pattern(upstream_path, pattern) 
                       for pattern in public_endpoints)
        
        route = {
            "DownstreamPathTemplate": "/api/{everything}",
            "DownstreamScheme": "http",  # Use HTTP for internal communication
            "DownstreamHostAndPorts": [
                {
                    "Host": "localhost",
                    "Port": downstream_port
                }
            ],
            "UpstreamPathTemplate": upstream_path,
            "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE", "PATCH"],
            "Key": f"{service_name.lower()}-{hash(upstream_path) % 10000}",
            "Metadata": {
                "ServiceName": service_name,
                "Description": description,
                "RequiredRoles": required_roles,
                "IsPublic": is_public,
                "GeneratedAt": datetime.now(timezone.utc).isoformat()
            }
        }
        
        # Add authentication options if not public
        if not is_public:
            route["AuthenticationOptions"] = {
                "AuthenticationProviderKey": "Bearer"
            }
            route["RouteClaimsRequirement"] = {}
        
        return route
    
    def _create_public_route(self, upstream_path: str, downstream_port: int,
                           description: str, downstream_path: str = None,
                           methods: List[str] = None) -> Dict[str, Any]:
        """Create a public route (no authentication required)."""
        
        if methods is None:
            methods = ["GET", "POST", "PUT", "DELETE", "PATCH"]
        
        if downstream_path is None:
            downstream_path = "/api/{everything}" if "{everything}" in upstream_path else upstream_path
        
        return {
            "DownstreamPathTemplate": downstream_path,
            "DownstreamScheme": "http",
            "DownstreamHostAndPorts": [
                {
                    "Host": "localhost",
                    "Port": downstream_port
                }
            ],
            "UpstreamPathTemplate": upstream_path,
            "UpstreamHttpMethod": methods,
            "Key": f"public-{hash(upstream_path) % 10000}",
            "Metadata": {
                "Description": description,
                "IsPublic": True,
                "RequiredRoles": [],
                "GeneratedAt": datetime.now(timezone.utc).isoformat()
            }
        }
    
    def _path_matches_pattern(self, path: str, pattern: str) -> bool:
        """Check if a path matches a pattern (supports wildcards)."""
        if pattern.endswith('*'):
            return path.startswith(pattern[:-1])
        return path == pattern
    
    def _apply_environment_overrides(self, routes: List[Dict[str, Any]], 
                                   auth_rules: Dict[str, Any], 
                                   environment: str) -> List[Dict[str, Any]]:
        """Apply environment-specific overrides to routes."""
        
        env_config = auth_rules.get('environments', {}).get(environment, {})
        overrides = env_config.get('overrides', [])
        additional_rules = env_config.get('additional_rules', [])
        
        # Apply overrides
        for override in overrides:
            override_path = override['path']
            new_roles = override['roles']
            
            for route in routes:
                if self._path_matches_pattern(route['UpstreamPathTemplate'], override_path):
                    route['Metadata']['RequiredRoles'] = new_roles
                    route['Metadata']['EnvironmentOverride'] = environment
        
        # Add additional rules for environment
        for rule in additional_rules:
            # Find a service port for the additional rule (use user-service as default)
            port = 7001  # Default to user service
            
            additional_route = self._create_ocelot_route(
                upstream_path=rule['path'],
                downstream_port=port,
                service_name="SystemService",
                required_roles=rule['roles'],
                description=rule.get('description', 'Environment-specific endpoint'),
                public_endpoints=[]
            )
            routes.append(additional_route)
        
        return routes
    
    def generate_gateway_config(self, auth_rules: Dict[str, Any], 
                               environment: str = "development") -> Dict[str, Any]:
        """Generate complete Ocelot gateway configuration."""
        
        routes = self.generate_ocelot_routes(auth_rules, environment)
        
        return {
            "Routes": routes,
            "GlobalConfiguration": {
                "BaseUrl": "http://localhost:7000",
                "RateLimitOptions": {
                    "ClientIdHeader": "X-ClientId",
                    "QuotaExceededMessage": "Rate limit exceeded",
                    "RateLimitCounterPrefix": "ocelot"
                }
            },
            "Metadata": {
                "GeneratedAt": datetime.now(timezone.utc).isoformat(),
                "Environment": environment,
                "Version": "1.0.0",
                "TotalRoutes": len(routes),
                "ProtectedRoutes": len([r for r in routes if not r['Metadata'].get('IsPublic', False)]),
                "PublicRoutes": len([r for r in routes if r['Metadata'].get('IsPublic', False)])
            }
        }
    
    def backup_existing_config(self) -> Optional[str]:
        """Backup existing gateway configuration."""
        if not os.path.exists(self.gateway_config_path):
            return None
        
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        backup_path = os.path.join(self.gateway_backup_dir, f"ocelot_backup_{timestamp}.json")
        
        shutil.copy2(self.gateway_config_path, backup_path)
        return backup_path
    
    def apply_to_gateway(self, gateway_config: Dict[str, Any]) -> str:
        """Apply generated configuration to gateway."""
        
        # Backup existing configuration
        backup_path = self.backup_existing_config()
        if backup_path:
            print(f"📁 Backed up existing config to: {backup_path}")
        
        # Write new configuration
        with open(self.gateway_config_path, 'w') as f:
            json.dump(gateway_config, f, indent=2)
        
        return self.gateway_config_path
    
    def validate_gateway_config(self) -> bool:
        """Validate existing gateway configuration."""
        
        if not os.path.exists(self.gateway_config_path):
            print(f"❌ Gateway config not found: {self.gateway_config_path}")
            return False
        
        try:
            with open(self.gateway_config_path, 'r') as f:
                config = json.load(f)
            
            routes = config.get('Routes', [])
            if not routes:
                print("❌ No routes found in gateway configuration")
                return False
            
            protected_routes = 0
            public_routes = 0
            routes_with_roles = 0
            
            for route in routes:
                metadata = route.get('Metadata', {})
                is_public = metadata.get('IsPublic', False)
                required_roles = metadata.get('RequiredRoles', [])
                
                if is_public:
                    public_routes += 1
                else:
                    protected_routes += 1
                
                if required_roles:
                    routes_with_roles += 1
            
            print(f"✅ Gateway configuration validation passed")
            print(f"📊 Statistics:")
            print(f"   Total routes: {len(routes)}")
            print(f"   Protected routes: {protected_routes}")
            print(f"   Public routes: {public_routes}")
            print(f"   Routes with role requirements: {routes_with_roles}")
            
            return True
            
        except Exception as e:
            print(f"❌ Gateway config validation failed: {str(e)}")
            return False
    
    def list_auth_rules(self) -> None:
        """List available authorization rule files."""
        print("📋 Available authorization rule files:")
        
        if not os.path.exists(self.auth_rules_dir):
            print("❌ No authorization rules directory found")
            return
        
        rule_files = [f for f in os.listdir(self.auth_rules_dir) if f.endswith('.yml')]
        
        if not rule_files:
            print("❌ No authorization rule files found")
            return
        
        for rule_file in sorted(rule_files):
            rule_path = os.path.join(self.auth_rules_dir, rule_file)
            try:
                with open(rule_path, 'r') as f:
                    rules = yaml.safe_load(f)
                    metadata = rules.get('metadata', {})
                    name = metadata.get('name', rule_file)
                    description = metadata.get('description', 'No description')
                    print(f"  • {rule_file} - {name}")
                    print(f"    {description}")
            except Exception as e:
                print(f"  • {rule_file} - Error loading: {str(e)}")

def main():
    parser = argparse.ArgumentParser(
        description="Generate QaliTrack gateway authorization configurations",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  %(prog)s --rules=configs/auth/auth-rules-template.yml
  %(prog)s --rules=configs/auth/custom-rules.yml --env=production
  %(prog)s --rules=auth-rules.yml --apply-to-gateway
  %(prog)s --validate-gateway
  %(prog)s --list-rules
        """
    )
    
    parser.add_argument(
        '--rules', '-r',
        help='Path to authorization rules YAML file'
    )
    
    parser.add_argument(
        '--environment', '--env', '-e',
        choices=['development', 'production', 'staging'],
        default='development',
        help='Target environment (default: development)'
    )
    
    parser.add_argument(
        '--apply-to-gateway', '-a',
        action='store_true',
        help='Apply generated config directly to gateway'
    )
    
    parser.add_argument(
        '--validate-gateway', '-v',
        action='store_true',
        help='Validate existing gateway configuration'
    )
    
    parser.add_argument(
        '--list-rules', '-l',
        action='store_true',
        help='List available authorization rule files'
    )
    
    parser.add_argument(
        '--output', '-o',
        help='Output file path (default: stdout or gateway config if --apply-to-gateway)'
    )
    
    args = parser.parse_args()
    
    generator = AuthConfigGenerator()
    
    # Handle special commands
    if args.list_rules:
        generator.list_auth_rules()
        return
    
    if args.validate_gateway:
        success = generator.validate_gateway_config()
        sys.exit(0 if success else 1)
    
    # Require rules file for generation
    if not args.rules:
        parser.error("--rules is required for config generation")
    
    try:
        print(f"🔐 Generating authorization configuration from: {args.rules}")
        print(f"🌍 Target environment: {args.environment}")
        
        # Load authorization rules
        auth_rules = generator.load_auth_rules(args.rules)
        
        # Generate gateway configuration
        gateway_config = generator.generate_gateway_config(auth_rules, args.environment)
        
        # Output or apply configuration
        if args.apply_to_gateway:
            output_path = generator.apply_to_gateway(gateway_config)
            print(f"✅ Applied configuration to gateway: {output_path}")
        elif args.output:
            with open(args.output, 'w') as f:
                json.dump(gateway_config, f, indent=2)
            print(f"✅ Generated configuration saved to: {args.output}")
        else:
            print(json.dumps(gateway_config, indent=2))
        
        # Show summary
        metadata = gateway_config['Metadata']
        print(f"\n📊 Configuration Summary:")
        print(f"   Total Routes: {metadata['TotalRoutes']}")
        print(f"   Protected Routes: {metadata['ProtectedRoutes']}")
        print(f"   Public Routes: {metadata['PublicRoutes']}")
        print(f"   Environment: {metadata['Environment']}")
        
    except Exception as e:
        print(f"❌ Error: {str(e)}")
        sys.exit(1)

if __name__ == "__main__":
    main()