![Build Status](https://github.com/lady-logic/FitnessAnalyticsHubV1_0/actions/workflows/main.yml/badge.svg)
![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=lady-logic_FitnessAnalyticsHubV1_0&metric=alert_status)
![Coverage](https://sonarcloud.io/api/project_badges/measure?project=lady-logic_FitnessAnalyticsHubV1_0&metric=coverage)
![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=lady-logic_FitnessAnalyticsHubV1_0&metric=sqale_rating)
![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=lady-logic_FitnessAnalyticsHubV1_0&metric=security_rating)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Docker](https://img.shields.io/badge/Docker-Multi--Service-blue)
![API Documentation](https://img.shields.io/badge/API-Swagger%20%2B%20OpenAPI-orange)
![License](https://img.shields.io/github/license/lady-logic/FitnessAnalyticsHubV1_0)
![Last Commit](https://img.shields.io/github/last-commit/lady-logic/FitnessAnalyticsHubV1_0)

<!-- Logo -->
<p align="center">
  <img src="logo.png" alt="FitnessAnalyticsHub Logo" width="200"/>
</p>

<h1 align="center">🏋️‍♀️ FitnessAnalyticsHub</h1>
<p align="center">
  Ein wachsendes Analyse- und Lernprojekt rund um Fitness, Trainingsdaten und moderne .NET-Technologien.
</p>

---

## ✨ Features

- 🏃‍♂️ **Strava Integration** - Automatic activity import and synchronization
- 📊 **Interactive Dashboard** - Comprehensive analytics and visualizations for fitness data
- 📈 **Activity Tracking** - Support for running, cycling, strength training, and more
- 🎯 **Performance Metrics** - Detailed statistics including pace, heart rate, power analysis
- 📱 **Modern Web UI** - Responsive Angular frontend with intuitive navigation
- 🛡️ **Enterprise Error Handling** - Comprehensive exception management with consistent API responses
- 🏥 **Health Monitoring** - Built-in health checks and observability dashboard

---

## 🚀 Projektziele

Dieses Projekt ist eine persönliche Spielwiese für moderne Softwareentwicklung mit Fokus auf:

- 🧱 Clean Architecture mit Domain, Application, Infrastructure
- 🖥️ Moderne UI mit Angular
- 🧪 Architekturtests mit NetArchTest
- 🐳 Dockerisierung & Bereitstellung
- 🤖 Automatisierung durch GitHub Actions
- 🔄 Datenintegration mit externen Services (Strava API)

---

## 🔬 Code Quality & Security

[![Quality gate](https://sonarcloud.io/api/project_badges/quality_gate?project=lady-logic_FitnessAnalyticsHubV1_0)](https://sonarcloud.io/summary/new_code?id=lady-logic_FitnessAnalyticsHubV1_0)

Dieses Projekt verwendet **SonarCloud** für kontinuierliche Code-Qualitätsüberwachung:
- 🛡️ **Security Vulnerabilities** - Automatische Sicherheitsprüfung
- 🐛 **Bug Detection** - Potentielle Fehler werden erkannt
- 📊 **Code Coverage** - Test-Abdeckung wird gemessen
- 🧹 **Code Smells** - Wartbarkeit wird bewertet
- 📈 **Technical Debt** - Refactoring-Bedarf wird geschätzt

[**→ Live SonarCloud Dashboard ansehen**](https://sonarcloud.io/project/overview?id=lady-logic_FitnessAnalyticsHubV1_0)

---

## 🧱 Architekturüberblick

```text
📦 FitnessAnalyticsHub
├── 01_Core
│   └── 🧠 FitnessAnalyticsHub.Domain            // Entitäten, Value Objects, Interfaces
│
├── 02_Application
│   └── 🧰 FitnessAnalyticsHub.Application       // Services, DTOs, CQRS Commands/Queries
│
├── 03_Infrastructure
│   └── 🏗️ FitnessAnalyticsHub.Infrastructure    // Repositories, Strava API, Entity Framework
│
├── 04_UI
│   ├── 🌐 FitnessAnalyticsHub.WebApi            // RESTful API mit Swagger/OpenAPI
│   └── 🌐 UI.Angular                            // Interactive Dashboard & Web-Frontend
│
├── 05_Tests
│   └── 🧪 FitnessAnalyticsHub.Tests             // Unit Tests, Integration Tests, Architecture Tests
│
└── 06_AIAssistant
    └── 🤖 FitnessAnalyticsHub.AIAssistant       // KI-Integration (in Entwicklung)
```

### Key Architecture Features
- **Clean Architecture** mit strikter Dependency Inversion
- **Domain-Driven Design** Prinzipien
- **Entity Framework Core** mit automatischen Migrations
- **Comprehensive Error Handling** mit custom exception hierarchy

---

## 🧪 Test Status

- ✅ **Unit Tests**: Controller und Service Layer mit umfassenden Tests
- 🏛️ **Architecture Tests**: Clean Architecture Compliance mit NetArchTest
- 📊 **Code Coverage**: Automatisch gesammelt und in SonarCloud visualisiert
- 🔄 **Automatische Ausführung**: Bei jedem Commit via GitHub Actions

---

## 🛠️ Technologie Stack & DevOps

**Backend & Framework:**
- 🧠 [.NET 8](https://dotnet.microsoft.com/) (Latest LTS)
- 🔄 [Entity Framework Core](https://docs.microsoft.com/ef/core/) mit SQLite
- 🧱 Clean Architecture Pattern 

**Frontend & UI:**
- 🌐 [Angular](https://angular.io/) mit TypeScript
- 📊 Interactive Charts und Data Visualizations
- 📱 Responsive Design für Desktop und Mobile

**Code Quality & Testing:**
- 🧪 [xUnit](https://xunit.net/) + [FluentAssertions](https://fluentassertions.com/)
- 🏛️ [NetArchTest](https://github.com/BenMorris/NetArchTest) für Architecture Compliance
- 🔬 **SonarCloud Integration** für kontinuierliche Code-Qualität
- 📊 Automated Code Coverage mit detailliertem Reporting

**DevOps & CI/CD:**
- 🤖 **GitHub Actions** - Vollautomatisierte CI/CD Pipeline
- 🛡️ **Branch Protection** mit enforced Code Reviews
- 🐳 **Docker Multi-Stage Builds** für Production Deployments
- 📦 **Health Monitoring** mit `/health-ui` Dashboard

**Integration & APIs:**
- 🔗 [Strava API v3](https://developers.strava.com/) Integration
- 🤖 AI Assistant Module (in Development)
- 📋 Swagger/OpenAPI Documentation

---

## 🔗 Strava API Integration

Umfassende Integration mit der Strava API für:

**Datenimport:**
- 🏃‍♂️ **Activities**: Laufen, Radfahren, Schwimmen, Krafttraining
- 📊 **Performance Metrics**: Pace, Herzfrequenz, Power, Cadence  
- 🗺️ **Route Data**: GPS-Tracks und Elevation profiles
- 👤 **Athlete Profiles**: Benutzerdaten und Präferenzen

**Features:**
- 🔄 **OAuth 2.0**: Sichere Authentifizierung
- 📈 **Data Validation**: Robuste Datenverarbeitung für alle Activity-Typen
- 🚫 **Flexible Schema**: Unterstützt Activities mit/ohne GPS, Pace, etc.

---

## 🏥 Enterprise Features

**Error Handling:** Comprehensive exception hierarchy mit consistent API responses und HTTP status mapping.

**Health Monitoring:** Built-in health checks für Database, Cache und externe APIs mit Visual Dashboard unter `/health-ui`.

**Observability:** Structured logging, performance metrics und automatic health status tracking.

---

## 📋 Getting Started

### Voraussetzungen
- .NET 8.0 SDK
- Node.js (für Angular Frontend)

### Quick Start
```bash
git clone https://github.com/lady-logic/FitnessAnalyticsHubV1_0.git
cd FitnessAnalyticsHubV1_0
dotnet restore && dotnet build

# API starten
cd FitnessAnalyticsHub.WebApi && dotnet run

# Frontend starten (separates Terminal)
cd UI.Angular && npm install && ng serve
```

**Endpoints:**
- API: `https://localhost:7001`
- Dashboard: `http://localhost:4200`
- Health UI: `https://localhost:7001/health-ui`
- Swagger: `https://localhost:7001/swagger`

---

## 🤖 AIAssistant-Modul

Das FitnessAnalyticsHub.AIAssistant-Modul ist derzeit nur rudimentär implementiert und noch nicht mit dem Hauptprojekt verbunden. Zukünftig soll es folgende Funktionen bieten:
- **Trainingsanalyse**: Auswertung von Leistungsdaten
- **Prognosen**: Vorhersage von Leistungsentwicklungen
- **Motivationscoaching**: Personalisierte Trainingstipps

---

## 🎯 Roadmap

**Current Sprint:**
- ✅ Interactive Dashboard Implementation
- ✅ Strava API Integration & Activity Import
- ✅ GitHub CI/CD Workflows
- ✅ Code Quality Monitoring mit SonarCloud

**Next Steps:**
- 🚧 **Enhanced Analytics**: Advanced charts und performance trends
- 🚧 **Data Export**: PDF reports und data export functionality  
- 📝 **Docker Deployment**: Production-ready containerization
- 📝 **Real-time Updates**: WebSocket integration für live dashboard updates
- 📝 **AIAssistant Integration**: Intelligent training recommendations
- 📝 **Multi-Sport Analytics**: Sport-specific metrics und insights

---

## 📄 License

Dieses Projekt steht unter der MIT License - siehe [LICENSE](LICENSE) Datei für Details.
