// functions.bicep - Azure Functions app (image processing)
// Uses identity-based connections (subscription policy blocks shared key access)

@description('Name prefix for all resources')
param namePrefix string

@description('Azure region')
param location string

@description('Deployment environment')
@allowed(['dev', 'staging', 'prod'])
param environment string

@description('Name of the main storage account (for blob triggers)')
param storageAccountName string

// Dedicated storage account for Functions runtime
var fnStorageAccountName = take(replace(toLower('${namePrefix}fnst${environment}'), '-', ''), 24)

resource fnStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: fnStorageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
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

// App Service Plan (Consumption)
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${namePrefix}-asp-fn-${environment}'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: true
  }
  tags: {
    environment: environment
    application: 'blend'
  }
}

// Application Insights for Functions
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${namePrefix}-ai-fn-${environment}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    RetentionInDays: environment == 'prod' ? 90 : 30
  }
  tags: {
    environment: environment
    application: 'blend'
  }
}

// Azure Functions App - uses managed identity for storage access
resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: '${namePrefix}-fn-${environment}'
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    reserved: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|9.0'
      appSettings: [
        {
          name: 'AzureWebJobsStorage__accountName'
          value: fnStorageAccount.name
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
      ]
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
  }
  tags: {
    environment: environment
    application: 'blend'
  }
}

// RBAC: Grant Functions managed identity Storage Blob Data Owner on its dedicated storage
resource blobDataOwnerRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(fnStorageAccount.id, functionApp.id, 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
  scope: fnStorageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b')
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// RBAC: Grant Functions managed identity Storage Account Contributor (needed for file shares)
resource storageContributorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(fnStorageAccount.id, functionApp.id, '17d1049b-9a84-46fb-8f53-869881c3d3ab')
  scope: fnStorageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '17d1049b-9a84-46fb-8f53-869881c3d3ab')
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

output functionAppId string = functionApp.id
output functionAppName string = functionApp.name
output functionAppHostname string = functionApp.properties.defaultHostName
output principalId string = functionApp.identity.principalId
