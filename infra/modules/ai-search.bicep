// ──────────────────────────────────────────────────────────────────────────────
// ai-search.bicep — Azure AI Search service
// ──────────────────────────────────────────────────────────────────────────────

@description('Name prefix for all resources')
param namePrefix string

@description('Azure region')
param location string

@description('Deployment environment')
@allowed(['dev', 'staging', 'prod'])
param environment string

// ── Azure AI Search ───────────────────────────────────────────────────────────
resource searchService 'Microsoft.Search/searchServices@2024-03-01-preview' = {
  name: '${namePrefix}-search-${environment}'
  location: location
  sku: {
    name: environment == 'prod' ? 'standard' : 'basic'
  }
  properties: {
    replicaCount: environment == 'prod' ? 2 : 1
    partitionCount: environment == 'prod' ? 2 : 1
    hostingMode: 'default'
    publicNetworkAccess: 'enabled'
    semanticSearch: environment == 'prod' ? 'standard' : 'disabled'
    authOptions: {
      aadOrApiKey: {
        aadAuthFailureMode: 'http401WithBearerChallenge'
      }
    }
  }
  tags: {
    environment: environment
    application: 'blend'
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────
output searchServiceId string = searchService.id
output searchServiceName string = searchService.name
output searchEndpoint string = 'https://${searchService.name}.search.windows.net'
