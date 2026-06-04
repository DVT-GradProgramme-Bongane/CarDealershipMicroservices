## Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY NotificationService/NotificationService.csproj NotificationService/
RUN dotnet restore NotificationService/NotificationService.csproj

COPY NotificationService/ NotificationService/
WORKDIR /src/NotificationService
RUN dotnet publish -c Release -o /app/publish

## Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5009
ENTRYPOINT ["dotnet", "NotificationService.dll"]