# Installation

This page covers how to set up a local development environment for the Blend application.

## Prerequisites

Before you begin, ensure you have the following installed:

- [Git](https://git-scm.com/)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+ and npm](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for the Cosmos DB emulator and Dev Container)
- [VS Code](https://code.visualstudio.com/) with the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) (recommended)

## Option 1: Dev Container (Recommended)

The repository includes a pre-configured Dev Container that installs all required tools automatically.

1. Clone the repository:
   ```bash
   git clone https://github.com/andrew-rubio/blend-app.git
   cd blend-app
   ```
2. Open in VS Code:
   ```bash
   code .
   ```
3. When prompted, click **Reopen in Container**. VS Code will build the Dev Container and install all dependencies.

## Option 2: Manual Setup

### 1. Clone the repository

```bash
git clone https://github.com/andrew-rubio/blend-app.git
cd blend-app
```

### 2. Install backend dependencies

```bash
dotnet restore src/backend/Blend.slnx
```

### 3. Install frontend dependencies

```bash
cd src/Blend.Web
npm install
```

### 4. Configure environment variables

Copy the example environment files and fill in the required values:

```bash
cp src/backend/Blend.Api/appsettings.Development.json.example \
   src/backend/Blend.Api/appsettings.Development.json
cp src/Blend.Web/.env.local.example src/Blend.Web/.env.local
```

See [Configuration](configuration.md) for all required variables.

### 5. Start the application

**Backend (with .NET Aspire):**

```bash
dotnet run --project src/backend/Blend.AppHost
```

**Frontend:**

```bash
cd src/Blend.Web
npm run dev
```

The API will be available at `https://localhost:7000` and the frontend at `http://localhost:3000`.

## Verifying the Installation

Once running, open `http://localhost:3000` in your browser. You should see the Blend landing page.

To verify the API is healthy:

```bash
curl https://localhost:7000/healthz
```

Expected response: `Healthy`

## Next Steps

- [Quick Start](quick-start.md) — Walk through the main user flows
- [Configuration](configuration.md) — Configure environment variables and options
