# Base image für .NET 8 Apps
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build image für .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Projekt-Dateien kopieren (angepasst an Ihre Struktur)
COPY ["FitnessAnalyticsHub.WebApi/FitnessAnalyticsHub.WebApi.csproj", "FitnessAnalyticsHub.WebApi/"]
COPY ["FitnessAnalyticsHub.Application/FitnessAnalyticsHub.Application.csproj", "FitnessAnalyticsHub.Application/"]
COPY ["FitnessAnalyticsHub.Domain/FitnessAnalyticsHub.Domain.csproj", "FitnessAnalyticsHub.Domain/"]
COPY ["FitnessAnalyticsHub.Infrastructure/FitnessAnalyticsHub.Infrastructure.csproj", "FitnessAnalyticsHub.Infrastructure/"]

# NuGet Pakete wiederherstellen
RUN dotnet restore "FitnessAnalyticsHub.WebApi/FitnessAnalyticsHub.WebApi.csproj"

# Alle Quelldateien kopieren
COPY . .

# Zur WebApi wechseln und bauen
WORKDIR "/src/FitnessAnalyticsHub.WebApi"
RUN dotnet build "FitnessAnalyticsHub.WebApi.csproj" -c Release -o /app/build

# App veröffentlichen
FROM build AS publish
RUN dotnet publish "FitnessAnalyticsHub.WebApi.csproj" -c Release -o /app/publish

# Finales Image erstellen
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FitnessAnalyticsHub.WebApi.dll"]