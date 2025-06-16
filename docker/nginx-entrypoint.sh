#!/bin/sh

# Exit on any error
set -e

# Check if environment variables are set
if [ -z "$CERT_PATH" ]; then
    echo "Error: CERT_PATH environment variable is not set"
    exit 1
fi

if [ -z "$KEY_PATH" ]; then
    echo "Error: KEY_PATH environment variable is not set"
    exit 1
fi

echo "Updating nginx configuration..."
sed -e "s|CERT_PATH|$CERT_PATH|g" \
    -e "s|KEY_PATH|$KEY_PATH|g" \
    /nginx.conf > /etc/nginx/conf.d/default.conf

echo "Starting nginx"
exec "/docker-entrypoint.sh" nginx -g "daemon off;"
