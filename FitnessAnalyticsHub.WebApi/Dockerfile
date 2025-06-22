# Base image für .NET 8 Apps
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build image für .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Zentrale Build-Konfiguration kopieren
COPY ["Directory.Build.props", "Directory.Packages.props", "./"]

# Projekt-Dateien kopieren (angepasst an Ihre Struktur)
COPY ["FitnessAnalyticsHub.WebApi/FitnessAnalyticsHub.WebApi.csproj", "FitnessAnalyticsHub.WebApi/"]
COPY ["FitnessAnalyticsHub.Application/FitnessAnalyticsHub.Application.csproj", "FitnessAnalyticsHub.Application/"]
COPY ["FitnessAnalyticsHub.Domain/FitnessAnalyticsHub.Domain.csproj", "FitnessAnalyticsHub.Domain/"]
COPY ["FitnessAnalyticsHub.Infrastructure/FitnessAnalyticsHub.Infrastructure.csproj", "FitnessAnalyticsHub.Infrastructure/"]

# NuGet Pakete wiederherstellen
RUN dotnet restore "FitnessAnalyticsHub.WebApi/FitnessAnalyticsHub.WebApi.csproj"

# Copy only source code directories
COPY ["FitnessAnalyticsHub.WebApi/", "FitnessAnalyticsHub.WebApi/"]
COPY ["FitnessAnalyticsHub.Application/", "FitnessAnalyticsHub.Application/"]
COPY ["FitnessAnalyticsHub.Domain/", "FitnessAnalyticsHub.Domain/"]
COPY ["FitnessAnalyticsHub.Infrastructure/", "FitnessAnalyticsHub.Infrastructure/"]

# Zur WebApi wechseln und bauen
WORKDIR "/src/FitnessAnalyticsHub.WebApi"
RUN dotnet build "FitnessAnalyticsHub.WebApi.csproj" -c Release -o /app/build

# App veröffentlichen
FROM build AS publish
RUN dotnet publish "FitnessAnalyticsHub.WebApi.csproj" -c Release -o /app/publish

# Finales Image erstellen
FROM base AS final
WORKDIR /app

# Files kopieren (als root)
COPY --from=publish /app/publish .

# Umgebungsvariablen für Strava-Konfiguration (werden zur Laufzeit überschrieben)
ENV Strava__ClientId=""
ENV Strava__ClientSecret=""
ENV Strava__RedirectUrl="http://localhost:8080/auth/strava/callback"
ENV Strava__BaseUrl="https://www.strava.com/api/v3/"
ENV Strava__AuthorizeUrl="https://www.strava.com/oauth/authorize"
ENV Strava__TokenUrl="https://www.strava.com/oauth/token"

# Standard ASP.NET Core Umgebungsvariablen
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Explizit non-root user verwenden
USER 1001

ENTRYPOINT ["dotnet", "FitnessAnalyticsHub.WebApi.dll"]