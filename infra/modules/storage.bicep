// ──────────────────────────────────────────────────────────────────────────────
// storage.bicep — Azure Blob Storage account + CDN profile
// ──────────────────────────────────────────────────────────────────────────────

@description('Name prefix for all resources')
param namePrefix string

@description('Azure region')
param location string

@description('Deployment environment')
@allowed(['dev', 'staging', 'prod'])
param environment string

@description('Allowed CORS origins for the blob storage (restrict in production)')
param corsAllowedOrigins array = []

// ── Storage account name: alphanumeric, 3–24 chars ───────────────────────────
var storageAccountName = take(replace(toLower('${namePrefix}st${environment}'), '-', ''), 24)

// ── Storage Account ───────────────────────────────────────────────────────────
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: environment == 'prod' ? 'Standard_GRS' : 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: environment != 'prod'
    allowSharedKeyAccess: true
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
  tags: {
    environment: environment
    application: 'blend'
  }
}

// ── Blob Service ─────────────────────────────────────────────────────────────
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    cors: {
      corsRules: [
        {
          allowedOrigins: corsAllowedOrigins
          allowedMethods: ['GET', 'HEAD']
          allowedHeaders: ['*']
          exposedHeaders: ['*']
          maxAgeInSeconds: 3600
        }
      ]
    }
  }
}

// ── Assets container ─────────────────────────────────────────────────────────
resource assetsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'blend-assets'
  properties: {
    publicAccess: 'Blob'
  }
}

// ── CDN Profile ──────────────────────────────────────────────────────────────
resource cdnProfile 'Microsoft.Cdn/profiles@2023-05-01' = {
  name: '${namePrefix}-cdn-${environment}'
  location: 'Global'
  sku: {
    name: 'Standard_Microsoft'
  }
  tags: {
    environment: environment
    application: 'blend'
  }
}

// ── CDN Endpoint ─────────────────────────────────────────────────────────────
resource cdnEndpoint 'Microsoft.Cdn/profiles/endpoints@2023-05-01' = {
  parent: cdnProfile
  name: '${namePrefix}-cdn-ep-${environment}'
  location: 'Global'
  properties: {
    originHostHeader: '${storageAccountName}.blob.${az.environment().suffixes.storage}'
    isHttpAllowed: false
    isHttpsAllowed: true
    origins: [
      {
        name: 'storage-origin'
        properties: {
          hostName: '${storageAccountName}.blob.${az.environment().suffixes.storage}'
          httpsPort: 443
          originHostHeader: '${storageAccountName}.blob.${az.environment().suffixes.storage}'
        }
      }
    ]
    isCompressionEnabled: true
    contentTypesToCompress: [
      'application/json'
      'text/plain'
      'image/svg+xml'
    ]
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────
output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob
output cdnEndpoint string = 'https://${cdnEndpoint.properties.hostName}'
