# Build stage
FROM node:18-alpine AS builder

WORKDIR /app

# Install pnpm (use latest version to match lockfile)
RUN npm install -g pnpm@latest

# Copy package files
COPY package.json pnpm-lock.yaml ./

# Install dependencies (force recreate lockfile if incompatible)
RUN pnpm install --frozen-lockfile || pnpm install --force

# Copy source code
COPY . .

# Build the application with standalone output
RUN pnpm run build

# Runtime stage
FROM node:18-alpine AS runner

WORKDIR /app

# Create a non-root user
RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

# Copy built application
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static
COPY --from=builder --chown=nextjs:nodejs /app/public ./public

# Create directory for runtime config
RUN mkdir -p /app/public && chown nextjs:nodejs /app/public

# Switch to non-root user
USER nextjs

# Expose port
EXPOSE 3000

# Set environment variables with defaults
ENV NODE_ENV=production
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD node healthcheck.js

# Start the application
CMD ["node", "server.js"]