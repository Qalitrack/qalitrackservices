#!/bin/bash
# QaliTrack Business Flow Test Interface Launcher

echo "🏭 QaliTrack Business Flow Test Interface"
echo "========================================"

# Check if Python 3 is available
if command -v python3 >/dev/null 2>&1; then
    echo "✅ Python 3 found"
    PORT=${1:-8080}
    echo "🚀 Starting server on port $PORT..."
    python3 serve.py $PORT
elif command -v python >/dev/null 2>&1; then
    # Check if it's Python 3
    if python -c 'import sys; exit(0 if sys.version_info >= (3, 0) else 1)' 2>/dev/null; then
        echo "✅ Python 3 found (via python command)"
        PORT=${1:-8080}
        echo "🚀 Starting server on port $PORT..."
        python serve.py $PORT
    else
        echo "❌ Python 3 is required but only Python 2 was found"
        echo "Please install Python 3 or use 'python3' command"
        exit 1
    fi
else
    echo "❌ Python is not installed or not in PATH"
    echo "Please install Python 3 to run the test interface"
    echo ""
    echo "Alternatives:"
    echo "1. Use Node.js: npx http-server . -p 8080 -c-1"
    echo "2. Use PHP: php -S localhost:8080"
    echo "3. Open index.html directly in browser (may have CORS issues)"
    exit 1
fi