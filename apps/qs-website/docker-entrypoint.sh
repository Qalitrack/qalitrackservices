#!/bin/sh

# Replace environment variables in config.js
if [ -f /usr/share/nginx/html/config.js ]; then
  echo "Replacing API_BASE_URL with: ${VITE_API_BASE_URL}"
  sed -i "s|__VITE_API_BASE_URL__|${VITE_API_BASE_URL}|g" /usr/share/nginx/html/config.js
fi

# Start nginx
exec "$@"