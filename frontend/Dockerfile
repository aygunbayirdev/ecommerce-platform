# Local dev-parity container — for daily development, `npm run dev` outside Docker remains the
# primary flow (Next.js itself recommends this on Mac/Windows for performance). This image exists
# for Docker Compose parity and as groundwork for the Faz 6 deploy.

FROM node:22-alpine AS deps
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci

FROM node:22-alpine AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Inlined into the client bundle at build time (Next.js NEXT_PUBLIC_ convention) — these are the
# URLs the *browser* calls, so they stay host-published addresses (http://localhost:8080/api etc.),
# not Docker service names. See src/lib/server-fetch.ts for the separate server-only counterpart
# (INTERNAL_API_URL) used by SSR fetches running inside this container.
ARG NEXT_PUBLIC_API_URL
ARG NEXT_PUBLIC_PAYMENT_API_URL
ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_PAYMENT_API_URL=$NEXT_PUBLIC_PAYMENT_API_URL

RUN npm run build

FROM node:22-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production
ENV PORT=3000
ENV HOSTNAME=0.0.0.0

COPY --from=builder /app/public ./public
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static

EXPOSE 3000
CMD ["node", "server.js"]
