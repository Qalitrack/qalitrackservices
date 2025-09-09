#!/bin/bash

# Regenerate documentation for QaliTrack Gateway
echo "🔧 Building solution with XML documentation..."
dotnet build src/QaliTrack.Gateway.csproj -c Release

echo "📚 Generating API documentation with DocFX..."
docfx metadata docfx.json --force

echo "🎨 Building documentation site..."
docfx build docfx.json --force

echo "📂 Copying documentation to wwwroot for serving..."
mkdir -p src/wwwroot/docs
cp -r _site/* src/wwwroot/docs/

echo "✅ Documentation regenerated successfully!"
echo "📖 View at: http://localhost:7002/docs/index.html"