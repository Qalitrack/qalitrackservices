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
COPY ["src/Transaction.Api/Transaction.Api.csproj", "Transaction.Api/"]
COPY ["src/Transaction.Core/Transaction.Core.csproj", "Transaction.Core/"]
COPY ["src/Transaction.Infrastructure/Transaction.Infrastructure.csproj", "Transaction.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Transaction.Api/Transaction.Api.csproj"

# Copy all source files
COPY src/ .

# Build the application
RUN dotnet build "Transaction.Api/Transaction.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Transaction.Api/Transaction.Api.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "Transaction.Api.dll"]