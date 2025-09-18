#!/bin/bash

# Docker entrypoint script for Next.js app
# This script replaces environment variable placeholders at runtime

echo "Starting Next.js application with runtime configuration..."

# Define the config file path
CONFIG_FILE="/app/public/config.js"

# Check if config file exists
if [ ! -f "$CONFIG_FILE" ]; then
    echo "Warning: Config file not found at $CONFIG_FILE"
    exit 1
fi

echo "Replacing environment variables in config.js..."

# Replace placeholders with actual environment variable values
sed -i "s|__NEXT_PUBLIC_API_BASE_URL__|${NEXT_PUBLIC_API_BASE_URL:-http://localhost:5000/api}|g" "$CONFIG_FILE"
sed -i "s|__NEXT_PUBLIC_SITE_URL__|${NEXT_PUBLIC_SITE_URL:-http://localhost:3000}|g" "$CONFIG_FILE"
sed -i "s|__NEXT_PUBLIC_ASSETS_URL__|${NEXT_PUBLIC_ASSETS_URL:-http://localhost:3000}|g" "$CONFIG_FILE"

echo "Configuration updated:"
cat "$CONFIG_FILE"

# Start the Next.js application
echo "Starting Next.js server..."
exec npm start