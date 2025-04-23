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

Dieses Projekt ist ein persönlicher Lern-Hub für moderne Softwareentwicklung mit Fokus auf:

- 🧱 Clean Architecture mit Domain, Application, Infrastructure, WPF-UI
- 🧪 Testautomatisierung & Qualitätssicherung
- 🐳 Dockerisierung & Bereitstellung
- 🤖 Automatisierung durch GitHub Actions
- 🔍 Trainingsdatenanalyse via ML.NET

---

## 🧱 Architekturüberblick

```text
📦 FitnessAnalyticsHubV1_0
├── 🧠 Domain          // Entitäten, Interfaces, Business-Logik
├── 🧰 Application     // UseCases, Services, DTOs
├── 🏗️ Infrastructure // Datenzugriffe, externe APIs (z.B. Strava), Logging
├── 🖼️ UI.WPF         // Oberfläche mit Charts & Prognosen
└── 🧪 Tests           // Unit & Integration Tests
```
---

## 🧪 Aktueller Fortschritt

| Thema             | Status       | Beschreibung                                  |
|------------------|--------------|----------------------------------------------|
| WPF UI           | 🚧 In Arbeit | Erste Views & Charts                          |
| Clean Architecture | ✅ Basis steht | Projektstruktur aufgebaut                     |
| ML.NET           | 🚧 Geplant   | Trainingsanalyse & Prognose                   |
| Strava API       | 🚧 In Arbeit | Abruf von Trainingsdaten                      |
| Unit Tests       | 📝 ToDo      | Basis schaffen mit xUnit                      |
| GitHub Actions   | 📝 ToDo      | CI/CD Workflow mit Build + Test               |
| Docker           | 📝 ToDo      | Deployment-Vorbereitung                       |

---

## 💡 Verwendete Technologien

- 🧠 [.NET 8](https://dotnet.microsoft.com/)
- 🖼️ [WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
- 🔍 [ML.NET](https://dotnet.microsoft.com/apps/machinelearning-ai/ml-dotnet)
- 🧪 [xUnit](https://xunit.net/) + [FluentAssertions](https://fluentassertions.com/)
- 🐳 [Docker](https://www.docker.com/)
- ⚙️ [GitHub Actions](https://github.com/features/actions)
- 🔗 [Strava API](https://developers.strava.com/)

---

## 🎯 Roadmap

- [ ] Trainingsdaten via Strava API laden
- [ ] GitHub CI/CD Workflows integrieren
- [ ] Dockerisieren für lokale + Cloud-Deployments
- [ ] Tests + Testabdeckung einführen
- [ ] ML.NET Modell trainieren und evaluieren
- [ ] Prognose-Chart mit WPF darstellen
