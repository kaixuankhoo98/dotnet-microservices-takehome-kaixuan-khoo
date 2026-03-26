# Runtime — keep in sync with repo-root Dockerfile.base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS app-runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj src/BuildingBlocks/BuildingBlocks.Messaging/
COPY src/PaymentService/PaymentService.API/PaymentService.API.csproj src/PaymentService/PaymentService.API/
RUN dotnet restore src/PaymentService/PaymentService.API/PaymentService.API.csproj
COPY src/BuildingBlocks/BuildingBlocks.Messaging/ src/BuildingBlocks/BuildingBlocks.Messaging/
COPY src/PaymentService/PaymentService.API/ src/PaymentService/PaymentService.API/
RUN dotnet publish src/PaymentService/PaymentService.API/PaymentService.API.csproj -c Release -o /app/publish --no-restore

FROM app-runtime
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.API.dll"]
