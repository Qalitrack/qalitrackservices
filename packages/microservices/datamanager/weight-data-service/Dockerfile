# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/WeightDataService.Api/WeightDataService.Api.csproj", "WeightDataService.Api/"]
COPY ["src/WeightDataService.Core/WeightDataService.Core.csproj", "WeightDataService.Core/"]
COPY ["src/WeightDataService.Infrastructure/WeightDataService.Infrastructure.csproj", "WeightDataService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "WeightDataService.Api/WeightDataService.Api.csproj"

# Copy all source files
COPY src/ .

# Build the application
RUN dotnet build "WeightDataService.Api/WeightDataService.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "WeightDataService.Api/WeightDataService.Api.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "WeightDataService.Api.dll"]
