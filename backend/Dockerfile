FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj files first for layer caching of restore
COPY src/StockOrders.sln src/
COPY src/StockOrders.Domain/StockOrders.Domain.csproj src/StockOrders.Domain/
COPY src/StockOrders.Application/StockOrders.Application.csproj src/StockOrders.Application/
COPY src/StockOrders.Infrastructure/StockOrders.Infrastructure.csproj src/StockOrders.Infrastructure/
COPY src/StockOrders.Api/StockOrders.Api.csproj src/StockOrders.Api/

# Restore
RUN dotnet restore src/StockOrders.sln

# Copy everything else
COPY src/ src/

# Publish from root with explicit project path to avoid glob resolution issues on Linux
RUN ls -la /app/src/StockOrders.Api/ && ls -la /app/src/StockOrders.Api/Controllers/
RUN dotnet publish src/StockOrders.Api/StockOrders.Api.csproj -c Release -o /app/publish -p:EnableDefaultEmbeddedResourceItems=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "StockOrders.Api.dll"]
