# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/WeightDataService.Api/WeightDataService.Api.csproj", "src/WeightDataService.Api/"]
COPY ["src/WeightDataService.Core/WeightDataService.Core.csproj", "src/WeightDataService.Core/"]
COPY ["src/WeightDataService.Infrastructure/WeightDataService.Infrastructure.csproj", "src/WeightDataService.Infrastructure/"]
RUN dotnet restore "src/WeightDataService.Api/WeightDataService.Api.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/src/WeightDataService.Api"
RUN dotnet build "WeightDataService.Api.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "WeightDataService.Api.csproj" -c Release -o /app/publish

# Use the official .NET runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy the published application
COPY --from=build /app/publish .

# Create directories for logs and data
RUN mkdir -p /app/logs /app/data && \
    chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose the port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/api/health || exit 1

# Set the entry point
ENTRYPOINT ["dotnet", "WeightDataService.Api.dll"]