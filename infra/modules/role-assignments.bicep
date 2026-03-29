// RBAC role assignments granting the API managed identity least-privilege access
// to Cosmos DB, Blob Storage, Container Registry, and Key Vault.

@description('Principal ID of the API Container App managed identity')
param apiPrincipalId string

@description('Principal ID of the Functions App managed identity')
param functionsPrincipalId string

@description('Cosmos DB account name')
param cosmosAccountName string

@description('Storage account name')
param storageAccountName string

@description('Container Registry name')
param registryName string

@description('Key Vault name')
param keyVaultName string

// Built-in role definition IDs
var cosmosDbDataContributor = '00000000-0000-0000-0000-000000000002' // Cosmos DB Built-in Data Contributor
var storageBlobDataContributor = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
var acrPull = '7f951dda-4ed3-4680-a7ca-43fe172d538d'
var keyVaultSecretsUser = '4633458b-17de-408a-b874-0445c86b69e6'

// Existing resource references
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-02-15-preview' existing = {
  name: cosmosAccountName
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: registryName
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// API → Cosmos DB Data Contributor (read/write containers and items)
resource apiCosmosRole 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-02-15-preview' = {
  parent: cosmosAccount
  name: guid(cosmosAccount.id, apiPrincipalId, cosmosDbDataContributor)
  properties: {
    roleDefinitionId: '${cosmosAccount.id}/sqlRoleDefinitions/${cosmosDbDataContributor}'
    principalId: apiPrincipalId
    scope: cosmosAccount.id
  }
}

// API → Blob Storage Data Contributor (media uploads)
resource apiBlobRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, apiPrincipalId, storageBlobDataContributor)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataContributor)
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// API → ACR Pull (pull container images)
resource apiAcrRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, apiPrincipalId, acrPull)
  scope: registry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPull)
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// API → Key Vault Secrets User (read secrets)
resource apiKeyVaultRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, apiPrincipalId, keyVaultSecretsUser)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUser)
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Functions → Blob Storage Data Contributor (image processing)
resource functionsBlobRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(functionsPrincipalId)) {
  name: guid(storageAccount.id, functionsPrincipalId, storageBlobDataContributor)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataContributor)
    principalId: functionsPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Functions → Key Vault Secrets User
resource functionsKeyVaultRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(functionsPrincipalId)) {
  name: guid(keyVault.id, functionsPrincipalId, keyVaultSecretsUser)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUser)
    principalId: functionsPrincipalId
    principalType: 'ServicePrincipal'
  }
}
