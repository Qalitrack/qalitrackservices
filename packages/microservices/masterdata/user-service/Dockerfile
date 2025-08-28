# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install wget for health checks and PostgreSQL client tools
RUN apt-get update && \
    apt-get install -y \
        wget \
        gnupg \
        lsb-release \
    && wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | apt-key add - \
    && echo "deb http://apt.postgresql.org/pub/repos/apt/ $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list \
    && apt-get update \
    && apt-get install -y postgresql-client-16 \
    && rm -rf /var/lib/apt/lists/*

# Use the SDK image to build the app with DocFX for documentation generation
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install DocFX for documentation generation
RUN dotnet tool install -g docfx
ENV PATH="$PATH:/root/.dotnet/tools"

# 2️⃣ Add local NuGet source
# RUN dotnet nuget add source "/src/nuget" --name LocalContracts

# 3️⃣ Copy UserService project files
COPY ["src/UserService.Api/UserService.Api.csproj", "UserService.Api/"]
COPY ["src/UserService.Core/UserService.Core.csproj", "UserService.Core/"]
COPY ["src/UserService.Infrastructure/UserService.Infrastructure.csproj", "UserService.Infrastructure/"]

# Copy documentation configuration files
COPY docfx.json /src/
COPY index.md /src/
COPY toc.yml /src/
# Copy docs directory if it exists
COPY docs/ /src/docs/

# 4️⃣ Restore dependencies (local NuGet is now available)
RUN dotnet restore "UserService.Api/UserService.Api.csproj"

# 5️⃣ Copy all source files
COPY src/ .

# 6️⃣ Build the application in the source directory so DocFX can find assemblies
RUN dotnet build "UserService.Api/UserService.Api.csproj" -c Release -o /app/build

# Install Node.js (LTS)
RUN apt-get update && apt-get install -y curl gnupg \
    && curl -fsSL https://deb.nodesource.com/setup_18.x | bash - \
    && apt-get install -y nodejs \
    && rm -rf /var/lib/apt/lists/*

# Generate documentation - DocFX will find the built assemblies in bin/Release
WORKDIR /src
RUN docfx docfx.json

# Publish the application
FROM build AS publish
WORKDIR /src
RUN dotnet publish "UserService.Api/UserService.Api.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Copy the generated documentation to wwwroot for serving via ASP.NET Core
COPY --from=publish /src/_site ./wwwroot/docs

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Create logs directory
RUN mkdir -p /app/logs

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "UserService.Api.dll"]