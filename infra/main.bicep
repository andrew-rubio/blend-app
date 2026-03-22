// ──────────────────────────────────────────────────────────────────────────────
// main.bicep — Blend App — Top-level orchestration
//
// Deploys all Blend application resources into the target resource group.
// Parameters are supplied via main.bicepparam (environment-specific files).
// ──────────────────────────────────────────────────────────────────────────────

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

// ── Modules ───────────────────────────────────────────────────────────────────

module registry 'modules/container-registry.bicep' = {
  name: 'registry'
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
  }
}

module containerAppsEnv 'modules/container-apps-environment.bicep' = {
  name: 'containerAppsEnv'
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
  }
}

module staticWebApp 'modules/static-web-app.bicep' = {
  name: 'staticWebApp'
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
  }
}

module api 'modules/container-app-api.bicep' = {
  name: 'api'
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
    containerAppsEnvironmentId: containerAppsEnv.outputs.environmentId
    registryLoginServer: registry.outputs.loginServer
    imageTag: apiImageTag
    corsAllowedOrigin: 'https://${staticWebApp.outputs.defaultHostname}'
  }
}

module cosmosDb 'modules/cosmos-db.bicep' = {
  name: 'cosmosDb'
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
  }
}

module search 'modules/ai-search.bicep' = {
  name: 'search'
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
  }
}

module storage 'modules/storage.bicep' = {
  name: 'storage'
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
    corsAllowedOrigins: ['https://${staticWebApp.outputs.defaultHostname}']
  }
}

module functions 'modules/functions.bicep' = {
  name: 'functions'
  params: {
    namePrefix: namePrefix
    location: location
    environment: environment
    storageAccountName: storage.outputs.storageAccountName
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────

output apiUrl string = api.outputs.apiUrl
output frontendUrl string = staticWebApp.outputs.defaultHostname
output registryLoginServer string = registry.outputs.loginServer
