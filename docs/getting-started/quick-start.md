# Quick Start

Get the Blend application up and running locally in about 5 minutes. This guide assumes you have completed the [Installation](installation.md) steps.

## Before You Begin

Ensure you have:

- Cloned the repository and installed all dependencies (see [Installation](installation.md))
- The backend and frontend running locally
- A Spoonacular API key (free tier available at [spoonacular.com/food-api](https://spoonacular.com/food-api))

## Step 1: Start the Backend

From the repository root, start the .NET Aspire AppHost:

```bash
dotnet run --project src/backend/Blend.AppHost
```

This starts the API at `https://localhost:7000` and the Aspire dashboard at `https://localhost:15888`.

## Step 2: Start the Frontend

In a separate terminal:

```bash
cd src/Blend.Web
npm run dev
```

The frontend is available at `http://localhost:3000`.

## Step 3: Create an Account

1. Open `http://localhost:3000` in your browser
2. Click **Sign Up**
3. Enter your name, email, and password
4. You are now logged in and can explore the app

## Step 4: Discover Recipes

1. Use the search bar to search for a recipe (e.g. "pasta carbonara")
2. Filter results by dietary preference, cuisine, or cooking time
3. Click a recipe to view the full details

## Step 5: Try Cook Mode

1. Open any recipe detail page
2. Click **Start Cooking**
3. Follow the step-by-step guided cook mode

## Step 6: Set Up Your Taste Profile

1. Navigate to your profile page
2. Click **Edit Preferences**
3. Select your dietary restrictions and favourite cuisines
4. Your home feed will now show personalised recommendations

## What's Next?

- [Configuration](configuration.md) — Configure environment variables and API keys
- [Development Guide](../guides/development.md) — Start contributing to Blend
- [Architecture Overview](../architecture/overview.md) — Understand how the system works
