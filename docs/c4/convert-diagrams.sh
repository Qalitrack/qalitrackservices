#!/bin/bash

# Script to convert Mermaid diagrams to PNG
# Prerequisites: npm install -g @mermaid-js/mermaid-cli

echo "🔄 Converting C4 Mermaid diagrams to PNG..."

# Create assets directory if it doesn't exist
mkdir -p assets

# Extract and convert individual diagrams
echo "📊 Converting System Context diagrams..."

# Check if mermaid CLI is installed
if ! command -v mmdc &> /dev/null; then
    echo "❌ Mermaid CLI not found. Install with: npm install -g @mermaid-js/mermaid-cli"
    exit 1
fi

# Convert system context diagrams
echo "Converting C4 Context diagram..."
mmdc -i system-context-c4.mmd -o assets/system-context-c4.png -t default -b white

echo "Converting Flowchart diagram (3x size)..."
mmdc -i system-context-flowchart.mmd -o assets/system-context-flowchart.png -t default -b white -s 3

echo "✅ Conversion complete!"
echo "📁 PNG files saved in assets/ directory:"
ls -la assets/*.png

echo ""
echo "🌐 Alternative: Use https://mermaid.live/ for online conversion"