# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install wget for health checks
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/ReportService.Api/ReportService.Api.csproj", "ReportService.Api/"]
COPY ["src/ReportService.Core/ReportService.Core.csproj", "ReportService.Core/"]
COPY ["src/ReportService.Infrastructure/ReportService.Infrastructure.csproj", "ReportService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ReportService.Api/ReportService.Api.csproj"

# Copy all source files
COPY src/ .

# Build the application
RUN dotnet build "ReportService.Api/ReportService.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "ReportService.Api/ReportService.Api.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "ReportService.Api.dll"]