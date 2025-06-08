![Build Status](https://github.com/lady-logic/FitnessAnalyticsHubV1_0/actions/workflows/main.yml/badge.svg)
![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=lady-logic_FitnessAnalyticsHubV1_0&metric=alert_status)
![Coverage](https://sonarcloud.io/api/project_badges/measure?project=lady-logic_FitnessAnalyticsHubV1_0&metric=coverage)
![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=lady-logic_FitnessAnalyticsHubV1_0&metric=sqale_rating)
![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=lady-logic_FitnessAnalyticsHubV1_0&metric=security_rating)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Health Monitoring](https://img.shields.io/badge/Health-Monitoring%20%2B%20UI-brightgreen)
![Docker](https://img.shields.io/badge/Docker-Multi--Service-blue)
![API Documentation](https://img.shields.io/badge/API-Swagger%20%2B%20OpenAPI-orange)
![Microservices](https://img.shields.io/badge/Architecture-Microservices-green)
![License](https://img.shields.io/github/license/lady-logic/FitnessAnalyticsHubV1_0)
![Last Commit](https://img.shields.io/github/last-commit/lady-logic/FitnessAnalyticsHubV1_0)

<!-- Logo -->
<p align="center">
  <img src="logo.png" alt="FitnessAnalyticsHub Logo" width="200"/>
</p>

<h1 align="center">🏋️‍♀️ FitnessAnalyticsHubV1_0</h1>
<p align="center">
  Ein wachsendes Analyse- und Lernprojekt rund um Fitness, Trainingsdaten und moderne .NET-Technologien.
</p>

---

## 🚀 Projektziele

Dieses Projekt ist eine persönliche Spielwiese für moderne Softwareentwicklung mit Fokus auf:

- 🧱 Clean Architecture mit Domain, Application, Infrastructure
- 🖥️ Moderne UI mit Angular & evtl. auch noch WPF
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

## 🏥 Health Monitoring & Observability

### **📊 Health Monitoring Features**
```csharp
// Umfassendes Health Monitoring System
services.AddHealthChecks()
    .AddCheck("api", () => HealthCheckResult.Healthy())
    .AddSqlServer(connectionString, tags: new[] { "database" })
    .AddRedis(redisConnection, tags: new[] { "cache" });

// Health Dashboard mit 60s Auto-Refresh
services.AddHealthChecksUI(setup => {
    setup.SetEvaluationTimeInSeconds(60);
    setup.MaximumHistoryEntriesPerEndpoint(50);
});
```

### **🌐 Health Endpoints & Dashboard**
Nach dem Starten des Systems sind folgende Monitoring-Endpoints verfügbar:

| Endpoint | Beschreibung | Beispiel |
|----------|--------------|----------|
| **`/health-ui`** | 📊 **Visual Dashboard** mit Verlauf | `http://localhost:8080/health-ui` |
| **`/health`** | 🔍 **JSON API** für alle Services | `http://localhost:8080/health` |
| **`/health/infrastructure`** | 🏗️ **Gruppierte Checks** (DB, Cache) | `http://localhost:8080/health/infrastructure` |

### **✨ Enterprise Health Features**
- **🏷️ Tag-based Grouping** - Services vs Infrastructure
- **📈 Historical Tracking** - 50 Health Check Einträge Verlauf
- **⏱️ Auto-Refresh** - Alle 60 Sekunden automatische Prüfung  
- **🎯 Production Ready** - Geeignet für Load Balancer Integration
- **🔄 Container Health** - Docker HEALTHCHECK Integration

```bash
# Health Status prüfen
curl http://localhost:8080/health | jq

# Health Dashboard öffnen  
open http://localhost:8080/health-ui
```

---

## 🧱 Architekturüberblick

```text
📦 FitnessAnalyticsHubV1_0
├── 01_Core
│   └── 🧠 FitnessAnalyticsHub.Domain            // Entitäten, Value Objects, Interfaces
│
├── 02_Application
│   └── 🧰 FitnessAnalyticsHub.Application       // Services, DTOs, Interfaces
│
├── 03_Infrastructure
│   └── 🏗️ FitnessAnalyticsHub.Infrastructure    // Repositories, externe APIs, Persistence
│
├── 04_UI
│   ├── 🌐 FitnessAnalyticsHub.WebApi            // RESTful API für Clients
│   └── 🌐 UI.Angular                            // Web-Frontend (erste Oberfläche für Athlet 😀)
│
├── 05_Tests
│   └── 🧪 FitnessAnalyticsHub.Tests             // Architekturtests und Unit-Tests
│
└── 06_AIAssistant
    └── 🤖 FitnessAnalyticsHub.AIAssistant       // KI-Integration (rudimentäre Implementierung)
```

---

## 🏗️ Recent Architecture Improvements

### ✅ Entity Configuration Organization
- **Before**: All entity configurations mixed in `ApplicationDbContext.OnModelCreating()`
- **After**: Separated into dedicated `IEntityTypeConfiguration<T>` classes
- **Benefit**: Better organization, easier maintenance, follows separation of concerns

### ✅ Database Configuration Consolidation
- **Before**: Database setup duplicated across multiple locations
- **After**: Centralized `DatabaseConfiguration` helper class
- **Benefit**: Single source of truth, eliminates inconsistencies

### ✅ AutoMapper Integration
- **Before**: Manual property mapping scattered throughout codebase
- **After**: Centralized mapping profiles with business logic
- **Benefit**: Reduced boilerplate, consistent mapping behavior

### ✅ Async Operations Enhancement
- **Before**: Limited cancellation token support
- **After**: Full cancellation token support throughout all layers
- **Benefit**: Better resource management and application responsiveness

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
- 🌐 [Angular](https://angular.io/) für moderne Web-UI

**Code Quality & Testing:**
- 🧪 [xUnit Testing Framework](https://xunit.net/) mit umfassenden Unit Tests
- 🧪 [FluentAssertions](https://fluentassertions.com/) für aussagekräftige Test-Assertions
- 🏛️ [NetArchTest](https://github.com/BenMorris/NetArchTest) für Architecture Tests
- 🔬 **SonarCloud Integration** für kontinuierliche Code-Qualitätsüberwachung
- 📊 Code Coverage Tracking und Reporting

**DevOps & CI/CD:**
- 🤖 **GitHub Actions** - Vollautomatisierte CI/CD Pipeline
- ✅ Automatisierte Tests bei jedem Commit
- 🛡️ **Branch Protection Rules** - Code Review Workflow enforced
- 🔄 **Multi-Stage Pipeline**: Build → Test → Code Quality Analysis
- 📦 Artifact Management für Test-Reports und Coverage-Daten

**Integration & APIs:**
- 🔗 [Strava API](https://developers.strava.com/) für Fitness-Datenintegration
- 🤖 AI Assistant Integration (in Entwicklung)

**Entwicklungsstandards:**
- 📋 Git Workflow mit Feature Branches
- 💬 Semantic Commit Messages
- 📖 Comprehensive Documentation
- 🏆 Professional README mit Quality Badges

---

## 📋 Getting Started

### Voraussetzungen
- .NET 8.0 SDK
- Visual Studio 2022 oder VS Code

### Installation
```bash
git clone https://github.com/lady-logic/FitnessAnalyticsHubV1_0.git
cd FitnessAnalyticsHubV1_0
dotnet restore
dotnet build
```

### Tests ausführen
```bash
# Alle Tests
dotnet test

# Nur Unit Tests
dotnet test --filter "Category!=Architecture"

# Nur Architecture Tests
dotnet test --filter "Category=Architecture"

# Mit Coverage
dotnet test --collect:"XPlat Code Coverage"
```

### API starten
```bash
cd FitnessAnalyticsHub.WebApi
dotnet run
```

Die API ist dann verfügbar unter: `https://localhost:7001`

---

## 🔗 Strava API Integration

Die Integration mit der Strava API ermöglicht den Zugriff auf:
- Aktivitätsdaten (Laufen, Radfahren, etc.)
- Leistungsmetriken
- Strecken und Routen
- Benutzerprofildaten

---

## 🤖 AIAssistant-Modul

Das FitnessAnalyticsHub.AIAssistant-Modul ist derzeit nur rudimentär implementiert und noch nicht mit dem Hauptprojekt verbunden. Zukünftig soll es folgende Funktionen bieten:
- **Trainingsanalyse**: Auswertung von Leistungsdaten
- **Prognosen**: Vorhersage von Leistungsentwicklungen
- **Motivationscoaching**: Personalisierte Trainingstipps

---

## 🎯 Roadmap

- ✅ GitHub CI/CD Workflows integrieren
- ✅ Code Quality Monitoring mit SonarCloud
- ✅ Tests + Testabdeckung ausbauen
- 🚧 Trainingsdaten via Strava API laden
- 📝 Dockerisieren für lokale + Cloud-Deployments
- 📝 CQRS-Pattern implementieren für bessere Trennung von Lese- und Schreiboperationen
- 📝 Fehlende Oberflächen in Angular ergänzen
- 📝 AIAssistant anbinden...uvm 😅

---

## 📄 License

Dieses Projekt steht unter der MIT License - siehe [LICENSE](LICENSE) Datei für Details.
