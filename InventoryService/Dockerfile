# ── Stage 1: Build 
# Build context is repo root (set in docker-compose.yml)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY InventoryService/InventoryService.csproj InventoryService/
COPY Shared/ Shared/
RUN dotnet restore InventoryService/InventoryService.csproj

COPY InventoryService/ InventoryService/
RUN dotnet publish InventoryService/InventoryService.csproj -c Release -o /app/publish

# ── Stage 2: Runtime 
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5001
EXPOSE 5101

ENTRYPOINT ["dotnet", "InventoryService.dll"]