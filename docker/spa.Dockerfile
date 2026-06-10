# Builds SPA application and serves it with nginx

FROM node:24 AS build-spa
WORKDIR /build
RUN corepack enable && corepack prepare pnpm@11.5.2 --activate

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
COPY src/hackathon-spa/package.json ./src/hackathon-spa/
COPY tests/e2e/package.json ./tests/e2e/
RUN pnpm install --frozen-lockfile --filter hackathon-spa...

COPY src/hackathon-spa ./src/hackathon-spa
RUN pnpm --filter hackathon-spa run build --outDir /app/publish

FROM nginx:1.28-alpine AS final

COPY --from=build-spa /app/publish /www/html
COPY docker/spa.nginx.conf /etc/nginx/conf.d/default.conf
