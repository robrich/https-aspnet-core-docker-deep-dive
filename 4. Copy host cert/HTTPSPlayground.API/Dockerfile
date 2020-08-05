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

COPY --from=build /ssl/localhost-nopass.pfx /root/.dotnet/corefx/cryptography/x509stores/my/
# DON'T COPY *.key !!!

ENV ASPNETCORE_ENVIRONMENT Development
ENV ASPNETCORE_URLS https://+:4001;http://+:4000
EXPOSE 4000 4001

WORKDIR /app

COPY --from=build /dist .

CMD ["dotnet", "HTTPSPlayground.API.dll"]
