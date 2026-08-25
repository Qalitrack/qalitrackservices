# Use the ASP.NET runtime as base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install PostgreSQL 16 client tools and wget for health checks
RUN apt-get update && \
    apt-get install -y wget gnupg lsb-release curl && \
    # Add PostgreSQL repository for version 16 - use bookworm (Debian 12) explicitly
    sh -c 'echo "deb http://apt.postgresql.org/pub/repos/apt bookworm-pgdg main" > /etc/apt/sources.list.d/pgdg.list' && \
    wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | gpg --dearmor -o /usr/share/keyrings/postgresql-archive-keyring.gpg && \
    chmod 644 /usr/share/keyrings/postgresql-archive-keyring.gpg && \
    echo "deb [signed-by=/usr/share/keyrings/postgresql-archive-keyring.gpg] http://apt.postgresql.org/pub/repos/apt bookworm-pgdg main" > /etc/apt/sources.list.d/pgdg.list && \
    apt-get update && \
    apt-get install -y --no-install-recommends postgresql-client-16 && \
    rm -rf /var/lib/apt/lists/* && \
    # Verify the installation
    which pg_dump && \
    which pg_isready && \
    pg_dump --version

# Static Docker CLI (client only, no daemon) so this container can `docker exec`/
# `docker run` against postgres-prod to drive pgBackRest, which must run where
# PGDATA lives rather than over a network connection like pg_dump/pg_restore.
RUN curl -fsSL https://download.docker.com/linux/static/stable/x86_64/docker-25.0.5.tgz \
      -o /tmp/docker.tgz && \
    tar -xzf /tmp/docker.tgz -C /tmp && \
    mv /tmp/docker/docker /usr/local/bin/docker && \
    chmod +x /usr/local/bin/docker && \
    rm -rf /tmp/docker.tgz /tmp/docker && \
    docker --version

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY BackupService.API/ BackupService.API/
COPY BackupService.Core/ BackupService.Core/
COPY BackupService.Infrastructure/ BackupService.Infrastructure/

# Restore dependencies
RUN dotnet restore "BackupService.API/BackupService.API.csproj"

# Build the application
RUN dotnet build "BackupService.API/BackupService.API.csproj" -c Release

# Publish stage
FROM build AS publish
WORKDIR /src
RUN dotnet publish "BackupService.API/BackupService.API.csproj" -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Create logs directory
RUN mkdir -p /app/logs

# Final verification - should show version 16.x
RUN which pg_dump && which pg_isready && pg_dump --version

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD wget -qO /dev/null http://localhost:80/Backup/available || exit 1

ENTRYPOINT ["dotnet", "BackupService.API.dll"]