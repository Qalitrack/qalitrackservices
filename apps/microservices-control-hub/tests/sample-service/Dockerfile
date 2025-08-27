FROM node:18-alpine

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci --omit=dev

# Copy application code and documentation
COPY app.js ./
COPY docs/ ./docs/

# Create non-root user
RUN addgroup -g 1001 -S nodejs && \
    adduser -S sampleuser -u 1001

# Change ownership
RUN chown -R sampleuser:nodejs /app
USER sampleuser

# Expose port
EXPOSE 3001

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD node -e "require('http').get('http://localhost:3001/health', (res) => { process.exit(res.statusCode === 200 ? 0 : 1) }).on('error', () => process.exit(1))"

# Start the application
CMD ["node", "app.js"]