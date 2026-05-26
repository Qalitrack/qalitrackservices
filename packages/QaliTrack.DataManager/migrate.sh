#!/bin/bash

# QaliTrack Data Manager Database Migration Script

set -e  # Exit on any error

echo "🗄️  QaliTrack Data Manager Database Migration..."

# Check if .NET SDK and EF Core tools are installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 8.0 SDK"
    exit 1
fi

# Check if EF Core tools are installed
if ! dotnet ef --version &> /dev/null; then
    echo "📥 Installing Entity Framework Core tools..."
    dotnet tool install --global dotnet-ef
fi

# Load environment variables
if [ -f .env ]; then
    echo "📋 Loading environment variables from .env file..."
    export $(cat .env | xargs)
fi

export DB_PROVIDER=${DB_PROVIDER:-SQLite}

echo "🗄️  Database Provider: $DB_PROVIDER"

# Ensure we're in the right directory
cd "$(dirname "$0")"

# Migration operations
case "${1:-help}" in
    "add")
        if [ -z "$2" ]; then
            echo "❌ Migration name required. Usage: ./migrate.sh add <MigrationName>"
            exit 1
        fi
        
        echo "➕ Creating migration: $2"
        dotnet ef migrations add "$2" \
            --project src/QaliTrack.DataManager.Infrastructure \
            --startup-project src/QaliTrack.DataManager.Api \
            --context DataManagerDbContext \
            --verbose
        ;;
        
    "update")
        echo "⬆️  Applying migrations to database..."
        dotnet ef database update \
            --project src/QaliTrack.DataManager.Infrastructure \
            --startup-project src/QaliTrack.DataManager.Api \
            --context DataManagerDbContext \
            --verbose
        echo "✅ Database updated successfully!"
        ;;
        
    "list")
        echo "📋 Available migrations:"
        dotnet ef migrations list \
            --project src/QaliTrack.DataManager.Infrastructure \
            --startup-project src/QaliTrack.DataManager.Api \
            --context DataManagerDbContext
        ;;
        
    "remove")
        echo "🗑️  Removing last migration..."
        dotnet ef migrations remove \
            --project src/QaliTrack.DataManager.Infrastructure \
            --startup-project src/QaliTrack.DataManager.Api \
            --context DataManagerDbContext \
            --verbose
        ;;
        
    "drop")
        echo "⚠️  This will DELETE the entire database!"
        read -p "Are you sure you want to drop the database? (y/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            dotnet ef database drop \
                --project src/QaliTrack.DataManager.Infrastructure \
                --startup-project src/QaliTrack.DataManager.Api \
                --context DataManagerDbContext \
                --force \
                --verbose
            echo "💥 Database dropped!"
        else
            echo "❌ Operation cancelled"
        fi
        ;;
        
    "reset")
        echo "🔄 Resetting database (drop + create + migrate)..."
        read -p "This will DELETE all data! Continue? (y/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            # Drop database
            dotnet ef database drop \
                --project src/QaliTrack.DataManager.Infrastructure \
                --startup-project src/QaliTrack.DataManager.Api \
                --context DataManagerDbContext \
                --force \
                --verbose || true
            
            # Apply migrations
            dotnet ef database update \
                --project src/QaliTrack.DataManager.Infrastructure \
                --startup-project src/QaliTrack.DataManager.Api \
                --context DataManagerDbContext \
                --verbose
            
            echo "✅ Database reset completed!"
        else
            echo "❌ Operation cancelled"
        fi
        ;;
        
    "init")
        echo "🆕 Initializing database with initial migration..."
        
        # Remove existing migrations directory if it exists
        if [ -d "src/QaliTrack.DataManager.Infrastructure/Migrations" ]; then
            echo "🗑️  Removing existing migrations..."
            rm -rf src/QaliTrack.DataManager.Infrastructure/Migrations
        fi
        
        # Create initial migration
        echo "➕ Creating InitialCreate migration..."
        dotnet ef migrations add InitialCreate \
            --project src/QaliTrack.DataManager.Infrastructure \
            --startup-project src/QaliTrack.DataManager.Api \
            --context DataManagerDbContext \
            --verbose
        
        # Apply migration
        echo "⬆️  Applying migration to database..."
        dotnet ef database update \
            --project src/QaliTrack.DataManager.Infrastructure \
            --startup-project src/QaliTrack.DataManager.Api \
            --context DataManagerDbContext \
            --verbose
        
        echo "✅ Database initialized successfully!"
        ;;
        
    "help"|*)
        echo "🗄️  QaliTrack Data Manager Migration Tool"
        echo ""
        echo "Usage: ./migrate.sh <command> [parameters]"
        echo ""
        echo "Commands:"
        echo "  init              Initialize database with initial migration"
        echo "  add <name>        Create a new migration"
        echo "  update           Apply pending migrations"
        echo "  list             List all migrations"
        echo "  remove           Remove the last migration"
        echo "  drop             Drop the database (⚠️  destructive)"
        echo "  reset            Drop and recreate database (⚠️  destructive)"
        echo "  help             Show this help"
        echo ""
        echo "Examples:"
        echo "  ./migrate.sh init                    # First-time setup"
        echo "  ./migrate.sh add AddNewColumn        # Add new migration"
        echo "  ./migrate.sh update                  # Apply migrations"
        echo "  ./migrate.sh list                    # Show migrations"
        echo ""
        echo "Database Provider: $DB_PROVIDER"
        ;;
esac