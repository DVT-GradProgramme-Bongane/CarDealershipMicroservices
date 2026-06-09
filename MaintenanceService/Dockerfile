FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY Shared/ ./Shared/
COPY MaintenanceService/ ./MaintenanceService/

WORKDIR /app/MaintenanceService
RUN dotnet restore
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
EXPOSE 5008
ENTRYPOINT ["dotnet", "MaintenanceService.dll"]