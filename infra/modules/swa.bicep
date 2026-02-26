// Azure Static Web Apps
// Hosts the Blend.Web Next.js frontend with API proxy to Blend.Api.

@description('Azure region.')
param location string

@description('Environment name.')
param environmentName string

@description('Application name.')
param appName string

@description('Azure resource ID of the Container App backend.')
param containerAppResourceId string

// ── Static Web App ────────────────────────────────────────────────────────────
resource swa 'Microsoft.Web/staticSites@2023-12-01' = {
  name: 'swa-${appName}-${environmentName}'
  location: location
  sku: {
    name: environmentName == 'production' ? 'Standard' : 'Free'
    tier: environmentName == 'production' ? 'Standard' : 'Free'
  }
  properties: {
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
    enterpriseGradeCdnStatus: environmentName == 'production' ? 'Enabled' : 'Disabled'
  }
  tags: {
    application: appName
    environment: environmentName
  }
}

// ── Linked Backend ────────────────────────────────────────────────────────────
resource linkedBackend 'Microsoft.Web/staticSites/linkedBackends@2023-12-01' = {
  parent: swa
  name: 'blend-api-link'
  properties: {
    backendResourceId: containerAppResourceId
    region: location
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────
output defaultHostname string = swa.properties.defaultHostname
output swaId string = swa.id
output swaName string = swa.name
