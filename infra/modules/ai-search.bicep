// Azure AI Search
// Full-text and vector search for recipes and ingredient knowledge base.

@description('Azure region.')
param location string

@description('Environment name.')
param environmentName string

@description('Application name.')
param appName string

// ── Search Service ────────────────────────────────────────────────────────────
resource searchService 'Microsoft.Search/searchServices@2024-03-01-preview' = {
  name: 'srch-${appName}-${environmentName}'
  location: location
  sku: {
    name: environmentName == 'production' ? 'standard' : 'basic'
  }
  properties: {
    replicaCount: environmentName == 'production' ? 2 : 1
    partitionCount: 1
    hostingMode: 'default'
    publicNetworkAccess: 'Enabled'
    semanticSearch: environmentName == 'production' ? 'standard' : 'disabled'
  }
  identity: {
    type: 'SystemAssigned'
  }
  tags: {
    application: appName
    environment: environmentName
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────
output endpoint string = 'https://${searchService.name}.search.windows.net'
output serviceName string = searchService.name
output principalId string = searchService.identity.principalId
