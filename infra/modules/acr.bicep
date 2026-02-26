// Azure Container Registry
// Stores Docker images for Blend.Api.

@description('Azure region.')
param location string

@description('Environment name.')
param environmentName string

@description('Application name.')
param appName string

// ── Resource ──────────────────────────────────────────────────────────────────
resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: 'acr${appName}${environmentName}'
  location: location
  sku: {
    name: environmentName == 'production' ? 'Premium' : 'Basic'
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
    zoneRedundancy: environmentName == 'production' ? 'Enabled' : 'Disabled'
  }
  tags: {
    application: appName
    environment: environmentName
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────
output loginServer string = acr.properties.loginServer
output acrId string = acr.id
