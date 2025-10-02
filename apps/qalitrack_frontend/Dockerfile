# Multi-stage Dockerfile for React UI with Node server
# Stage 1: Build the React app
FROM node:24-alpine AS builder

# Create non-root user for security
RUN addgroup -g 1001 -S nodejs && \
    adduser -S nextjs -u 1001

WORKDIR /src

# Copy package files
COPY package*.json ./

# Install all dependencies (including dev for build)
RUN npm ci

# Copy source code
COPY . .

# Build the React app
RUN npm run build

# Stage 2: Runtime (serve built app + server.js)
FROM node:24-alpine AS runner

# Create non-root user
RUN addgroup -g 1001 -S nodejs && \
    adduser -S nextjs -u 1001

WORKDIR /src

# Copy package files from builder
COPY --from=builder /src/package*.json ./
# Install only production dependencies (express for static serving)
RUN npm ci --only=production && npm cache clean --force

# Copy built assets and server.js
COPY --from=builder /src/dist ./dist
COPY --from=builder /src/server.js ./

# Change ownership to non-root user
RUN chown -R nextjs:nodejs /src
USER nextjs

# Expose port 4000 for the Express server
EXPOSE 4000

# Run only the Express server serving static files
CMD ["node", "server.js"]