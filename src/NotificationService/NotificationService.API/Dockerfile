# Runtime — keep in sync with repo-root Dockerfile.base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS app-runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj src/BuildingBlocks/BuildingBlocks.Messaging/
COPY src/NotificationService/NotificationService.API/NotificationService.API.csproj src/NotificationService/NotificationService.API/
RUN dotnet restore src/NotificationService/NotificationService.API/NotificationService.API.csproj
COPY src/BuildingBlocks/BuildingBlocks.Messaging/ src/BuildingBlocks/BuildingBlocks.Messaging/
COPY src/NotificationService/NotificationService.API/ src/NotificationService/NotificationService.API/
RUN dotnet publish src/NotificationService/NotificationService.API/NotificationService.API.csproj -c Release -o /app/publish --no-restore

FROM app-runtime
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.API.dll"]
