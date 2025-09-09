#!/bin/bash

# QaliTrack Data Manager Build Script

set -e  # Exit on any error

echo "🚀 Building QaliTrack Data Manager Service..."

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 8.0 SDK"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo "📦 Using .NET SDK version: $DOTNET_VERSION"

# Clean previous builds
echo "🧹 Cleaning previous builds..."
dotnet clean QaliTrack.DataManager.sln --verbosity minimal

# Restore dependencies
echo "📥 Restoring NuGet packages..."
dotnet restore QaliTrack.DataManager.sln --verbosity minimal

# Build the solution
echo "🔨 Building solution..."
dotnet build QaliTrack.DataManager.sln -c Release --no-restore --verbosity minimal

# Run tests
echo "🧪 Running tests..."
dotnet test tests/QaliTrack.DataManager.Tests/QaliTrack.DataManager.Tests.csproj -c Release --no-build --verbosity minimal

# Generate documentation (optional)
if command -v docfx &> /dev/null; then
    echo "📖 Generating documentation with DocFX..."
    docfx docfx.json --build --output _site
    echo "📚 Documentation generated in _site/ directory"
else
    echo "⚠️  DocFX not found. Install with: dotnet tool install -g docfx"
    echo "   Documentation generation skipped"
fi

echo "✅ Build completed successfully!"

# Check if .env file exists
if [ ! -f .env ]; then
    echo "⚠️  .env file not found. Creating from sample..."
    cp .env.sample .env
    echo "📝 Please edit .env file with your configuration before running the service"
fi

echo ""
echo "🎯 Next steps:"
echo "1. Edit .env file with your database configuration"
echo "2. Run database migrations: ./migrate.sh"
echo "3. Start the service: ./run.sh"
echo ""
echo "📚 Documentation:"
echo "   - API Documentation: http://localhost:5000 (Swagger UI)"
if [ -d "_site" ]; then
    echo "   - DocFX Documentation: _site/index.html"
fi
echo "   - README: $(pwd)/README.md"