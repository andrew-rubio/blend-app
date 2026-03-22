// ──────────────────────────────────────────────────────────────────────────────
// container-registry.bicep — Azure Container Registry
// ──────────────────────────────────────────────────────────────────────────────

@description('Name prefix for all resources')
param namePrefix string

@description('Azure region')
param location string

@description('Deployment environment')
@allowed(['dev', 'staging', 'prod'])
param environment string

// ── Resource name ─────────────────────────────────────────────────────────────
// ACR names: alphanumeric only, 5–50 chars
var registryName = replace('${namePrefix}acr${environment}', '-', '')

// ── Azure Container Registry ─────────────────────────────────────────────────
resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: registryName
  location: location
  sku: {
    name: environment == 'prod' ? 'Premium' : 'Basic'
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
    zoneRedundancy: environment == 'prod' ? 'Enabled' : 'Disabled'
  }
  tags: {
    environment: environment
    application: 'blend'
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────
output registryId string = registry.id
output loginServer string = registry.properties.loginServer
output registryName string = registry.name
