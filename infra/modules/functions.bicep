// Azure Functions (placeholder)
// Used for async background jobs: media processing, notification dispatch,
// scheduled data enrichment, and search index maintenance.
//
// NOTE: Functions are not deployed with the initial scaffolding.
// This file is a placeholder for future implementation.

@description('Azure region.')
param location string

@description('Environment name.')
param environmentName string

@description('Application name.')
param appName string

@description('Storage account name (shared with media storage).')
param storageAccountName string

// ── App Service Plan (Consumption) ───────────────────────────────────────────
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'asp-${appName}-func-${environmentName}'
  location: location
  kind: 'functionapp'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {}
  tags: {
    application: appName
    environment: environmentName
  }
}

// ── Function App ──────────────────────────────────────────────────────────────
resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: 'func-${appName}-${environmentName}'
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      use32BitWorkerProcess: false
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          // Uses managed identity — assign 'Storage Blob Data Contributor'
          // RBAC role on the storage account to the Functions principalId.
          name: 'AzureWebJobsStorage__accountName'
          value: storageAccountName
        }
      ]
    }
    httpsOnly: true
  }
  tags: {
    application: appName
    environment: environmentName
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────
output functionAppName string = functionApp.name
output functionAppId string = functionApp.id
output principalId string = functionApp.identity.principalId
