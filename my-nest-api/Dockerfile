# Stage 1: Build
FROM node:20-alpine AS builder
WORKDIR /app

# Install dependencies
COPY package*.json ./
RUN npm ci

# Copy source and build
COPY . .
RUN npm run build

# Stage 2: Run
FROM node:20-alpine
WORKDIR /app

# Install curl for health checks
RUN apk add --no-cache curl

# Copy built output and dependencies
COPY --from=builder /app/package*.json ./
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/dist ./dist

# Expose default Nest port
EXPOSE 3000

CMD ["node", "dist/main.js"]
