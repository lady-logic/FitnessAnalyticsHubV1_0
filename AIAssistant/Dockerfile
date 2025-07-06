# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Zentrale Build-Konfiguration kopieren
COPY ["Directory.Build.props", "Directory.Packages.props", "./"]

COPY ["AIAssistant/FitnessAnalyticsHub.AIAssistant.csproj", "AIAssistant/"]
RUN dotnet restore "./AIAssistant/FitnessAnalyticsHub.AIAssistant.csproj"
COPY ["AIAssistant/", "AIAssistant/"]
COPY ["FitnessAnalyticsHub.Domain/", "FitnessAnalyticsHub.Domain/"]
COPY ["FitnessAnalyticsHub.Application/", "FitnessAnalyticsHub.Application/"]
COPY ["FitnessAnalyticsHub.Infrastructure/", "FitnessAnalyticsHub.Infrastructure/"]
WORKDIR "/src/AIAssistant"
RUN dotnet build "./FitnessAnalyticsHub.AIAssistant.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FitnessAnalyticsHub.AIAssistant.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app

# Files kopieren (als root)
COPY --from=publish /app/publish .

# Explizit non-root user verwenden
USER 1001

ENTRYPOINT ["dotnet", "FitnessAnalyticsHub.AIAssistant.dll"]