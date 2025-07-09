FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MockUserService.csproj", "."]
RUN dotnet restore "./MockUserService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "MockUserService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MockUserService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "MockUserService.dll"]