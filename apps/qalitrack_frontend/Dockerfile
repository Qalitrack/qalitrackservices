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
# Install only production dependencies
RUN npm ci --only=production && npm cache clean --force
RUN npm install -g concurrently
# Copy built assets, server.js, and .env
COPY --from=builder /src/dist ./dist
COPY --from=builder /src/server.js ./

# Copy source if needed for server.js (e.g., if it serves dynamic content)
COPY --from=builder /src ./

# Change ownership to non-root user
RUN chown -R nextjs:nodejs /src
USER nextjs

# Expose ports (adjust as needed)
EXPOSE 3000 5000

# For dev/prod hybrid: Run both commands concurrently
# Assumes "concurrently" is in your package.json devDependencies
CMD ["sh", "-c", "concurrently 'node server.js' 'npm run dev -- --host 0.0.0.0'"]