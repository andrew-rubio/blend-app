// main.bicep - Blend App - Top-level orchestration
//
// Deploys all Blend application resources into the target resource group.
// Parameters are supplied via main.bicepparam (environment-specific files).

targetScope = 'resourceGroup'

@description('Name prefix for all resources (e.g. blend-prod)')
param namePrefix string

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Deployment environment tag (dev | staging | prod)')
@allowed(['dev', 'staging', 'prod'])
param environment string

@description('Container image tag to deploy to Container Apps')
param apiImageTag string = 'latest'

@description('Deploy Azure Functions for image processing (requires Dynamic VM quota)')
param deployFunctions bool = true

@description('Use a public placeholder image for initial provisioning (before first ACR push)')
param usePublicImage bool = true

// Container Registry
module registry 'modules/container-registry.bicep' = {
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
  }
}

// Container Apps Environment + Log Analytics
module containerAppsEnv 'modules/container-apps-environment.bicep' = {
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
  }
}

// Key Vault for secrets management
module keyVault 'modules/key-vault.bicep' = {
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
  }
}

// Application Insights for API monitoring
module appInsights 'modules/app-insights.bicep' = {
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
    logAnalyticsWorkspaceId: containerAppsEnv.outputs.logAnalyticsWorkspaceId
  }
}

// Static Web App for Next.js frontend
// SWA only supports a limited set of regions; westeurope is the closest to UK South
module staticWebApp 'modules/static-web-app.bicep' = {
  params: {
    namePrefix: namePrefix
    location: 'westeurope'
    environment: environment
  }
}

// Cosmos DB NoSQL database
module cosmosDb 'modules/cosmos-db.bicep' = {
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
  }
}

// Blob Storage for media uploads + CDN
module storage 'modules/storage.bicep' = {
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
    corsAllowedOrigins: ['https://${staticWebApp.outputs.defaultHostname}']
  }
}

// API Container App with all configuration wired in
module api 'modules/container-app-api.bicep' = {
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
    containerAppsEnvironmentId: containerAppsEnv.outputs.environmentId
    registryLoginServer: registry.outputs.loginServer
    imageTag: apiImageTag
    usePublicImage: usePublicImage
    corsAllowedOrigin: 'https://${staticWebApp.outputs.defaultHostname}'
    cosmosEndpoint: cosmosDb.outputs.documentEndpoint
    keyVaultUri: keyVault.outputs.keyVaultUri
    appInsightsConnectionString: appInsights.outputs.connectionString
    blobEndpoint: storage.outputs.blobEndpoint
  }
}

// Azure AI Search
module search 'modules/ai-search.bicep' = {
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
  }
}

// Azure Functions for image processing (skipped when subscription lacks Dynamic VM quota)
module functions 'modules/functions.bicep' = if (deployFunctions) {
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
    storageAccountName: storage.outputs.storageAccountName
  }
}

// RBAC role assignments - grants managed identities least-privilege access
module roleAssignments 'modules/role-assignments.bicep' = {
  params: {
    apiPrincipalId: api.outputs.principalId
    functionsPrincipalId: deployFunctions ? functions.outputs.principalId : ''
    cosmosAccountName: cosmosDb.outputs.accountName
    storageAccountName: storage.outputs.storageAccountName
    registryName: registry.outputs.registryName
    keyVaultName: keyVault.outputs.keyVaultName
  }
}

// Outputs
output apiUrl string = api.outputs.apiUrl
output frontendUrl string = staticWebApp.outputs.defaultHostname
output registryLoginServer string = registry.outputs.loginServer
output keyVaultUri string = keyVault.outputs.keyVaultUri
output appInsightsConnectionString string = appInsights.outputs.connectionString
