#!/bin/bash

# Create Dockerfiles for all services

# Master Data Services
services=(
    "packages/microservices/masterdata/organization-service:OrganizationService"
    "packages/microservices/masterdata/vehicle-service:VehicleService"
    "packages/microservices/masterdata/driver-service:DriverService"
    "packages/microservices/masterdata/product-service:ProductService"
    "packages/microservices/masterdata/route-service:RouteService"
    "packages/microservices/masterdata/weighbridge-service:WeighbridgeService"
    "packages/microservices/masterdata/customer-service:CustomerService"
    "packages/microservices/masterdata/supplier-service:SupplierService"
    "packages/microservices/masterdata/transporter-service:TransporterService"
    "packages/microservices/masterdata/sacco-service:SaccoService"
)

# Data Manager Services
data_services=(
    "packages/microservices/datamanager/weight-data-service:WeightDataService"
    "packages/microservices/datamanager/compliance-service:ComplianceService"
    "packages/microservices/datamanager/operational-data-service:OperationalDataService"
    "packages/microservices/datamanager/transaction-service:TransactionService"
    "packages/microservices/datamanager/analytics-service:AnalyticsService"
    "packages/microservices/datamanager/data-sync-service:DataSyncService"
    "packages/microservices/datamanager/archive-service:ArchiveService"
)

# Function to create Dockerfile
create_dockerfile() {
    local service_path=$1
    local service_name=$2
    
    cat > "${service_path}/Dockerfile" << EOF
# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/${service_name}.Api/${service_name}.Api.csproj", "${service_name}.Api/"]
COPY ["src/${service_name}.Core/${service_name}.Core.csproj", "${service_name}.Core/"]
COPY ["src/${service_name}.Infrastructure/${service_name}.Infrastructure.csproj", "${service_name}.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "${service_name}.Api/${service_name}.Api.csproj"

# Copy all source files
COPY src/ .

# Build the application
RUN dotnet build "${service_name}.Api/${service_name}.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "${service_name}.Api/${service_name}.Api.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "${service_name}.Api.dll"]
EOF
    
    echo "Created Dockerfile for ${service_path}"
}

# Create Dockerfiles for master data services
for service in "${services[@]}"; do
    IFS=':' read -ra ADDR <<< "$service"
    service_path="${ADDR[0]}"
    service_name="${ADDR[1]}"
    create_dockerfile "$service_path" "$service_name"
done

# Create Dockerfiles for data manager services
for service in "${data_services[@]}"; do
    IFS=':' read -ra ADDR <<< "$service"
    service_path="${ADDR[0]}"
    service_name="${ADDR[1]}"
    create_dockerfile "$service_path" "$service_name"
done

echo "All Dockerfiles created successfully!"