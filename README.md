![Build Status](https://github.com/lady-logic/FitnessAnalyticsHubV1_0/actions/workflows/main.yml/badge.svg)
![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=lady-logic_FitnessAnalyticsHubV1_0&metric=alert_status)
![Coverage](https://sonarcloud.io/api/project_badges/measure?project=lady-logic_FitnessAnalyticsHub_0&metric=coverage)
![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=lady-logic_FitnessAnalyticsHubV1_0&metric=sqale_rating)
![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=lady-logic_FitnessAnalyticsHubV1_0&metric=security_rating)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Health Monitoring](https://img.shields.io/badge/Health-Monitoring%20%2B%20UI-brightgreen)
![Docker](https://img.shields.io/badge/Docker-Multi--Service-blue)
![API Documentation](https://img.shields.io/badge/API-Swagger%20%2B%20OpenAPI-orange)
![Microservices](https://img.shields.io/badge/Architecture-Microservices-green)
![AI Integration](https://img.shields.io/badge/AI-HuggingFace%20%2B%20Llama-orange)
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
- 🧠 **KI-Integration** mit HuggingFace für intelligente Trainingsanalyse

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

## 🤖 AI-Powered Fitness Analytics

### **🧠 HuggingFace Integration**
Das Projekt integriert moderne KI-Technologien für intelligente Trainingsanalyse:

- **🔥 Meta-Llama-3.1-8B-Instruct** - Hochmodernes Sprachmodell für Fitnessanalyse
- **📊 Intelligente Workout-Analyse** - KI-basierte Trend- und Leistungsanalyse
- **💪 AI Motivation Coach** - Personalisierte, kontextbezogene Trainingsmotivation
- **🎯 Smarte Empfehlungen** - Datengestützte Trainingsoptimierung
- **🛡️ Fallback-System** - Robuste Fehlerbehandlung bei API-Limits

### **🔄 Microservice-Architektur**
```bash
# AI-Service verfügbar auf:
http://localhost:5169/api/WorkoutAnalysis/analyze/huggingface

# Health Check:
http://localhost:5169/api/WorkoutAnalysis/health

# Swagger UI:
http://localhost:5169/swagger
```

### **📈 AI Features im Detail**
- **Trend-Analyse**: Erkennung von Trainingsmustern über Zeit
- **Performance-Insights**: Intelligente Leistungsbewertung
- **Gesundheitsmetriken**: KI-gestützte Verletzungsprävention
- **Motivations-Content**: Dynamische, personalisierte Ermutigung

---

## 🛡️ Error Handling

This application implements a comprehensive error handling strategy using Clean Architecture principles.

### Exception Hierarchy

```
Exception
├── DomainException (Base for all domain exceptions)
│   ├── NotFoundException
│   │   ├── ActivityNotFoundException
│   │   └── AthleteNotFoundException
│   ├── ValidationException
│   └── BusinessRuleException
└── StravaServiceException (Infrastructure exceptions)
    ├── InvalidStravaTokenException
    ├── StravaApiException
    ├── StravaConfigurationException
    └── StravaAuthorizationException
```

### Error Response Format

All API errors return a consistent JSON structure:

```json
{
  "type": "ActivityNotFound",
  "message": "Activity with ID 123 not found",
  "statusCode": 404,
  "details": "ActivityId: 123",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### HTTP Status Code Mapping

| Exception Type | HTTP Status | Description |
|----------------|-------------|-------------|
| `ActivityNotFoundException` | 404 | Activity not found |
| `AthleteNotFoundException` | 404 | Athlete not found |
| `InvalidStravaTokenException` | 401 | Invalid or expired token |
| `StravaConfigurationException` | 500 | Server configuration error |
| `StravaApiException` | 400/502 | External API error |
| Generic exceptions | 500 | Internal server error |

### For Developers

**Controllers are exception-free:**
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<ActivityDto>> GetById(int id)
{
    var activity = await _activityService.GetActivityByIdAsync(id);
    return Ok(activity); // Exceptions handled by middleware
}
```

**Services throw specific exceptions:**
```csharp
public async Task<ActivityDto> GetActivityByIdAsync(int id)
{
    var activity = await _repository.GetByIdAsync(id);
    if (activity == null)
        throw new ActivityNotFoundException(id);
    return _mapper.Map<ActivityDto>(activity);
}
```

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
    └── 🤖 FitnessAnalyticsHub.AIAssistant       // KI-Integration mit HuggingFace
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

### ✅ AI Integration with HuggingFace
- **New**: Complete AI microservice architecture for workout analysis
- **Features**: Intelligent trend analysis, motivation coaching, health insights
- **Technology**: Meta-Llama-3.1-8B-Instruct model via HuggingFace Inference API
- **Benefit**: Personalized, data-driven fitness recommendations and motivation

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

**AI & Machine Learning:**
- 🤖 [HuggingFace Inference API](https://huggingface.co/inference-api) für KI-Integration
- 🧠 **Meta-Llama-3.1-8B-Instruct** für natürliche Sprachverarbeitung
- 🔄 **Microservice-Architektur** für AI-Services
- 🛡️ **Fallback-Mechanismen** für robuste AI-Integration

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
- 🤖 **AI Assistant Integration** mit HuggingFace

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
# Haupt-API
cd FitnessAnalyticsHub.WebApi
dotnet run

# AI-Microservice
cd FitnessAnalyticsHub.AIAssistant
dotnet run
```

Die Haupt-API ist verfügbar unter: `https://localhost:7001`
Der AI-Service ist verfügbar unter: `http://localhost:5169`

---

## 🔗 Strava API Integration

Die Integration mit der Strava API ermöglicht den Zugriff auf:
- Aktivitätsdaten (Laufen, Radfahren, etc.)
- Leistungsmetriken
- Strecken und Routen
- Benutzerprofildaten

---

## 🤖 AIAssistant-Modul

Das FitnessAnalyticsHub.AIAssistant-Modul ist **vollständig integriert** und bietet:
- **🔥 Trainingsanalyse**: KI-basierte Auswertung von Leistungsdaten mit Meta-Llama-3.1-8B
- **📈 Intelligente Prognosen**: Trend-Analyse und Leistungsentwicklung
- **💪 Motivationscoaching**: Personalisierte, kontextbezogene Trainingstipps
- **🎯 Smarte Empfehlungen**: Datengestützte Trainingsoptimierung
- **🛡️ Robuste Integration**: Fallback-Mechanismen für zuverlässige Funktion

### **Verfügbare AI-Endpoints:**
```bash
# Workout-Analyse mit KI
POST http://localhost:5169/api/WorkoutAnalysis/analyze/huggingface

# Performance-Trends
GET http://localhost:5169/api/WorkoutAnalysis/performance-trends/{athleteId}

# Training-Empfehlungen  
GET http://localhost:5169/api/WorkoutAnalysis/recommendations/{athleteId}

# Gesundheitsanalyse
POST http://localhost:5169/api/WorkoutAnalysis/health-analysis

# Service Health Check
GET http://localhost:5169/api/WorkoutAnalysis/health
```

---

## 🎯 Roadmap

- ✅ GitHub CI/CD Workflows integrieren
- ✅ Code Quality Monitoring mit SonarCloud
- ✅ Tests + Testabdeckung ausbauen
- ✅ **KI-Integration mit HuggingFace für intelligente Trainingsanalyse**
- 🚧 Trainingsdaten via Strava API laden
- 📝 Dockerisieren für lokale + Cloud-Deployments
- 📝 CQRS-Pattern implementieren für bessere Trennung von Lese- und Schreiboperationen
- 📝 Fehlende Oberflächen in Angular ergänzen
- 📝 AI-Features in Frontend integrieren...uvm 😅

---

## 📄 License

Dieses Projekt steht unter der MIT License - siehe [LICENSE](LICENSE) Datei für Details.
