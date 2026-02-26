// Azure Blob Storage + CDN
// Stores and serves user-uploaded recipe media (images, videos).

@description('Azure region.')
param location string

@description('Environment name.')
param environmentName string

@description('Application name.')
param appName string

// ── Storage Account ───────────────────────────────────────────────────────────
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: 'st${appName}${environmentName}'
  location: location
  kind: 'StorageV2'
  sku: {
    name: environmentName == 'production' ? 'Standard_ZRS' : 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
  }
  tags: {
    application: appName
    environment: environmentName
  }
}

// ── Blob Service & Containers ─────────────────────────────────────────────────
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    cors: {
      corsRules: [
        {
          allowedOrigins: [
            'https://swa-${appName}-${environmentName}.azurestaticapps.net'
            'http://localhost:3000'
          ]
          allowedMethods: ['GET', 'HEAD', 'OPTIONS']
          allowedHeaders: ['*']
          exposedHeaders: ['*']
          maxAgeInSeconds: 3600
        }
      ]
    }
  }
}

resource mediaContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'blend-media'
  properties: {
    publicAccess: 'None'
  }
}

// ── CDN Profile + Endpoint ────────────────────────────────────────────────────
resource cdnProfile 'Microsoft.Cdn/profiles@2023-05-01' = {
  name: 'cdn-${appName}-${environmentName}'
  location: 'global'
  sku: {
    name: 'Standard_Microsoft'
  }
  tags: {
    application: appName
    environment: environmentName
  }
}

resource cdnEndpoint 'Microsoft.Cdn/profiles/endpoints@2023-05-01' = {
  parent: cdnProfile
  name: 'cdn-${appName}-media-${environmentName}'
  location: 'global'
  properties: {
    originHostHeader: '${storageAccount.name}.blob.core.windows.net'
    isHttpAllowed: false
    isHttpsAllowed: true
    origins: [
      {
        name: 'storage-origin'
        properties: {
          hostName: '${storageAccount.name}.blob.core.windows.net'
          httpsPort: 443
          originHostHeader: '${storageAccount.name}.blob.core.windows.net'
        }
      }
    ]
    deliveryPolicy: {
      rules: [
        {
          name: 'EnforceHTTPS'
          order: 1
          conditions: [
            {
              name: 'RequestScheme'
              parameters: {
                typeName: 'DeliveryRuleRequestSchemeConditionParameters'
                matchValues: ['HTTP']
                operator: 'Equal'
                negateCondition: false
              }
            }
          ]
          actions: [
            {
              name: 'UrlRedirect'
              parameters: {
                typeName: 'DeliveryRuleUrlRedirectActionParameters'
                redirectType: 'PermanentRedirect'
                destinationProtocol: 'Https'
              }
            }
          ]
        }
      ]
    }
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────
output storageAccountId string = storageAccount.id
output storageAccountName string = storageAccount.name
output cdnEndpointHostname string = cdnEndpoint.properties.hostName
