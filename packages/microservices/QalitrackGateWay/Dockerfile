# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["QalitrackGateWay.sln", "./"]
COPY ["QalitrackGateWay/QalitrackGateWay.csproj", "QalitrackGateWay/"]
RUN dotnet restore "QalitrackGateWay.sln"

# Copy everything else and build
COPY . .
WORKDIR "/src/QalitrackGateWay"

# Publish the application with production settings
RUN dotnet publish "QalitrackGateWay.csproj" -c Release -o /app/publish /p:UseAppHost=false \
    /p:GenerateRuntimeConfigurationFiles=true /p:EnvironmentName=Production

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for healthchecks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 80

# Copy the published app
COPY --from=build /app/publish .

# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV ASPNETCORE_HTTPS_DISABLE=1
ENV DOTNET_EnableDiagnostics=0

# Set the entry point
ENTRYPOINT ["dotnet", "QalitrackGateWay.dll"]
