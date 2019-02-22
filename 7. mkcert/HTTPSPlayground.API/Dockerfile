FROM golang:alpine as cert

RUN apk update \
    && apk add git openssl nss-tools \
    && rm -rf /var/cache/apk/*

RUN cd /go && \
    go get -u github.com/FiloSottile/mkcert && \
    cd src/github.com/FiloSottile/mkcert && \
    go build -o /bin/mkcert

WORKDIR /root/.local/share/mkcert
# copied from %localappdata%\mkcert
COPY rootCA*.pem /root/.local/share/mkcert/

# make a pfx with known password by making a pem/key then converting it
# includes minikube and docker ips
RUN mkcert -install \
    && mkcert -key-file https-api.key -cert-file https-api.pem minikube 192.168.184.148 localhost 127.0.0.1 api \
    && openssl pkcs12 -export -out https-api.pfx -inkey https-api.key -in https-api.pem -certfile rootCA.pem -passout pass:https-api



FROM microsoft/dotnet:2.2-sdk-alpine as build

WORKDIR /src
COPY *.csproj .
RUN dotnet restore

# also copies rootCA-key.pem, but we'll throw this image away in a minute
COPY . .
RUN dotnet publish -c Debug -o /dist



FROM microsoft/dotnet:2.2-aspnetcore-runtime-alpine

COPY --from=cert /bin/mkcert /bin/mkcert
# root cert but not root private key:
COPY --from=cert /root/.local/share/mkcert/rootCA.pem /root/.local/share/mkcert/rootCA.pem
COPY --from=cert /root/.local/share/mkcert/https-api.pfx /app/
# TODO: consider `chmod 600 *.pem and *.pfx`

# tell ASP.NET Core to use the cert
ENV Kestrel__Certificates__Default__Path=/app/https-api.pfx
ENV Kestrel__Certificates__Default__Password=https-api

# trust root cert
RUN apk update \
    && apk add nss-tools \
    && rm -rf /var/cache/apk/* \
    && mkcert -install \
    && apk del nss-tools \
    && rm -rf /bin/mkcert

WORKDIR /app
COPY --from=build /dist /app

# so no HSTS:
ENV ASPNETCORE_ENVIRONMENT Development
ENV ASPNETCORE_URLS https://+:4001;http://+:4000
EXPOSE 4000 4001

CMD ["dotnet", "HTTPSPlayground.API.dll"]
