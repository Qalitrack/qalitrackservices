# QaliTrack Data Manager Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["QaliTrack.DataManager.sln", "."]
COPY ["src/QaliTrack.DataManager.Api/QaliTrack.DataManager.Api.csproj", "src/QaliTrack.DataManager.Api/"]
COPY ["src/QaliTrack.DataManager.Core/QaliTrack.DataManager.Core.csproj", "src/QaliTrack.DataManager.Core/"]
COPY ["src/QaliTrack.DataManager.Infrastructure/QaliTrack.DataManager.Infrastructure.csproj", "src/QaliTrack.DataManager.Infrastructure/"]
COPY ["tests/QaliTrack.DataManager.Tests/QaliTrack.DataManager.Tests.csproj", "tests/QaliTrack.DataManager.Tests/"]

# Restore dependencies
RUN dotnet restore "QaliTrack.DataManager.sln"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/QaliTrack.DataManager.Api"
RUN dotnet build "QaliTrack.DataManager.Api.csproj" -c Release -o /app/build

# Run tests
WORKDIR "/src"
RUN dotnet test "tests/QaliTrack.DataManager.Tests/QaliTrack.DataManager.Tests.csproj" --no-build --verbosity normal

FROM build AS publish
WORKDIR "/src/src/QaliTrack.DataManager.Api"
RUN dotnet publish "QaliTrack.DataManager.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DB_PROVIDER=SQLite
ENV DB_PATH=/data/datamanager.db

# Create data directory
RUN mkdir -p /data

# Set up volume for database
VOLUME ["/data"]

# Labels for image metadata
LABEL maintainer="QaliTrack Team"
LABEL version="1.0.0"
LABEL description="QaliTrack Data Manager - Consolidated operational data service"
LABEL org.label-schema.name="QaliTrack Data Manager"
LABEL org.label-schema.description="Django-style modular data manager for QaliTrack weighbridge system"
LABEL org.label-schema.version="1.0.0"
LABEL org.label-schema.schema-version="1.0"

ENTRYPOINT ["dotnet", "QaliTrack.DataManager.Api.dll"]