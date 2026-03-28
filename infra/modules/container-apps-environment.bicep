// ──────────────────────────────────────────────────────────────────────────────
// container-apps-environment.bicep — Azure Container Apps Environment
// ──────────────────────────────────────────────────────────────────────────────

@description('Name prefix for all resources')
param namePrefix string

@description('Azure region')
param location string

@description('Deployment environment')
@allowed(['dev', 'staging', 'prod'])
param environment string

// ── Log Analytics Workspace (required by Container Apps) ─────────────────────
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${namePrefix}-logs-${environment}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: environment == 'prod' ? 90 : 30
  }
  tags: {
    environment: environment
    application: 'blend'
  }
}

// ── Container Apps Managed Environment ───────────────────────────────────────
resource containerAppsEnv 'Microsoft.App/managedEnvironments@2023-11-02-preview' = {
  name: '${namePrefix}-cae-${environment}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    zoneRedundant: environment == 'prod'
  }
  tags: {
    environment: environment
    application: 'blend'
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────
output environmentId string = containerAppsEnv.id
output environmentName string = containerAppsEnv.name
output defaultDomain string = containerAppsEnv.properties.defaultDomain
output logAnalyticsWorkspaceId string = logAnalytics.id
