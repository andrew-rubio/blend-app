// Azure Container Apps Environment + Blend.Api Container App
// Deploys the backend API as a serverless container.

@description('Azure region.')
param location string

@description('Environment name.')
param environmentName string

@description('Application name.')
param appName string

@description('ACR login server (e.g. acrblenddev.azurecr.io).')
param acrLoginServer string

@description('Cosmos DB connection string (from cosmos-db module output).')
@secure()
param cosmosDbConnectionString string

@description('Container image tag to deploy.')
param imageTag string = 'latest'

// ── Container Apps Environment ────────────────────────────────────────────────
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'log-${appName}-${environmentName}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: environmentName == 'production' ? 90 : 30
  }
  tags: {
    application: appName
    environment: environmentName
  }
}

resource containerAppsEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: 'cae-${appName}-${environmentName}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    zoneRedundant: environmentName == 'production'
  }
  tags: {
    application: appName
    environment: environmentName
  }
}

// ── Blend.Api Container App ───────────────────────────────────────────────────
resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'ca-${appName}-api-${environmentName}'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppsEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        corsPolicy: {
          allowedOrigins: [
            'https://swa-${appName}-${environmentName}.azurestaticapps.net'
            'http://localhost:3000'
          ]
          allowedMethods: ['GET', 'POST', 'PUT', 'DELETE', 'PATCH', 'OPTIONS']
          allowedHeaders: ['*']
        }
      }
      registries: [
        {
          server: acrLoginServer
          identity: 'system'
        }
      ]
      secrets: [
        {
          name: 'cosmos-connection'
          value: cosmosDbConnectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'blend-api'
          image: '${acrLoginServer}/blend-api:${imageTag}'
          resources: {
            cpu: json(environmentName == 'production' ? '0.5' : '0.25')
            memory: environmentName == 'production' ? '1Gi' : '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environmentName == 'production' ? 'Production' : 'Staging'
            }
            {
              name: 'CosmosDb__ConnectionString'
              secretRef: 'cosmos-connection'
            }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health'
                port: 8080
              }
              initialDelaySeconds: 30
              periodSeconds: 30
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health/ready'
                port: 8080
              }
              initialDelaySeconds: 10
              periodSeconds: 10
            }
          ]
        }
      ]
      scale: {
        minReplicas: environmentName == 'production' ? 1 : 0
        maxReplicas: environmentName == 'production' ? 10 : 3
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
  tags: {
    application: appName
    environment: environmentName
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────
output backendUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output containerAppName string = containerApp.name
output containerAppId string = containerApp.id
output principalId string = containerApp.identity.principalId
