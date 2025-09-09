#!/bin/bash

# Regenerate documentation for QaliTrack Data Manager
echo "🔧 Building solution with XML documentation..."
dotnet build src/QaliTrack.DataManager.Api/QaliTrack.DataManager.Api.csproj -c Release

echo "📚 Generating API documentation with DocFX..."
docfx metadata docfx.json

echo "🎨 Building documentation site..."
docfx build docfx.json

echo "✅ Documentation regenerated successfully!"
echo "📖 View at: file://$(pwd)/_site/index.html"
echo "🌐 Or serve locally with: docfx serve _site --port 8080"