# Builds SPA application and serves it with nginx

FROM node:24 AS build-spa
WORKDIR /build

COPY package.json package-lock.json ./
COPY src/hackathon-spa/package.json ./src/hackathon-spa/
RUN npm ci

COPY src/hackathon-spa ./src/hackathon-spa
RUN npm run build --workspace=src/hackathon-spa -- --outDir /app/publish

FROM nginx:1.28-alpine AS final

COPY --from=build-spa /app/publish /www/html
COPY docker/spa.nginx.conf /etc/nginx/conf.d/default.conf
