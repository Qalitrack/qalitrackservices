#!/usr/bin/env python3
"""
Service Removal Script

This script removes a microservice and its associated make targets.
Usage: python remove-service.py <service-type> <service-name>

Examples:
  python remove-service.py masterdata inventory-service
  python remove-service.py datamanager analytics-service
"""

import os
import shutil
import sys
import re
from pathlib import Path

def to_kebab_case(text):
    """Convert text to kebab-case"""
    return re.sub(r'[-_\s]+', '-', text.lower())

def to_pascal_case(text):
    """Convert text to PascalCase"""
    return ''.join(word.capitalize() for word in re.split(r'[-_\s]', text))

def get_service_dir(service_type, service_name):
    """Get the service directory path"""
    script_dir = Path(__file__).parent
    repo_root = script_dir.parent
    
    # Determine target directory based on service type
    if service_type == "masterdata":
        target_parent = repo_root / "packages" / "microservices" / "masterdata"
    elif service_type == "datamanager":
        target_parent = repo_root / "packages" / "microservices" / "datamanager"
    else:
        raise ValueError(f"Invalid service type: {service_type}. Must be 'masterdata' or 'datamanager'")
    
    return target_parent / service_name

def remove_make_targets(service_kebab, service_pascal):
    """Remove make targets for the service from the Makefile"""
    script_dir = Path(__file__).parent
    repo_root = script_dir.parent
    makefile_path = repo_root / "Makefile"
    
    if not makefile_path.exists():
        print(f"Warning: Makefile not found at {makefile_path}")
        return
    
    try:
        with open(makefile_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Find and remove the service-specific targets
        lines = content.split('\n')
        filtered_lines = []
        skip_section = False
        
        for line in lines:
            # Check if this is the start of our service section
            if line.strip() == f"# {service_pascal} Service Targets (Auto-generated)":
                skip_section = True
                continue
            
            # Check if this is a target for our service
            if skip_section:
                # Skip until we find the next section or end of file
                if line.strip() == "" and not line.startswith('\t'):
                    skip_section = False
                    continue
                elif line.startswith(f'build-{service_kebab}:') or \
                     line.startswith(f'run-{service_kebab}:') or \
                     line.startswith(f'test-{service_kebab}:') or \
                     line.startswith(f'docker-build-{service_kebab}:') or \
                     line.startswith(f'docker-run-{service_kebab}:') or \
                     line.startswith(f'.PHONY: build-{service_kebab}'):
                    continue
                elif line.startswith('\t') or line.startswith(' '):
                    continue
                else:
                    skip_section = False
            
            if not skip_section:
                filtered_lines.append(line)
        
        # Write back the filtered content
        updated_content = '\n'.join(filtered_lines)
        
        with open(makefile_path, 'w', encoding='utf-8') as f:
            f.write(updated_content)
        
        print(f"✅ Removed make targets for {service_pascal}")
        
    except Exception as e:
        print(f"Warning: Could not remove make targets from Makefile: {e}")

def remove_service(service_type, service_name):
    """Remove a service and its make targets"""
    
    # Validate service type
    if service_type not in ["masterdata", "datamanager"]:
        print(f"Error: Invalid service type '{service_type}'. Must be 'masterdata' or 'datamanager'")
        return False
    
    # Normalize names
    service_kebab = to_kebab_case(service_name)
    service_pascal = to_pascal_case(service_name)
    
    # Get service directory
    try:
        service_dir = get_service_dir(service_type, service_kebab)
    except ValueError as e:
        print(f"Error: {e}")
        return False
    
    if not service_dir.exists():
        print(f"Error: Service directory {service_dir} does not exist!")
        return False
    
    print(f"Removing {service_pascal} service...")
    print(f"Service directory: {service_dir}")
    
    # Confirm deletion
    confirm = input(f"Are you sure you want to remove {service_pascal} service? (y/N): ")
    if confirm.lower() != 'y':
        print("Removal cancelled.")
        return False
    
    # Remove the service directory
    try:
        shutil.rmtree(service_dir)
        print(f"✅ Removed service directory: {service_dir}")
    except Exception as e:
        print(f"Error removing service directory: {e}")
        return False
    
    # Remove make targets
    remove_make_targets(service_kebab, service_pascal)
    
    print(f"\n✅ Service {service_pascal} removed successfully!")
    print(f"📁 Removed: {service_dir}")
    print(f"🏷️  Type: {service_type}")
    print(f"🎯 Make targets removed: build-{service_kebab}, run-{service_kebab}, test-{service_kebab}")
    
    return True

def main():
    """Main entry point"""
    if len(sys.argv) < 3:
        print("Usage: python remove-service.py <service-type> <service-name>")
        print("\nService Types:")
        print("  masterdata   - For master data services")
        print("  datamanager  - For data management services")
        print("\nExamples:")
        print("  python remove-service.py masterdata inventory-service")
        print("  python remove-service.py datamanager analytics-service")
        sys.exit(1)
    
    service_type = sys.argv[1]
    service_name = sys.argv[2]
    
    # Validate inputs
    if service_type not in ["masterdata", "datamanager"]:
        print("Error: Service type must be 'masterdata' or 'datamanager'")
        sys.exit(1)
    
    if not re.match(r'^[a-zA-Z][a-zA-Z0-9-_]*$', service_name):
        print("Error: Service name must start with a letter and contain only letters, numbers, hyphens, and underscores")
        sys.exit(1)
    
    success = remove_service(service_type, service_name)
    if not success:
        sys.exit(1)

if __name__ == "__main__":
    main()