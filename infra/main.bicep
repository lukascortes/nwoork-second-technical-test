// ---------------------------------------------------------------------------
// TimeOff Manager — Azure infrastructure (Container Apps + Service Bus)
//
// Provisions the cloud compute + messaging for the API:
//   - Log Analytics workspace (required by the Container Apps environment)
//   - Container Apps environment + the API container app (scale-to-zero)
//   - Service Bus namespace (Basic) + the email-notifications queue
//
// The database is intentionally NOT created here: point ConnectionStrings:Default
// at a free managed Postgres (e.g. Neon / Supabase) to keep the demo at ~$0.
//
// Deploy:
//   az group create -n timeoff-rg -l eastus2
//   az deployment group create -g timeoff-rg -f infra/main.bicep \
//     -p namePrefix=timeoff containerImage='ghcr.io/lukascortes/timeoff-api:latest' \
//        jwtKey='<32+ char secret>' dbConnectionString='<neon connection string>' \
//        corsOrigin='https://<your-frontend>'
// ---------------------------------------------------------------------------

@description('Resource name prefix.')
param namePrefix string = 'timeoff'

@description('Azure region.')
param location string = resourceGroup().location

@description('Public container image for the API, e.g. ghcr.io/<user>/timeoff-api:latest')
param containerImage string

@description('JWT signing key (>= 32 chars).')
@secure()
param jwtKey string

@description('PostgreSQL connection string (e.g. a Neon/Supabase database).')
@secure()
param dbConnectionString string

@description('Allowed CORS origin for the frontend (e.g. your Vercel URL).')
param corsOrigin string = 'http://localhost:5173'

var queueName = 'email-notifications'

resource logs 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${namePrefix}-logs'
  location: location
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: '${namePrefix}-sb-${uniqueString(resourceGroup().id)}'
  location: location
  sku: { name: 'Basic', tier: 'Basic' }
}

resource queue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBus
  name: queueName
}

resource sbRootRule 'Microsoft.ServiceBus/namespaces/authorizationRules@2022-10-01-preview' existing = {
  parent: serviceBus
  name: 'RootManageSharedAccessKey'
}

resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${namePrefix}-env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logs.properties.customerId
        sharedKey: logs.listKeys().primarySharedKey
      }
    }
  }
}

resource api 'Microsoft.App/containerApps@2024-03-01' = {
  name: '${namePrefix}-api'
  location: location
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
      }
      secrets: [
        { name: 'jwt-key', value: jwtKey }
        { name: 'db-connection', value: dbConnectionString }
        { name: 'servicebus-connection', value: sbRootRule.listKeys().primaryConnectionString }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: containerImage
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
            { name: 'ASPNETCORE_URLS', value: 'http://+:8080' }
            { name: 'Jwt__Key', secretRef: 'jwt-key' }
            { name: 'Jwt__Issuer', value: 'TimeOffManager' }
            { name: 'Jwt__Audience', value: 'TimeOffManager.Clients' }
            { name: 'ConnectionStrings__Default', secretRef: 'db-connection' }
            { name: 'Messaging__Provider', value: 'AzureServiceBus' }
            { name: 'ServiceBus__ConnectionString', secretRef: 'servicebus-connection' }
            { name: 'ServiceBus__QueueName', value: queueName }
            { name: 'Email__Provider', value: 'Logging' }
            { name: 'Cors__AllowedOrigins__0', value: corsOrigin }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 2
      }
    }
  }
  dependsOn: [
    queue
  ]
}

@description('Public HTTPS URL of the deployed API.')
output apiUrl string = 'https://${api.properties.configuration.ingress.fqdn}'
