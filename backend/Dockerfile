# Use the official .NET 10 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copy solution and project files
COPY src/StockOrders.sln ./
COPY src/StockOrders.Domain/*.csproj ./src/StockOrders.Domain/
COPY src/StockOrders.Application/*.csproj ./src/StockOrders.Application/
COPY src/StockOrders.Infrastructure/*.csproj ./src/StockOrders.Infrastructure/
COPY src/StockOrders.Api/*.csproj ./src/StockOrders.Api/

# Restore dependencies
RUN dotnet restore src/StockOrders.Api/StockOrders.Api.csproj

# Copy the rest of the source code
COPY . .

# Build and publish
RUN dotnet publish src/StockOrders.Api/StockOrders.Api.csproj -c Release -o out

# Use the official .NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose port 80 or 8080 (ASP.NET default)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "StockOrders.Api.dll"]
