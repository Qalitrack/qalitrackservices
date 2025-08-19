# Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# SDK build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1️⃣ Copy Messaging.Contracts fully & pack
COPY ./Messaging.Contracts/ Messaging.Contracts/
RUN dotnet pack "Messaging.Contracts/Messaging.Contracts.csproj" -c Release -o /src/nuget

# 2️⃣ Add local NuGet source
RUN dotnet nuget add source "/src/nuget" --name LocalContracts

# 3️⃣ Copy all other project files
COPY ./BackupService.API/ BackupService.API/
COPY ./BackupService.Core/ BackupService.Core/
COPY ./BackupService.Infrastructure/ BackupService.Infrastructure/

# 4️⃣ Restore all dependencies
RUN dotnet restore "BackupService.API/BackupService.API.csproj"

# 5️⃣ Build & publish
RUN dotnet build "BackupService.API/BackupService.API.csproj" -c Release -o /app/build
RUN dotnet publish "BackupService.API/BackupService.API.csproj" -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "BackupService.API.dll"]
