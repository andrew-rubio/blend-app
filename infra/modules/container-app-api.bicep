// container-app-api.bicep - Blend API Azure Container App

@description('Name prefix for all resources')
param namePrefix string

@description('Azure region')
param location string

@description('Deployment environment')
@allowed(['dev', 'staging', 'prod'])
param environment string

@description('Resource ID of the Container Apps Environment')
param containerAppsEnvironmentId string

@description('ACR login server (e.g. myregistry.azurecr.io)')
param registryLoginServer string

@description('Container image tag to deploy')
param imageTag string = 'latest'

@description('Allowed CORS origin for the API (e.g. the SWA hostname)')
param corsAllowedOrigin string = '*'

@description('Cosmos DB account endpoint for data access')
param cosmosEndpoint string = ''

@description('Cosmos DB database name')
param cosmosDatabaseName string = 'blend'

@description('Key Vault URI for secret retrieval')
param keyVaultUri string = ''

@description('Application Insights connection string')
param appInsightsConnectionString string = ''

@description('Blob Storage endpoint')
param blobEndpoint string = ''

@description('Use a public placeholder image for initial provisioning (before first ACR push)')
param usePublicImage bool = false

// Container image: use MCR placeholder for initial deploy, ACR image after first CI push
var containerImage = usePublicImage ? 'mcr.microsoft.com/k8se/quickstart:latest' : '${registryLoginServer}/blend-api:${imageTag}'

resource apiApp 'Microsoft.App/containerApps@2023-11-02-preview' = {
  name: '${namePrefix}-api-${environment}'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        corsPolicy: {
          allowedOrigins: [corsAllowedOrigin]
          allowedMethods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS']
          allowedHeaders: ['*']
        }
      }
      registries: usePublicImage ? [] : [
        {
          server: registryLoginServer
          identity: 'system'
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'blend-api'
          image: containerImage
          resources: {
            cpu: json(environment == 'prod' ? '0.5' : '0.25')
            memory: environment == 'prod' ? '1Gi' : '0.5Gi'
          }
          env: [
            { name: 'ASPNETCORE_ENVIRONMENT', value: environment == 'prod' ? 'Production' : 'Development' }
            { name: 'CosmosDb__EndpointUri', value: cosmosEndpoint }
            { name: 'CosmosDb__DatabaseName', value: cosmosDatabaseName }
            { name: 'CosmosDb__EnsureCreated', value: 'true' }
            { name: 'AzureBlobStorage__BlobEndpoint', value: blobEndpoint }
            { name: 'AzureBlobStorage__ContainerName', value: 'blend-media' }
            { name: 'KeyVault__Uri', value: keyVaultUri }
            { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsightsConnectionString }
            { name: 'Cors__AllowedOrigins__0', value: corsAllowedOrigin }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/healthz'
                port: 8080
              }
              initialDelaySeconds: 10
              periodSeconds: 30
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/ready'
                port: 8080
              }
              initialDelaySeconds: 5
              periodSeconds: 10
            }
          ]
        }
      ]
      scale: {
        minReplicas: environment == 'prod' ? 1 : 0
        maxReplicas: environment == 'prod' ? 10 : 3
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '50'
              }
            }
          }
        ]
      }
    }
  }
  tags: {
    environment: environment
    application: 'blend'
  }
}

output apiUrl string = 'https://${apiApp.properties.configuration.ingress.fqdn}'
output containerAppName string = apiApp.name
output principalId string = apiApp.identity.principalId
