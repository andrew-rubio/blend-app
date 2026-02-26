# Deployment Guide

This guide covers deploying your application to Azure using the Blend deployment workflow.

## Prerequisites

- Azure subscription
- Azure Developer CLI (`azd`) installed (included in the Dev Container)
- Azure CLI (`az`) installed (included in the Dev Container)

## Deploying with the `/deploy` Workflow

The simplest way to deploy is through the Blend deployment prompt:

```
/deploy
```

The Azure agent will:

1. Identify your application type and infrastructure requirements
2. Generate or update `azure.yaml` and `infra/` Bicep templates
3. Run `azd up` to provision resources and deploy the application

## Manual Deployment

### Step 1: Initialise Azure Developer CLI

```bash
azd init
```

### Step 2: Log in to Azure

```bash
azd auth login
```

### Step 3: Provision Infrastructure

```bash
azd provision
```

### Step 4: Deploy the Application

```bash
azd deploy
```

### Step 5: View Deployment Status

```bash
azd show
```

## Environment Configuration

Before deploying, ensure your environment variables are set. See [Environment Variables](../reference/environment-variables.md) for the full list.

```bash
azd env set MY_VARIABLE my-value
```

## CI/CD Deployment

Blend includes a GitHub Actions workflow for automated deployment. To enable it:

1. Configure the required secrets in your GitHub repository settings
2. Push to the `main` branch to trigger a deployment

## Rollback

To roll back to a previous deployment:

```bash
azd deploy --from-package <previous-package-path>
```

## Troubleshooting

See the [Troubleshooting guide](troubleshooting.md) for common deployment issues.
