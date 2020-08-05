FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as build

WORKDIR /src
COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Debug -o /dist


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

ENV ASPNETCORE_ENVIRONMENT Development
ENV ASPNETCORE_URLS https://+:5001;http://+:5000
EXPOSE 5000 5001

WORKDIR /app

COPY --from=build /dist .

CMD ["dotnet", "HTTPSPlayground.Web.dll"]
