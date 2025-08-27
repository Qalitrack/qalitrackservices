# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install wget for health checks
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Use the SDK image to build the app and generate documentation
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install DocFX for documentation generation
RUN dotnet tool install -g docfx
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy project files
COPY ["src/QaliTrack.MasterData.Api/QaliTrack.MasterData.Api.csproj", "QaliTrack.MasterData.Api/"]
COPY ["src/QaliTrack.MasterData.Core/QaliTrack.MasterData.Core.csproj", "QaliTrack.MasterData.Core/"]
COPY ["src/QaliTrack.MasterData.Infrastructure/QaliTrack.MasterData.Infrastructure.csproj", "QaliTrack.MasterData.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "QaliTrack.MasterData.Api/QaliTrack.MasterData.Api.csproj"

# Copy all source files and documentation configuration
COPY src/ .
COPY docfx.json /src/
COPY index.md /src/
COPY toc.yml /src/
COPY docs/ /src/docs/

# Build the application in the source directory so DocFX can find assemblies
RUN dotnet build "QaliTrack.MasterData.Api/QaliTrack.MasterData.Api.csproj" -c Release

# Generate documentation - DocFX will find the built assemblies in bin/Release
WORKDIR /src
RUN docfx docfx.json

# Publish the application
FROM build AS publish
WORKDIR /src
RUN dotnet publish "QaliTrack.MasterData.Api/QaliTrack.MasterData.Api.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Copy the generated documentation to wwwroot for serving via ASP.NET Core
COPY --from=build /src/_site ./wwwroot/docs

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Create logs directory
RUN mkdir -p /app/logs

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "QaliTrack.MasterData.Api.dll"]