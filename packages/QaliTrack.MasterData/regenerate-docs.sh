#!/bin/bash

# Regenerate documentation for QaliTrack MasterData
echo "🔧 Building solution with XML documentation..."
dotnet build src/QaliTrack.MasterData.Api/QaliTrack.MasterData.Api.csproj -c Release

echo "📚 Generating API documentation with DocFX..."
docfx metadata docfx.json --force

echo "🎨 Building documentation site..."
docfx build docfx.json --force

echo "✅ Documentation regenerated successfully!"
echo "📖 View at: http://localhost:7001/docs/index.html"