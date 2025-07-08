FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["UserModule/UserModule.csproj", "./"]
RUN dotnet restore "./UserModule.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "UserModule/UserModule.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserModule/UserModule.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "UserModule.dll"]