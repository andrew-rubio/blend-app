// ──────────────────────────────────────────────────────────────────────────────
// static-web-app.bicep — Azure Static Web Apps (Frontend)
// ──────────────────────────────────────────────────────────────────────────────

@description('Name prefix for all resources')
param namePrefix string

@description('Azure region (SWA supports a limited set; content is served globally via CDN)')
param location string = 'westeurope'

@description('Deployment environment')
@allowed(['dev', 'staging', 'prod'])
param environment string

@description('Backend API URL for the linked backend feature')
param backendUrl string = ''

// ── Azure Static Web App ─────────────────────────────────────────────────────
resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: '${namePrefix}-swa-${environment}'
  location: location
  sku: {
    name: environment == 'prod' ? 'Standard' : 'Free'
    tier: environment == 'prod' ? 'Standard' : 'Free'
  }
  properties: {
    buildProperties: {
      appLocation: 'src/Blend.Web'
      outputLocation: '.next'
      skipGithubActionWorkflowGeneration: true
    }
    stagingEnvironmentPolicy: environment == 'prod' ? 'Enabled' : 'Disabled'
    allowConfigFileUpdates: true
    enterpriseGradeCdnStatus: 'Disabled'
  }
  tags: {
    environment: environment
    application: 'blend'
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────
output staticWebAppId string = staticWebApp.id
output defaultHostname string = staticWebApp.properties.defaultHostname
output staticWebAppName string = staticWebApp.name
