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

COPY docker/nginx-entrypoint.sh /
COPY docker/spa.nginx.conf /nginx.conf
RUN chmod +x /nginx-entrypoint.sh

RUN mkdir /certificates
VOLUME /certificates

EXPOSE 443
ENTRYPOINT [ "/nginx-entrypoint.sh" ]
