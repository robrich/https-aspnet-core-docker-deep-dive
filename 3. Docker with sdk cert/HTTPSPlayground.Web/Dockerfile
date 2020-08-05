FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as build

# generate the dev cert if it doesn't already exist
RUN dotnet dev-certs https

WORKDIR /src
COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Debug -o /dist


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

COPY --from=build /root/.dotnet/corefx/cryptography /root/.dotnet/corefx/cryptography

ENV ASPNETCORE_ENVIRONMENT Development
ENV ASPNETCORE_URLS https://+:5001;http://+:5000
EXPOSE 5000 5001

WORKDIR /app

COPY --from=build /dist .

CMD ["dotnet", "HTTPSPlayground.Web.dll"]
