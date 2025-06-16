# Proxy server for the standalone SPA and API applications

FROM nginx:1.28-alpine

COPY nginx-entrypoint.sh /
COPY reverse-proxy.nginx.conf /nginx.conf
RUN chmod +x /nginx-entrypoint.sh

RUN mkdir /certificates
VOLUME /certificates

EXPOSE 443
ENTRYPOINT [ "/nginx-entrypoint.sh" ]
