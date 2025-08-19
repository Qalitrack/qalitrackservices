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

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1️⃣ Copy Messaging.Contracts from BackupService and pack
COPY ./Messaging.Contracts/ Messaging.Contracts/
RUN dotnet pack "Messaging.Contracts/Messaging.Contracts.csproj" -c Release -o /src/nuget

# 2️⃣ Add local NuGet source
RUN dotnet nuget add source "/src/nuget" --name LocalContracts

# 3️⃣ Copy UserService project files
COPY ["src/UserService.Api/UserService.Api.csproj", "UserService.Api/"]
COPY ["src/UserService.Core/UserService.Core.csproj", "UserService.Core/"]
COPY ["src/UserService.Infrastructure/UserService.Infrastructure.csproj", "UserService.Infrastructure/"]

# 4️⃣ Restore dependencies (local NuGet is now available)
RUN dotnet restore "UserService.Api/UserService.Api.csproj"

# 5️⃣ Copy all source files
COPY src/ .

# 6️⃣ Build the application
RUN dotnet build "UserService.Api/UserService.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "UserService.Api/UserService.Api.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "UserService.Api.dll"]