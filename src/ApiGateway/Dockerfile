# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS app-runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj src/BuildingBlocks/BuildingBlocks.Messaging/
COPY src/ApiGateway/ApiGateway.csproj src/ApiGateway/
RUN dotnet restore src/ApiGateway/ApiGateway.csproj

COPY src/BuildingBlocks/BuildingBlocks.Messaging/ src/BuildingBlocks/BuildingBlocks.Messaging/
COPY src/ApiGateway/ src/ApiGateway/
RUN dotnet publish src/ApiGateway/ApiGateway.csproj -c Release -o /app/publish --no-restore

FROM app-runtime
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]

