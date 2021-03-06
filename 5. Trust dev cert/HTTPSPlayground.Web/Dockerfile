FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as build

WORKDIR /ssl
COPY ./localhost.pfx .

# convert pfx to pem/key and to pfx w/o pass
#   password is only in the unpublished stage
RUN apk update \
    && apk add ca-certificates openssl \
    && rm -rf /var/cache/apk/* \
    && openssl pkcs12 -in localhost.pfx -out localhost.pem -nokeys -nodes -passin pass:localhost \
    && openssl pkcs12 -in localhost.pfx -out localhost.key -nocerts -nodes -passin pass:localhost \
    && openssl pkcs12 -export -out localhost-nopass.pfx -inkey localhost.key -in localhost.pem -passout pass: \
    && chmod 644 * \
    && chmod 600 *.pfx

WORKDIR /src
COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Debug -o /dist


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

COPY --from=build /ssl/localhost.pem /usr/local/share/ca-certificates/localhost.crt
COPY --from=build /ssl/localhost-nopass.pfx /root/.dotnet/corefx/cryptography/x509stores/my/
# DON'T COPY *.key !!!

# trust cert:
RUN apk update \
    && apk add ca-certificates \
    && rm -rf /var/cache/apk/* \
    && update-ca-certificates 2>/dev/null \
    && ls /etc/ssl/certs/*localhost* \
    && echo 'cert imported correctly if listed above'

ENV ASPNETCORE_ENVIRONMENT Development
ENV ASPNETCORE_URLS https://+:5001;http://+:5000
EXPOSE 5000 5001

WORKDIR /app

COPY --from=build /dist .

CMD ["dotnet", "HTTPSPlayground.Web.dll"]
