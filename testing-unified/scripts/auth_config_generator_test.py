#!/usr/bin/env python3
"""
Test suite for the QaliTrack Authorization Configuration Generator

Tests the auth-config-generator.py script functionality including:
- YAML rule loading and validation
- Ocelot route generation
- Gateway configuration generation
- Environment-specific overrides
- Configuration validation

Usage:
    python -m pytest testing-unified/scripts/auth_config_generator_test.py -v
"""

import pytest
import json
import yaml
import tempfile
import os
import sys
from unittest.mock import patch, mock_open
from datetime import datetime, timezone

# Add the scripts directory to the Python path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'scripts'))

from scripts.auth_config_generator import AuthConfigGenerator


class TestAuthConfigGenerator:
    """Test suite for AuthConfigGenerator class."""
    
    @pytest.fixture
    def generator(self):
        """Create a test instance of AuthConfigGenerator."""
        return AuthConfigGenerator()
    
    @pytest.fixture
    def sample_auth_rules(self):
        """Sample authorization rules for testing."""
        return {
            "metadata": {
                "name": "Test Authorization Rules",
                "version": "1.0.0",
                "description": "Test configuration"
            },
            "services": {
                "user-service": 7001,
                "vehicle-service": 7003
            },
            "roles": {
                "hierarchy": {
                    "SuperAdmin": 5,
                    "Admin": 4,
                    "SiteManager": 3,
                    "Operator": 2,
                    "User": 1
                }
            },
            "public_endpoints": [
                "/api/auth/*",
                "/health"
            ],
            "authorization_rules": {
                "user-service": {
                    "base_path": "/api/users",
                    "service_name": "UserService",
                    "description": "User management service",
                    "rules": [
                        {
                            "path": "/api/users/{everything}",
                            "roles": ["User"],
                            "description": "Basic user operations"
                        },
                        {
                            "path": "/api/users/admin/{everything}",
                            "roles": ["Admin"],
                            "description": "Admin operations"
                        }
                    ]
                },
                "vehicle-service": {
                    "base_path": "/api/vehicles",
                    "service_name": "VehicleService",
                    "description": "Vehicle management service",
                    "rules": [
                        {
                            "path": "/api/vehicles/{everything}",
                            "roles": ["Operator"],
                            "description": "Vehicle operations"
                        }
                    ]
                }
            },
            "environments": {
                "development": {
                    "overrides": [
                        {
                            "path": "/api/*/admin/*",
                            "roles": ["SiteManager"]
                        }
                    ]
                },
                "production": {
                    "additional_rules": [
                        {
                            "path": "/api/system/*",
                            "roles": ["SuperAdmin"],
                            "description": "System administration"
                        }
                    ]
                }
            }
        }
    
    def test_load_auth_rules_success(self, generator, sample_auth_rules):
        """Test successful loading of authorization rules from YAML file."""
        with tempfile.NamedTemporaryFile(mode='w', suffix='.yml', delete=False) as f:
            yaml.dump(sample_auth_rules, f)
            temp_file = f.name
        
        try:
            loaded_rules = generator.load_auth_rules(temp_file)
            assert loaded_rules == sample_auth_rules
        finally:
            os.unlink(temp_file)
    
    def test_load_auth_rules_file_not_found(self, generator):
        """Test FileNotFoundError when rules file doesn't exist."""
        with pytest.raises(FileNotFoundError):
            generator.load_auth_rules("nonexistent_file.yml")
    
    def test_generate_ocelot_routes(self, generator, sample_auth_rules):
        """Test generation of Ocelot routes from authorization rules."""
        routes = generator.generate_ocelot_routes(sample_auth_rules)
        
        # Should generate routes for each service + auth route + health routes
        assert len(routes) >= 4  # 2 user-service routes + 1 vehicle-service route + auth + health routes
        
        # Check user service routes
        user_routes = [r for r in routes if r.get('Metadata', {}).get('ServiceName') == 'UserService']
        assert len(user_routes) == 2
        
        # Check vehicle service routes
        vehicle_routes = [r for r in routes if r.get('Metadata', {}).get('ServiceName') == 'VehicleService']
        assert len(vehicle_routes) == 1
        
        # Check auth route (public)
        auth_routes = [r for r in routes if r['UpstreamPathTemplate'] == '/api/auth/{everything}']
        assert len(auth_routes) == 1
        assert auth_routes[0]['Metadata']['IsPublic'] is True
    
    def test_create_ocelot_route_protected(self, generator):
        """Test creation of protected Ocelot route."""
        route = generator._create_ocelot_route(
            upstream_path="/api/test/{everything}",
            downstream_port=7001,
            service_name="TestService",
            required_roles=["Admin"],
            description="Test route",
            public_endpoints=[]
        )
        
        assert route['UpstreamPathTemplate'] == "/api/test/{everything}"
        assert route['DownstreamHostAndPorts'][0]['Port'] == 7001
        assert route['Metadata']['ServiceName'] == "TestService"
        assert route['Metadata']['RequiredRoles'] == ["Admin"]
        assert route['Metadata']['IsPublic'] is False
        assert 'AuthenticationOptions' in route
    
    def test_create_ocelot_route_public(self, generator):
        """Test creation of public Ocelot route."""
        route = generator._create_ocelot_route(
            upstream_path="/api/auth/login",
            downstream_port=7001,
            service_name="AuthService",
            required_roles=["User"],
            description="Login endpoint",
            public_endpoints=["/api/auth/*"]
        )
        
        assert route['Metadata']['IsPublic'] is True
        assert 'AuthenticationOptions' not in route
    
    def test_create_public_route(self, generator):
        """Test creation of public route."""
        route = generator._create_public_route(
            upstream_path="/health",
            downstream_port=7001,
            description="Health check",
            methods=["GET"]
        )
        
        assert route['UpstreamPathTemplate'] == "/health"
        assert route['UpstreamHttpMethod'] == ["GET"]
        assert route['Metadata']['IsPublic'] is True
        assert route['Metadata']['RequiredRoles'] == []
    
    def test_path_matches_pattern(self, generator):
        """Test path pattern matching."""
        # Exact match
        assert generator._path_matches_pattern("/api/users", "/api/users") is True
        assert generator._path_matches_pattern("/api/users", "/api/vehicles") is False
        
        # Wildcard match
        assert generator._path_matches_pattern("/api/users/123", "/api/users/*") is True
        assert generator._path_matches_pattern("/api/users/123/profile", "/api/users/*") is True
        assert generator._path_matches_pattern("/api/vehicles/456", "/api/users/*") is False
    
    def test_apply_environment_overrides_development(self, generator, sample_auth_rules):
        """Test application of development environment overrides."""
        routes = [
            {
                'UpstreamPathTemplate': '/api/users/admin/{everything}',
                'Metadata': {'RequiredRoles': ['Admin']}
            }
        ]
        
        modified_routes = generator._apply_environment_overrides(
            routes, sample_auth_rules, "development"
        )
        
        # Should override admin routes to require SiteManager in development
        admin_route = modified_routes[0]
        assert admin_route['Metadata']['RequiredRoles'] == ['SiteManager']
        assert admin_route['Metadata']['EnvironmentOverride'] == 'development'
    
    def test_apply_environment_overrides_production(self, generator, sample_auth_rules):
        """Test application of production environment additional rules."""
        routes = []
        
        modified_routes = generator._apply_environment_overrides(
            routes, sample_auth_rules, "production"
        )
        
        # Should add system admin route in production
        system_routes = [r for r in modified_routes if '/api/system/' in r['UpstreamPathTemplate']]
        assert len(system_routes) == 1
        assert system_routes[0]['Metadata']['RequiredRoles'] == ['SuperAdmin']
    
    def test_generate_gateway_config(self, generator, sample_auth_rules):
        """Test generation of complete gateway configuration."""
        config = generator.generate_gateway_config(sample_auth_rules, "development")
        
        # Check structure
        assert 'Routes' in config
        assert 'GlobalConfiguration' in config
        assert 'Metadata' in config
        
        # Check metadata
        metadata = config['Metadata']
        assert metadata['Environment'] == "development"
        assert metadata['TotalRoutes'] > 0
        assert 'GeneratedAt' in metadata
        
        # Check global configuration
        global_config = config['GlobalConfiguration']
        assert global_config['BaseUrl'] == "http://localhost:7000"
        assert 'RateLimitOptions' in global_config
    
    def test_backup_existing_config(self, generator):
        """Test backup of existing gateway configuration."""
        # Create a temporary gateway config file
        with tempfile.NamedTemporaryFile(mode='w', suffix='.json', delete=False) as f:
            json.dump({"test": "config"}, f)
            temp_config = f.name
        
        # Mock the gateway config path
        generator.gateway_config_path = temp_config
        
        try:
            backup_path = generator.backup_existing_config()
            assert backup_path is not None
            assert os.path.exists(backup_path)
            
            # Verify backup content
            with open(backup_path, 'r') as f:
                backup_content = json.load(f)
            assert backup_content == {"test": "config"}
            
        finally:
            os.unlink(temp_config)
            if backup_path and os.path.exists(backup_path):
                os.unlink(backup_path)
    
    def test_backup_existing_config_no_file(self, generator):
        """Test backup when no existing config file exists."""
        generator.gateway_config_path = "/nonexistent/path/ocelot.json"
        backup_path = generator.backup_existing_config()
        assert backup_path is None
    
    def test_apply_to_gateway(self, generator):
        """Test applying configuration to gateway."""
        test_config = {
            "Routes": [],
            "GlobalConfiguration": {"BaseUrl": "http://localhost:7000"}
        }
        
        with tempfile.NamedTemporaryFile(mode='w', suffix='.json', delete=False) as f:
            temp_config = f.name
        
        generator.gateway_config_path = temp_config
        
        try:
            result_path = generator.apply_to_gateway(test_config)
            assert result_path == temp_config
            assert os.path.exists(temp_config)
            
            # Verify content
            with open(temp_config, 'r') as f:
                saved_config = json.load(f)
            assert saved_config == test_config
            
        finally:
            if os.path.exists(temp_config):
                os.unlink(temp_config)
    
    def test_validate_gateway_config_success(self, generator):
        """Test successful validation of gateway configuration."""
        valid_config = {
            "Routes": [
                {
                    "UpstreamPathTemplate": "/api/test/{everything}",
                    "Metadata": {
                        "IsPublic": False,
                        "RequiredRoles": ["User"]
                    }
                }
            ]
        }
        
        with tempfile.NamedTemporaryFile(mode='w', suffix='.json', delete=False) as f:
            json.dump(valid_config, f)
            temp_config = f.name
        
        generator.gateway_config_path = temp_config
        
        try:
            result = generator.validate_gateway_config()
            assert result is True
        finally:
            os.unlink(temp_config)
    
    def test_validate_gateway_config_no_file(self, generator):
        """Test validation when gateway config file doesn't exist."""
        generator.gateway_config_path = "/nonexistent/path/ocelot.json"
        result = generator.validate_gateway_config()
        assert result is False
    
    def test_validate_gateway_config_no_routes(self, generator):
        """Test validation when gateway config has no routes."""
        invalid_config = {"Routes": []}
        
        with tempfile.NamedTemporaryFile(mode='w', suffix='.json', delete=False) as f:
            json.dump(invalid_config, f)
            temp_config = f.name
        
        generator.gateway_config_path = temp_config
        
        try:
            result = generator.validate_gateway_config()
            assert result is False
        finally:
            os.unlink(temp_config)
    
    def test_list_auth_rules(self, generator):
        """Test listing available authorization rule files."""
        # Create temporary auth rules directory with test files
        with tempfile.TemporaryDirectory() as temp_dir:
            generator.auth_rules_dir = temp_dir
            
            # Create test YAML files
            test_rules = {
                "metadata": {
                    "name": "Test Rules",
                    "description": "Test authorization rules"
                }
            }
            
            rule_file = os.path.join(temp_dir, "test-rules.yml")
            with open(rule_file, 'w') as f:
                yaml.dump(test_rules, f)
            
            # Test listing (this will print to stdout, so we can't easily assert the output)
            # But we can verify it doesn't raise an exception
            generator.list_auth_rules()


class TestIntegration:
    """Integration tests for the complete auth config generator workflow."""
    
    def test_end_to_end_workflow(self):
        """Test complete workflow from YAML rules to gateway config."""
        # Create sample auth rules
        auth_rules = {
            "metadata": {
                "name": "Integration Test Rules",
                "version": "1.0.0"
            },
            "services": {
                "user-service": 7001
            },
            "roles": {
                "hierarchy": {
                    "Admin": 4,
                    "User": 1
                }
            },
            "public_endpoints": ["/api/auth/*"],
            "authorization_rules": {
                "user-service": {
                    "rules": [
                        {
                            "path": "/api/users/{everything}",
                            "roles": ["User"],
                            "description": "User operations"
                        }
                    ]
                }
            }
        }
        
        # Create temporary files
        with tempfile.NamedTemporaryFile(mode='w', suffix='.yml', delete=False) as rules_file, \
             tempfile.NamedTemporaryFile(mode='w', suffix='.json', delete=False) as config_file:
            
            yaml.dump(auth_rules, rules_file)
            rules_path = rules_file.name
            config_path = config_file.name
        
        try:
            generator = AuthConfigGenerator()
            generator.gateway_config_path = config_path
            
            # Load rules
            loaded_rules = generator.load_auth_rules(rules_path)
            
            # Generate gateway config
            gateway_config = generator.generate_gateway_config(loaded_rules)
            
            # Apply to gateway
            result_path = generator.apply_to_gateway(gateway_config)
            
            # Validate result
            assert result_path == config_path
            validation_result = generator.validate_gateway_config()
            assert validation_result is True
            
            # Verify final config structure
            with open(config_path, 'r') as f:
                final_config = json.load(f)
            
            assert 'Routes' in final_config
            assert 'GlobalConfiguration' in final_config
            assert 'Metadata' in final_config
            assert len(final_config['Routes']) > 0
            
        finally:
            for path in [rules_path, config_path]:
                if os.path.exists(path):
                    os.unlink(path)


if __name__ == '__main__':
    # Run tests if executed directly
    pytest.main([__file__, '-v'])