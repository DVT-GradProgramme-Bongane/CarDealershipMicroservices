FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY UsedCarSalesService/*.csproj UsedCarSalesService/
COPY Shared/Protos/. Shared/Protos/
RUN dotnet restore UsedCarSalesService/UsedCarSalesService.csproj

COPY UsedCarSalesService/. UsedCarSalesService/
WORKDIR /src/UsedCarSalesService
RUN dotnet publish UsedCarSalesService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5005

ENV ASPNETCORE_URLS=http://+:5005

ENTRYPOINT ["dotnet", "UsedCarSalesService.dll"]