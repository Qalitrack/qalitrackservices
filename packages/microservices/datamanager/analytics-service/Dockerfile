# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/AnalyticsService.Api/AnalyticsService.Api.csproj", "AnalyticsService.Api/"]
COPY ["src/AnalyticsService.Core/AnalyticsService.Core.csproj", "AnalyticsService.Core/"]
COPY ["src/AnalyticsService.Infrastructure/AnalyticsService.Infrastructure.csproj", "AnalyticsService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "AnalyticsService.Api/AnalyticsService.Api.csproj"

# Copy all source files
COPY src/ .

# Build the application
RUN dotnet build "AnalyticsService.Api/AnalyticsService.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "AnalyticsService.Api/AnalyticsService.Api.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "AnalyticsService.Api.dll"]
