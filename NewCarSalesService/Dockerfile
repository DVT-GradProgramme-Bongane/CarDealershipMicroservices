FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NewCarSalesService/NewCarSalesService.csproj", "NewCarSalesService/"]
RUN dotnet restore "NewCarSalesService/NewCarSalesService.csproj"
COPY . .
WORKDIR "/src/NewCarSalesService"
RUN dotnet build "./NewCarSalesService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NewCarSalesService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NewCarSalesService.dll"]