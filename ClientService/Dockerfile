FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ClientService/ClientService.csproj ClientService/
COPY Shared/Protos/inventory.proto Shared/Protos/
RUN dotnet restore ClientService/ClientService.csproj

COPY ClientService/ ClientService/
COPY Shared/Protos/ Shared/Protos/
RUN dotnet publish ClientService/ClientService.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5003
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ClientCustomerService.dll"]
