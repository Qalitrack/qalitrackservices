# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/ArchiveService.Api/ArchiveService.Api.csproj", "ArchiveService.Api/"]
COPY ["src/ArchiveService.Core/ArchiveService.Core.csproj", "ArchiveService.Core/"]
COPY ["src/ArchiveService.Infrastructure/ArchiveService.Infrastructure.csproj", "ArchiveService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ArchiveService.Api/ArchiveService.Api.csproj"

# Copy all source files
COPY src/ .

# Build the application
RUN dotnet build "ArchiveService.Api/ArchiveService.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "ArchiveService.Api/ArchiveService.Api.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "ArchiveService.Api.dll"]
