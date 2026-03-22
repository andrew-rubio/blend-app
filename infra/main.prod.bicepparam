// ──────────────────────────────────────────────────────────────────────────────
// main.prod.bicepparam — Production environment parameters
// ──────────────────────────────────────────────────────────────────────────────
using './main.bicep'

param namePrefix = 'blend-prod'
param location = 'australiaeast'
param environment = 'prod'
// imageTag is injected at deploy time via --parameters apiImageTag=<sha>
param apiImageTag = 'latest'
