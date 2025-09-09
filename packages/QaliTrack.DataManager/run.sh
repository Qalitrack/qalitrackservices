#!/bin/bash

# QaliTrack Data Manager Run Script

set -e  # Exit on any error

echo "🚀 Starting QaliTrack Data Manager Service..."

# Load environment variables
if [ -f .env ]; then
    echo "📋 Loading environment variables from .env file..."
    export $(cat .env | xargs)
else
    echo "⚠️  .env file not found. Using default configuration..."
    echo "💡 Consider copying .env.sample to .env and customizing it"
fi

# Set default values if not provided
export DB_PROVIDER=${DB_PROVIDER:-SQLite}
export ASPNETCORE_URLS=${ASPNETCORE_URLS:-http://localhost:5001}
export ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Development}

echo "🗄️  Database Provider: $DB_PROVIDER"
echo "🌐 Service URL: $ASPNETCORE_URLS"
echo "🔧 Environment: $ASPNETCORE_ENVIRONMENT"

# Ensure database exists (auto-creates for SQLite, requires setup for others)
if [ "$DB_PROVIDER" = "SQLite" ]; then
    echo "📱 Using SQLite database (auto-created)"
elif [ "$DB_PROVIDER" = "PostgreSQL" ]; then
    echo "🐘 Using PostgreSQL database"
    echo "⚠️  Ensure PostgreSQL server is running and database exists"
elif [ "$DB_PROVIDER" = "SQLServer" ]; then
    echo "🔢 Using SQL Server database"
    echo "⚠️  Ensure SQL Server is running and database exists"
fi

echo ""
echo "🎯 Available endpoints:"
echo "  📚 Swagger UI:     $ASPNETCORE_URLS/"
echo "  💓 Health Check:   $ASPNETCORE_URLS/health"
echo "  📊 Weight Data:    $ASPNETCORE_URLS/weightdata"
echo "  🔄 Transactions:   $ASPNETCORE_URLS/transactions"
echo "  ✅ Compliance:     $ASPNETCORE_URLS/compliance"
echo "  📈 Analytics:      $ASPNETCORE_URLS/analytics"
echo "  ⚙️  Operations:     $ASPNETCORE_URLS/operations"
echo "  🔄 Data Sync:      $ASPNETCORE_URLS/datasync"
echo "  📦 Archive:        $ASPNETCORE_URLS/archive"
echo ""

# Run the application
echo "▶️  Starting service..."
cd src/QaliTrack.DataManager.Api
dotnet run --verbosity minimal

echo "🛑 Service stopped"