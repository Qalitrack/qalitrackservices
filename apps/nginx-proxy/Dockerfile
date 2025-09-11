FROM nginx:alpine

# Install certbot and bash for SSL certificate management
RUN apk add --no-cache certbot bash

# Copy our custom entrypoint
COPY entrypoint.sh /docker-entrypoint.d/99-setup-domains.sh
RUN chmod +x /docker-entrypoint.d/99-setup-domains.sh

# Standard nginx ports
EXPOSE 80 443