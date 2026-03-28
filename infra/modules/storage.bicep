// ──────────────────────────────────────────────────────────────────────────────
// storage.bicep — Azure Blob Storage account + CDN profile
// ──────────────────────────────────────────────────────────────────────────────

@description('Name prefix for all resources')
@minLength(3)
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
    allowBlobPublicAccess: false
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
    publicAccess: 'None'
  }
}

// ── Front Door CDN Profile (replaces deprecated classic CDN) ─────────────────
resource frontDoorProfile 'Microsoft.Cdn/profiles@2024-02-01' = {
  name: '${namePrefix}-fd-${environment}'
  location: 'Global'
  sku: {
    name: 'Standard_AzureFrontDoor'
  }
  tags: {
    environment: environment
    application: 'blend'
  }
}

resource frontDoorEndpoint 'Microsoft.Cdn/profiles/afdEndpoints@2024-02-01' = {
  parent: frontDoorProfile
  name: '${namePrefix}-fd-ep-${environment}'
  location: 'Global'
  properties: {
    enabledState: 'Enabled'
  }
}

resource originGroup 'Microsoft.Cdn/profiles/originGroups@2024-02-01' = {
  parent: frontDoorProfile
  name: 'storage-origin-group'
  properties: {
    loadBalancingSettings: {
      sampleSize: 4
      successfulSamplesRequired: 3
    }
    healthProbeSettings: {
      probePath: '/'
      probeRequestType: 'HEAD'
      probeProtocol: 'Https'
      probeIntervalInSeconds: 100
    }
  }
}

resource origin 'Microsoft.Cdn/profiles/originGroups/origins@2024-02-01' = {
  parent: originGroup
  name: 'storage-origin'
  properties: {
    hostName: '${storageAccountName}.blob.${az.environment().suffixes.storage}'
    httpPort: 80
    httpsPort: 443
    originHostHeader: '${storageAccountName}.blob.${az.environment().suffixes.storage}'
    priority: 1
    weight: 1000
    enabledState: 'Enabled'
  }
}

resource route 'Microsoft.Cdn/profiles/afdEndpoints/routes@2024-02-01' = {
  parent: frontDoorEndpoint
  name: 'storage-route'
  properties: {
    originGroup: {
      id: originGroup.id
    }
    supportedProtocols: ['Https']
    patternsToMatch: ['/*']
    forwardingProtocol: 'HttpsOnly'
    linkToDefaultDomain: 'Enabled'
    httpsRedirect: 'Enabled'
  }
  dependsOn: [origin]
}

// ── Outputs ───────────────────────────────────────────────────────────────────
output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob
output cdnEndpoint string = 'https://${frontDoorEndpoint.properties.hostName}'
