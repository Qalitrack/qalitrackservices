#!/bin/bash

# BackupService - Entity Framework Migrations Creation Script
# This script creates the initial database migrations for the BackupService

echo "Creating Entity Framework migrations for BackupService..."

# Ensure we're in the correct directory
cd "$(dirname "$0")"

# Create initial migration
echo "Creating InitialCreate migration..."
dotnet ef migrations add InitialCreate \
    --project BackupService.Infrastructure/BackupService.Infrastructure.csproj \
    --startup-project BackupService.API/BackupService.API.csproj \
    --context BackupServiceDbContext \
    --output-dir Migrations \
    --verbose

echo "Migration creation completed!"
echo ""
echo "To apply migrations to database, run:"
echo "dotnet ef database update --project BackupService.API/BackupService.API.csproj"