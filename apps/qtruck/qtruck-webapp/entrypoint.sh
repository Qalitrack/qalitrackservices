#!/bin/sh

# Replace hardcoded localhost URLs with runtime environment variables
if [ -n "$NEXT_PUBLIC_API_URL" ]; then
    echo "Replacing hardcoded localhost URLs with: $NEXT_PUBLIC_API_URL"
    
    # Find and replace in all JavaScript files
    find /app/.next -name "*.js" -type f -exec sed -i "s|http://localhost:8000|${NEXT_PUBLIC_API_URL}|g" {} \;
    find /app/.next -name "*.json" -type f -exec sed -i "s|http://localhost:8000|${NEXT_PUBLIC_API_URL}|g" {} \;
    
    echo "URL replacement completed"
else
    echo "NEXT_PUBLIC_API_URL not set, skipping URL replacement"
fi

# Start the Next.js server
exec node server.js