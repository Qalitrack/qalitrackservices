# Use official Node.js runtime as base image
FROM node:18-alpine

# Install curl for health checks
RUN apk add --no-cache curl

# Set working directory
WORKDIR /usr/src/app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy application files
COPY . .

# Create a non-root user
RUN addgroup -g 1001 -S nodejs
RUN adduser -S ui5user -u 1001

# Change ownership of the app directory to the nodejs user
RUN chown -R ui5user:nodejs /usr/src/app

# Switch to non-root user
USER ui5user

# Expose port 5001 (S/4HANA Mock Server)
EXPOSE 5001

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5001/health || exit 1

# Start the application
CMD ["npm", "start"]