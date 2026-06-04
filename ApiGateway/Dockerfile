FROM node:24-alpine

# Create a non-root user for security
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

WORKDIR /app

# Install dependencies (cached layer if package.json unchanged)
COPY package*.json ./
RUN npm ci --omit=dev

# Copy application source
COPY . .

# Switch to non-root user
USER appuser

EXPOSE 3000

# Use node directly (not npm) for proper signal handling
CMD ["node", "server.js"]