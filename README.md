# StageTrack API

![CI](https://github.com/peacedayou/stagetrack-api/actions/workflows/ci.yml/badge.svg)

API **ASP.NET Core** pour suivre ses candidatures de stage : entreprises, postes, statut
(à envoyer → envoyée → entrevue → offre / refusée) et statistiques, avec une petite
interface web incluse.

## Fonctionnalités

- **API REST** avec contrôleurs, DTO et validation (`DataAnnotations`, réponses `ProblemDetails`)
- **Entity Framework Core** avec SQLite : relations un-à-plusieurs, index unique, suppression en cascade, statut enregistré en texte
- **Filtres** par statut et recherche texte (`/api/candidatures?statut=Entrevue&q=dev`)
- **Statistiques** : nombre de candidatures par statut et taux d'entrevue
- **Règle métier** : passer une candidature à « Envoyée » fixe sa date d'envoi
- **Interface web** en HTML/JavaScript qui consomme l'API (`/`)
- **Documentation Swagger** (`/swagger`) et point de santé (`/health`)
- **Tests d'intégration** xUnit avec `WebApplicationFactory` et une base isolée par test
- **Docker** (build multi-étapes) et **intégration continue** GitHub Actions

## Démarrage

Prérequis : [SDK .NET 10](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/StageTrack.Api
# ouvrir l'URL affichée (ex. http://localhost:5000) puis /swagger
dotnet test tests/StageTrack.Api.Tests
```

Avec Docker :

```bash
docker build -t stagetrack-api .
docker run -p 8080:8080 -v stagetrack-data:/data stagetrack-api
```

## Points d'accès

| Méthode | Route | Rôle |
| --- | --- | --- |
| GET / POST | `/api/entreprises` | Lister / créer (409 si le nom existe déjà) |
| GET / DELETE | `/api/entreprises/{id}` | Détail / suppression (et ses candidatures) |
| GET / POST | `/api/candidatures` | Lister (filtres `statut`, `q`) / créer |
| GET / DELETE | `/api/candidatures/{id}` | Détail / suppression |
| PATCH | `/api/candidatures/{id}/statut` | Changer le statut |
| GET | `/api/statistiques` | Totaux par statut et taux d'entrevue |

## Architecture

```
src/StageTrack.Api/
  Models/        entités EF Core (Entreprise, Candidature)
  Data/          AppDbContext et configuration du modèle
  Dtos/          objets d'entrée/sortie validés
  Controllers/   points d'accès REST
  wwwroot/       interface web
tests/StageTrack.Api.Tests/   tests d'intégration
```

## Pistes d'amélioration

Migrations EF Core et SQL Server, authentification JWT, pagination, déploiement sur Azure.

## Technologies

C# · .NET 10 · ASP.NET Core · Entity Framework Core · SQLite · xUnit · Swagger · Docker · GitHub Actions
