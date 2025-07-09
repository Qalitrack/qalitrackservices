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
COPY ["src/TestServiceV4.Api/TestServiceV4.Api.csproj", "TestServiceV4.Api/"]
COPY ["src/TestServiceV4.Core/TestServiceV4.Core.csproj", "TestServiceV4.Core/"]
COPY ["src/TestServiceV4.Infrastructure/TestServiceV4.Infrastructure.csproj", "TestServiceV4.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "TestServiceV4.Api/TestServiceV4.Api.csproj"

# Copy all source files
COPY src/ .

# Build the application
RUN dotnet build "TestServiceV4.Api/TestServiceV4.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "TestServiceV4.Api/TestServiceV4.Api.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "TestServiceV4.Api.dll"]