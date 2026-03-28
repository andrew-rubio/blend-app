// ──────────────────────────────────────────────────────────────────────────────
// cosmos-db.bicep — Azure Cosmos DB account (NoSQL API)
// ──────────────────────────────────────────────────────────────────────────────

@description('Name prefix for all resources')
param namePrefix string

@description('Azure region')
param location string

@description('Deployment environment')
@allowed(['dev', 'staging', 'prod'])
param environment string

// ── Cosmos DB Account ─────────────────────────────────────────────────────────
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-02-15-preview' = {
  name: '${namePrefix}-cosmos-${environment}'
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: environment == 'prod'
      }
    ]
    enableFreeTier: false
    enableAutomaticFailover: environment == 'prod'
    enableMultipleWriteLocations: false
    backupPolicy: environment == 'prod' ? {
      type: 'Continuous'
      continuousModeProperties: {
        tier: 'Continuous7Days'
      }
    } : {
      type: 'Periodic'
      periodicModeProperties: {
        backupIntervalInMinutes: 240
        backupRetentionIntervalInHours: 8
        backupStorageRedundancy: 'Local'
      }
    }
    publicNetworkAccess: 'Enabled'
    enableAnalyticalStorage: false
    capabilities: []
  }
  tags: {
    environment: environment
    application: 'blend'
  }
}

// ── Database ──────────────────────────────────────────────────────────────────
resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-02-15-preview' = {
  parent: cosmosAccount
  name: 'blend'
  properties: {
    resource: {
      id: 'blend'
    }
    options: environment == 'dev'
      ? { throughput: 400 }
      : { autoscaleSettings: { maxThroughput: 4000 } }
  }
}

// ── Containers ────────────────────────────────────────────────────────────────
resource contentContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-02-15-preview' = {
  parent: database
  name: 'content'
  properties: {
    resource: {
      id: 'content'
      partitionKey: {
        paths: ['/contentType']
        kind: 'Hash'
      }
      defaultTtl: -1
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [{ path: '/*' }]
        excludedPaths: [{ path: '/"_etag"/?' }]
      }
    }
  }
}

resource cacheContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-02-15-preview' = {
  parent: database
  name: 'cache'
  properties: {
    resource: {
      id: 'cache'
      partitionKey: {
        paths: ['/pk']
        kind: 'Hash'
      }
      defaultTtl: 86400
    }
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────
output accountId string = cosmosAccount.id
output accountName string = cosmosAccount.name
output documentEndpoint string = cosmosAccount.properties.documentEndpoint
