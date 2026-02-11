# Use the official .NET 8 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install curl in the runtime image
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["ProductionGateway.csproj", "./"]
RUN dotnet restore "./ProductionGateway.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/."
RUN dotnet build "ProductionGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProductionGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80

# Run the application
ENTRYPOINT ["dotnet", "ProductionGateway.dll"]
