// Blend Infrastructure — Main Bicep entry point
// Deploys all resources for a given environment (dev/staging/production).

targetScope = 'subscription'

@description('Deployment environment name.')
@allowed(['dev', 'staging', 'production'])
param environmentName string = 'dev'

@description('Azure region for all resources.')
param location string = 'australiaeast'

@description('Short application name used in resource naming.')
param appName string = 'blend'

// ── Resource Group ────────────────────────────────────────────────────────────
resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: 'rg-${appName}-${environmentName}'
  location: location
  tags: {
    application: appName
    environment: environmentName
    managedBy: 'bicep'
  }
}

// ── Module Deployments ────────────────────────────────────────────────────────
module acr 'modules/acr.bicep' = {
  name: 'acr'
  scope: rg
  params: {
    location: location
    environmentName: environmentName
    appName: appName
  }
}

module cosmosDb 'modules/cosmos-db.bicep' = {
  name: 'cosmosDb'
  scope: rg
  params: {
    location: location
    environmentName: environmentName
    appName: appName
  }
}

module aiSearch 'modules/ai-search.bicep' = {
  name: 'aiSearch'
  scope: rg
  params: {
    location: location
    environmentName: environmentName
    appName: appName
  }
}

module storage 'modules/storage.bicep' = {
  name: 'storage'
  scope: rg
  params: {
    location: location
    environmentName: environmentName
    appName: appName
  }
}

module containerApps 'modules/container-apps.bicep' = {
  name: 'containerApps'
  scope: rg
  params: {
    location: location
    environmentName: environmentName
    appName: appName
    acrLoginServer: acr.outputs.loginServer
    cosmosDbConnectionString: cosmosDb.outputs.connectionString
  }
}

module swa 'modules/swa.bicep' = {
  name: 'swa'
  scope: rg
  params: {
    location: location
    environmentName: environmentName
    appName: appName
    containerAppResourceId: containerApps.outputs.containerAppId
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────
output resourceGroupName string = rg.name
output backendUrl string = containerApps.outputs.backendUrl
output frontendUrl string = swa.outputs.defaultHostname
