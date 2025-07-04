# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/SaccoService.Api/SaccoService.Api.csproj", "SaccoService.Api/"]
COPY ["src/SaccoService.Core/SaccoService.Core.csproj", "SaccoService.Core/"]
COPY ["src/SaccoService.Infrastructure/SaccoService.Infrastructure.csproj", "SaccoService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "SaccoService.Api/SaccoService.Api.csproj"

# Copy all source files
COPY src/ .

# Build the application
RUN dotnet build "SaccoService.Api/SaccoService.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "SaccoService.Api/SaccoService.Api.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "SaccoService.Api.dll"]
