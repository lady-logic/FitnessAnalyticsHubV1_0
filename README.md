![Build Status](https://github.com/lady-logic/FitnessAnalyticsHubV1_0/actions/workflows/main.yml/badge.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
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
│   ├── 🖼️ FitnessAnalyticsHub.UI.WPF            // Desktop-Client mit MVVM (enthält noch einige offene Baustellen...)
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

## 🧪 Aktueller Fortschritt

| Thema | Status | Beschreibung |
|---|---|---|
| Clean Architecture | ✅ Basis steht | Projektstruktur aufgebaut |
| WPF UI | 🚧 In Arbeit | Desktop-Anwendung mit Charts |
| Angular UI | 🚧 In Arbeit | Web-Oberfläche mit responsivem Design |
| Strava API | 🚧 In Arbeit | Abruf von Trainingsdaten |
| Unit Tests | 🚧 In Arbeit | xUnit + FluentAssertions |
| Architekturtests | 🚧 In Arbeit | NetArchTest für Strukturvalidierung |
| GitHub Actions | 🚧 In Arbeit | CI/CD Workflow mit Build + Test |
| Docker | 📝 ToDo | Deployment-Vorbereitung |
| CQRS | 📝 ToDo | Implementierung von Command/Query Separation |

---

## 🧪 Test Status

- ✅ **Unit Tests**: Controller und Service Layer
- 🏛️ **Architecture Tests**: Clean Architecture Compliance
- 📊 **Code Coverage**: Automatisch gesammelt

---

## 💡 Verwendete Technologien

- 🧠 [.NET 8](https://dotnet.microsoft.com/)
- 🖼️ [WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
- 🌐 [Angular](https://angular.io/)
- 🧪 [xUnit](https://xunit.net/) + [FluentAssertions](https://fluentassertions.com/)
- 🧪 [NetArchTest](https://github.com/BenMorris/NetArchTest)
- 🔄 [EF Core](https://docs.microsoft.com/ef/core/) mit SQLite
- 🐳 [Docker](https://www.docker.com/)
- ⚙️ [GitHub Actions](https://github.com/features/actions)
- 🔗 [Strava API](https://developers.strava.com/)

---

## Strava API Integration

Die Integration mit der Strava API ermöglicht den Zugriff auf:
- Aktivitätsdaten (Laufen, Radfahren, etc.)
- Leistungsmetriken
- Strecken und Routen
- Benutzerprofildaten

## AIAssistant-Modul

Das FitnessAnalyticsHub.AIAssistant-Modul ist derzeit nur rudimentär implementiert und noch nicht mit dem Hauptprojekt verbunden. Zukünftig soll es folgende Funktionen bieten:
- **Trainingsanalyse**: Auswertung von Leistungsdaten
- **Prognosen**: Vorhersage von Leistungsentwicklungen
- **Motivationscoaching**: Personalisierte Trainingstipps

---

## 🎯 Roadmap

- Trainingsdaten via Strava API laden
- GitHub CI/CD Workflows integrieren
- Dockerisieren für lokale + Cloud-Deployments
- Tests + Testabdeckung ausbauen
- CQRS-Pattern implementieren für bessere Trennung von Lese- und Schreiboperationen
- Fehlende Oberflächen in Angular ergänzen
- AIAssistant anbinden...uvm 😅
