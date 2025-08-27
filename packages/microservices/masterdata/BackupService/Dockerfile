# Use the ASP.NET runtime as base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install PostgreSQL 16 client tools (to match your server version)
RUN apt-get update && \
    apt-get install -y wget gnupg lsb-release && \
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

# SDK build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ./BackupService.API/ BackupService.API/
COPY ./BackupService.Core/ BackupService.Core/
COPY ./BackupService.Infrastructure/ BackupService.Infrastructure/

# Restore dependencies
RUN dotnet restore "BackupService.API/BackupService.API.csproj"

# Build & publish
RUN dotnet build "BackupService.API/BackupService.API.csproj" -c Release -o /app/build
RUN dotnet publish "BackupService.API/BackupService.API.csproj" -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

RUN mkdir -p /app/logs

# Final verification - should show version 16.x
RUN which pg_dump && which pg_isready && pg_dump --version

ENTRYPOINT ["dotnet", "BackupService.API.dll"]
