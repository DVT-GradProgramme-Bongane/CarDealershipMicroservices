FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AccessoriesSuppliersService/AccessoriesSuppliersService.csproj AccessoriesSuppliersService/
RUN dotnet restore AccessoriesSuppliersService/AccessoriesSuppliersService.csproj

COPY AccessoriesSuppliersService/ AccessoriesSuppliersService/
WORKDIR /src/AccessoriesSuppliersService
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5007
ENTRYPOINT ["dotnet", "AccessoriesSuppliersService.dll"]
