# Blend

**Blend** is a web application for home cooks that makes it easy to discover recipes, get creative in the kitchen, and connect with a community of food lovers.

## Overview

Blend brings together three core pillars to enhance the home cooking experience:

- **Recipe Discovery** — Search millions of recipes by ingredient, cuisine, dietary preference, or cooking time. Powered by the Spoonacular API and personalised to your taste profile.
- **Creative Cooking Mode** — Step-by-step guided cook mode with timers, substitution suggestions, and AI-assisted tips to help you adapt any recipe on the fly.
- **Community & Content** — Share your culinary creations, follow other home cooks, and discover trending recipes from people who cook like you.

## Key Features

| Feature | Description |
|---|---|
| Recipe Search & Discovery | Ingredient-based and free-text search with filtering by diet, cuisine, and time |
| Personalised Recommendations | Taste profile and preference-driven recipe suggestions |
| Cook Mode | Guided step-by-step cooking with timers and ingredient substitution |
| User Profiles | Personal recipe collections, favourites, and cooking history |
| Social Feed | Follow friends, share recipes, and discover community content |
| Admin Content Management | Curated editorial content and featured recipe collections |

## Architecture at a Glance

Blend is built on a modern cloud-native stack:

| Layer | Technology |
|---|---|
| Frontend | Next.js (React) — Static Web App |
| Backend | ASP.NET Core .NET 9 Web API |
| Database | Azure Cosmos DB (NoSQL) |
| Authentication | ASP.NET Core Identity + JWT |
| Deployment | Azure Container Apps + Azure Static Web Apps |

See the [Architecture Overview](architecture/overview.md) for full details.

## Quick Navigation

- [Installation](getting-started/installation.md) — Set up your local development environment
- [Quick Start](getting-started/quick-start.md) — Get the app running in minutes
- [Architecture Overview](architecture/overview.md) — Understand how the components fit together
- [REST API](api/rest-api.md) — API reference documentation
- [Development Guide](guides/development.md) — Developer workflow and coding standards

## Project Status

Blend is actively developed. See the [GitHub repository](https://github.com/andrew-rubio/blend-app) for the latest updates, issues, and releases.
