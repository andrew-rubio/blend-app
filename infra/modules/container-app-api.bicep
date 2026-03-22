// ──────────────────────────────────────────────────────────────────────────────
// container-app-api.bicep — Blend API Azure Container App
// ──────────────────────────────────────────────────────────────────────────────

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

// ── Container App ─────────────────────────────────────────────────────────────
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
      registries: [
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
          image: '${registryLoginServer}/blend-api:${imageTag}'
          resources: {
            cpu: json(environment == 'prod' ? '0.5' : '0.25')
            memory: environment == 'prod' ? '1Gi' : '0.5Gi'
          }
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

// ── Outputs ───────────────────────────────────────────────────────────────────
output apiUrl string = 'https://${apiApp.properties.configuration.ingress.fqdn}'
output containerAppName string = apiApp.name
output principalId string = apiApp.identity.principalId
