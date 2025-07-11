#!/usr/bin/env python3
"""
Service Generator Script

This script generates a new microservice from the service template.
Usage: python generate-service.py <service-type> <service-name> <entity-name> [description]

Service Types:
  - masterdata: For master data services (user, customer, product, etc.)
  - datamanager: For data management services (analytics, transaction, etc.)

Examples:
  python generate-service.py masterdata inventory-service inventory "Inventory Management Service"
  python generate-service.py datamanager analytics-service analytics "Analytics Processing Service"
"""

import os
import shutil
import sys
import re
from pathlib import Path

def to_pascal_case(text):
    """Convert text to PascalCase"""
    return ''.join(word.capitalize() for word in re.split(r'[-_\s]', text))

def to_camel_case(text):
    """Convert text to camelCase"""
    pascal = to_pascal_case(text)
    return pascal[0].lower() + pascal[1:] if pascal else ''

def to_kebab_case(text):
    """Convert text to kebab-case"""
    return re.sub(r'[-_\s]+', '-', text.lower())

def replace_placeholders(content, replacements):
    """Replace template placeholders with actual values"""
    for placeholder, value in replacements.items():
        content = content.replace(placeholder, value)
    return content

def get_template_dir():
    """Get the template directory path"""
    script_dir = Path(__file__).parent
    repo_root = script_dir.parent
    template_dir = repo_root / "packages" / "microservices" / "masterdata" / "service-template"
    
    if not template_dir.exists():
        raise FileNotFoundError(f"Template directory not found at {template_dir}")
    
    return template_dir

def get_target_dir(service_type, service_name):
    """Get the target directory for the new service"""
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

def generate_service(service_type, service_name, entity_name, description=""):
    """Generate a new service from the template"""
    
    # Validate service type
    if service_type not in ["masterdata", "datamanager"]:
        print(f"Error: Invalid service type '{service_type}'. Must be 'masterdata' or 'datamanager'")
        return False
    
    # Normalize names
    service_kebab = to_kebab_case(service_name)
    service_pascal = to_pascal_case(service_name)
    entity_pascal = to_pascal_case(entity_name)
    entity_camel = to_camel_case(entity_name)
    
    # Set up paths
    try:
        template_dir = get_template_dir()
        target_dir = get_target_dir(service_type, service_kebab)
    except (FileNotFoundError, ValueError) as e:
        print(f"Error: {e}")
        return False
    
    if target_dir.exists():
        print(f"Error: Service directory {target_dir} already exists!")
        return False
    
    # Define replacements
    replacements = {
        '{{ServiceName}}': service_pascal,
        '{{service-name}}': service_kebab,
        '{{EntityName}}': entity_pascal,
        '{{entityName}}': entity_camel,
        '{{ServiceDescription}}': description or f"{service_pascal} Management",
        '{{api-prefix}}': entity_camel.lower() + 's'
    }
    
    print(f"Generating {service_pascal} in {service_type} from template...")
    print(f"Entity: {entity_pascal}")
    print(f"Target directory: {target_dir}")
    
    # Create target parent directory if it doesn't exist
    target_dir.parent.mkdir(parents=True, exist_ok=True)
    
    # Copy template directory
    shutil.copytree(template_dir, target_dir)
    
    # Remove template-specific files from the copy
    files_to_remove = [
        "generate-service.py",
        "README-TEMPLATE.md"
    ]
    
    for file_to_remove in files_to_remove:
        file_path = target_dir / file_to_remove
        if file_path.exists():
            file_path.unlink()
    
    # Remove duplicate test files in the Disabled directory
    disabled_dir = target_dir / "tests" / "{{ServiceName}}.Tests" / "Disabled"
    if disabled_dir.exists():
        shutil.rmtree(disabled_dir)
    
    # Rename directories recursively
    def rename_directories(directory):
        """Recursively rename directories with template placeholders"""
        if not directory.exists():
            return
            
        for root, dirs, files in os.walk(directory, topdown=False):
            root_path = Path(root)
            for dir_name in dirs:
                old_dir = root_path / dir_name
                new_dir_name = dir_name
                
                # Replace all template placeholders
                for placeholder, value in replacements.items():
                    new_dir_name = new_dir_name.replace(placeholder, value)
                
                if new_dir_name != dir_name:
                    new_dir = root_path / new_dir_name
                    try:
                        old_dir.rename(new_dir)
                    except OSError as e:
                        print(f"Warning: Could not rename {old_dir} to {new_dir}: {e}")
    
    # Rename directories in src and tests
    rename_directories(target_dir / "src")
    rename_directories(target_dir / "tests")
    
    # Process all files
    for root, dirs, files in os.walk(target_dir):
        for file in files:
            file_path = Path(root) / file
            
            # Skip binary files and specific file types
            if file_path.suffix in ['.dll', '.exe', '.bin', '.db', '.sqlite', '.pdb']:
                continue
                
            # Rename files with template placeholders
            new_file_name = file
            for placeholder, value in replacements.items():
                new_file_name = new_file_name.replace(placeholder, value)
            
            if new_file_name != file:
                new_file_path = file_path.parent / new_file_name
                file_path.rename(new_file_path)
                file_path = new_file_path
            
            # Make shell scripts executable
            if file_path.suffix in ['.sh'] or file_path.name in ['run.sh', 'run.cmd']:
                try:
                    import stat
                    file_path.chmod(file_path.stat().st_mode | stat.S_IEXEC)
                except:
                    pass
            
            # Process file content
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                
                # Replace placeholders
                new_content = replace_placeholders(content, replacements)
                
                if new_content != content:
                    with open(file_path, 'w', encoding='utf-8') as f:
                        f.write(new_content)
                        
            except (UnicodeDecodeError, PermissionError):
                # Skip binary files or files we can't read
                continue
    
    # Update solution and project files
    update_project_files(target_dir, service_pascal, service_kebab)
    
    # Create service-specific README
    create_service_readme(target_dir, service_type, service_pascal, entity_pascal, description)
    
    # Add make targets for the new service
    add_make_targets(service_type, service_kebab, service_pascal)
    
    print(f"\n✅ Service {service_pascal} generated successfully!")
    print(f"📁 Location: {target_dir}")
    print(f"🏷️  Type: {service_type}")
    print(f"\nNext steps:")
    print(f"1. cd packages/microservices/{service_type}/{service_kebab}")
    print(f"2. Review and customize the generated code")
    print(f"3. Update the entities in src/{service_pascal}.Core/Entities/")
    print(f"4. Add domain-specific properties and methods")
    print(f"5. Run: dotnet build")
    print(f"6. Run: dotnet run --project src/{service_pascal}.Api")
    print(f"\nTo test the service:")
    print(f"7. Access Swagger UI at http://localhost:5000")
    print(f"8. Check health endpoint at http://localhost:5000/health")
    print(f"9. Run tests: make test-{service_kebab}")
    
    return True

def update_project_files(target_dir, service_pascal, service_kebab):
    """Update solution and project files with correct names"""
    
    # Find and rename solution files
    for sln_file in target_dir.glob("*.sln"):
        new_sln_name = f"{service_pascal}.sln"
        new_sln_path = target_dir / new_sln_name
        if sln_file.name != new_sln_name:
            sln_file.rename(new_sln_path)
            
        # Update solution file content
        try:
            with open(new_sln_path, 'r', encoding='utf-8') as f:
                content = f.read()
            
            content = content.replace('UserService', service_pascal)
            content = content.replace('UserModule', service_pascal)
            
            with open(new_sln_path, 'w', encoding='utf-8') as f:
                f.write(content)
        except:
            pass
    
    # Update csproj files
    for csproj_file in target_dir.rglob("*.csproj"):
        try:
            with open(csproj_file, 'r', encoding='utf-8') as f:
                content = f.read()
            
            content = content.replace('UserService', service_pascal)
            
            with open(csproj_file, 'w', encoding='utf-8') as f:
                f.write(content)
        except:
            continue

def create_service_readme(target_dir, service_type, service_pascal, entity_pascal, description):
    """Create a service-specific README file"""
    
    readme_content = f"""# {service_pascal}

{description or f"{service_pascal} microservice for the QaliTrack platform."}

## Service Type: {service_type.title()}

This service was generated from the QaliTrack service template and follows clean architecture principles.

## Features

- **Clean Architecture**: API, Core, and Infrastructure layers
- **Entity Framework**: SQLite database with EF Core
- **AutoMapper**: Object-to-object mapping
- **Swagger/OpenAPI**: API documentation
- **Logging**: Structured logging with Serilog
- **Health Checks**: Built-in monitoring

## Main Entity

The primary entity for this service is `{entity_pascal}`.

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQLite (included)

### Running the Service

```bash
# Build the service
dotnet build

# Run the service
dotnet run --project src/{service_pascal}.Api

# Access the API
# Swagger UI: http://localhost:5000
# Health Check: http://localhost:5000/health
```

### Development

1. **Customize Entities**: Update entities in `src/{service_pascal}.Core/Entities/`
2. **Add Business Logic**: Implement services in `src/{service_pascal}.Core/Services/`
3. **Configure Database**: Modify `src/{service_pascal}.Infrastructure/Data/{service_pascal}DbContext.cs`
4. **Add Controllers**: Create API endpoints in `src/{service_pascal}.Api/Controllers/`

### API Endpoints

The service provides RESTful endpoints for {entity_pascal} management:

- `GET /api/{entity_pascal.lower()}s` - Get all {entity_pascal.lower()}s
- `GET /api/{entity_pascal.lower()}s/{{id}}` - Get {entity_pascal.lower()} by ID
- `POST /api/{entity_pascal.lower()}s` - Create new {entity_pascal.lower()}
- `PUT /api/{entity_pascal.lower()}s/{{id}}` - Update {entity_pascal.lower()}
- `DELETE /api/{entity_pascal.lower()}s/{{id}}` - Delete {entity_pascal.lower()}

### Integration with Gateway

This service is designed to work with the QaliTrack API Gateway:

- Routes are automatically configured
- Health checks are exposed for monitoring
- CORS is configured for cross-origin requests

## Project Structure

```
{service_pascal}/
├── src/
│   ├── {service_pascal}.Api/           # Web API layer
│   ├── {service_pascal}.Core/          # Business logic
│   └── {service_pascal}.Infrastructure/ # Data access
├── tests/
│   └── {service_pascal}.Tests/         # Unit & integration tests
├── Dockerfile                          # Container configuration
└── {service_pascal}.sln               # Solution file
```

## Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/{service_pascal}.Tests
```

## Docker

```bash
# Build Docker image
docker build -t {service_pascal.lower()} .

# Run container
docker run -p 5000:8080 {service_pascal.lower()}
```

## Contributing

1. Follow clean architecture principles
2. Add comprehensive tests
3. Update documentation
4. Follow naming conventions

Generated with QaliTrack Service Template 🚀
"""
    
    readme_path = target_dir / "README.md"
    with open(readme_path, 'w', encoding='utf-8') as f:
        f.write(readme_content)

def add_make_targets(service_type, service_kebab, service_pascal):
    """Add make targets for the new service to the Makefile"""
    script_dir = Path(__file__).parent
    repo_root = script_dir.parent
    makefile_path = repo_root / "Makefile"
    
    if not makefile_path.exists():
        print(f"Warning: Makefile not found at {makefile_path}")
        return
    
    try:
        with open(makefile_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Define the new make targets
        new_targets = f"""
# {service_pascal} Service Targets (Auto-generated)
.PHONY: build-{service_kebab} run-{service_kebab} test-{service_kebab}

build-{service_kebab}:
	@echo "Building {service_pascal} service..."
	@cd packages/microservices/{service_type}/{service_kebab} && dotnet build

run-{service_kebab}:
	@echo "Running {service_pascal} service..."
	@cd packages/microservices/{service_type}/{service_kebab} && dotnet run --project src/{service_pascal}.Api

test-{service_kebab}:
	@echo "Testing {service_pascal} service..."
	@cd packages/microservices/{service_type}/{service_kebab} && dotnet test tests/{service_pascal}.Tests --verbosity normal

docker-build-{service_kebab}:
	@echo "Building Docker image for {service_pascal} service..."
	@cd packages/microservices/{service_type}/{service_kebab} && docker build -t {service_kebab} .

docker-run-{service_kebab}:
	@echo "Running Docker container for {service_pascal} service..."
	@docker run -p 5000:80 {service_kebab}
"""
        
        # Add the new targets to the end of the Makefile
        updated_content = content + new_targets
        
        with open(makefile_path, 'w', encoding='utf-8') as f:
            f.write(updated_content)
        
        print(f"✅ Added make targets for {service_pascal}: build-{service_kebab}, run-{service_kebab}, test-{service_kebab}")
        
    except Exception as e:
        print(f"Warning: Could not add make targets to Makefile: {e}")

def main():
    """Main entry point"""
    if len(sys.argv) < 4:
        print("Usage: python generate-service.py <service-type> <service-name> <entity-name> [description]")
        print("\nService Types:")
        print("  masterdata   - For master data services (user, customer, product, etc.)")
        print("  datamanager  - For data management services (analytics, transaction, etc.)")
        print("\nExamples:")
        print("  python generate-service.py masterdata inventory-service inventory")
        print("  python generate-service.py datamanager analytics-service analytics 'Analytics Processing Service'")
        print("  python generate-service.py masterdata customer-service customer 'Customer Relationship Management'")
        sys.exit(1)
    
    service_type = sys.argv[1]
    service_name = sys.argv[2]
    entity_name = sys.argv[3]
    description = sys.argv[4] if len(sys.argv) > 4 else ""
    
    # Validate inputs
    if service_type not in ["masterdata", "datamanager"]:
        print("Error: Service type must be 'masterdata' or 'datamanager'")
        sys.exit(1)
    
    if not re.match(r'^[a-zA-Z][a-zA-Z0-9-_]*$', service_name):
        print("Error: Service name must start with a letter and contain only letters, numbers, hyphens, and underscores")
        sys.exit(1)
    
    if not re.match(r'^[a-zA-Z][a-zA-Z0-9]*$', entity_name):
        print("Error: Entity name must start with a letter and contain only letters and numbers")
        sys.exit(1)
    
    success = generate_service(service_type, service_name, entity_name, description)
    if not success:
        sys.exit(1)

if __name__ == "__main__":
    main()