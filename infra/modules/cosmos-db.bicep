// Azure Cosmos DB (NoSQL API)
// Primary database for Blend — recipes, users, preferences.

@description('Azure region.')
param location string

@description('Environment name.')
param environmentName string

@description('Application name.')
param appName string

// ── Cosmos DB Account ─────────────────────────────────────────────────────────
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: 'cosmos-${appName}-${environmentName}'
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    databaseAccountOfferType: 'Standard'
    enableAutomaticFailover: environmentName == 'production'
    enableMultipleWriteLocations: false
    publicNetworkAccess: 'Enabled'
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: environmentName == 'production'
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    backupPolicy: {
      type: environmentName == 'production' ? 'Continuous' : 'Periodic'
    }
  }
  tags: {
    application: appName
    environment: environmentName
  }
}

// ── Database ──────────────────────────────────────────────────────────────────
resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: cosmosAccount
  name: 'Blend'
  properties: {
    resource: {
      id: 'Blend'
    }
  }
}

// ── Containers ────────────────────────────────────────────────────────────────
var containers = [
  { name: 'users', partitionKey: '/id' }
  { name: 'recipes', partitionKey: '/userId' }
  { name: 'preferences', partitionKey: '/userId' }
  { name: 'friends', partitionKey: '/userId' }
  { name: 'notifications', partitionKey: '/userId' }
]

resource cosmosContainers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = [
  for container in containers: {
    parent: database
    name: container.name
    properties: {
      resource: {
        id: container.name
        partitionKey: {
          paths: [container.partitionKey]
          kind: 'Hash'
        }
        indexingPolicy: {
          automatic: true
          indexingMode: 'consistent'
        }
      }
    }
  }
]

// ── Outputs ───────────────────────────────────────────────────────────────────
output connectionString string = cosmosAccount.listConnectionStrings().connectionStrings[0].connectionString
output accountName string = cosmosAccount.name
output accountId string = cosmosAccount.id
