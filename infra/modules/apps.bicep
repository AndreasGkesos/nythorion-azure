param location string
param environmentName string
param logAnalyticsName string
param apiAppName string
param authAppName string

param apiImage string
param authImage string

param postgresFqdn string
param postgresLogin string

@secure()
param postgresPassword string

param openAiEndpoint string

@description('Public base URL of the Angular app, for CORS and the OAuth redirect URIs.')
param spaUrl string

param seedUsername string

@secure()
param seedPassword string

var containerPort = 8080
var authUrl = 'https://${authAppName}.${environment.properties.defaultDomain}'

resource logs 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    // Keeps a runaway log stream from using up the credit
    workspaceCapping: {
      dailyQuotaGb: json('0.1')
    }
  }
}

resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: environmentName
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

resource auth 'Microsoft.App/containerApps@2024-03-01' = {
  name: authAppName
  location: location
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      ingress: {
        external: true
        targetPort: containerPort
        transport: 'auto'
      }
      secrets: [
        { name: 'database-connection', value: 'Host=${postgresFqdn};Database=nythorion_auth;Username=${postgresLogin};Password=${postgresPassword};Ssl Mode=Require' }
        { name: 'seed-password', value: seedPassword }
      ]
    }
    template: {
      containers: [
        {
          name: 'auth'
          image: authImage
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            { name: 'ConnectionStrings__Default', secretRef: 'database-connection' }
            { name: 'Spa__BaseUrl', value: spaUrl }
            { name: 'Cors__AllowedOrigins__0', value: spaUrl }
            { name: 'Registration__Enabled', value: 'false' }
            { name: 'SeedUsers__0__Username', value: seedUsername }
            { name: 'SeedUsers__0__Password', secretRef: 'seed-password' }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: { path: '/health', port: containerPort }
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}

resource api 'Microsoft.App/containerApps@2024-03-01' = {
  name: apiAppName
  location: location
  // Calls Azure OpenAI with this identity, no keys involved
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      ingress: {
        external: true
        targetPort: containerPort
        transport: 'auto'
      }
      secrets: [
        { name: 'database-connection', value: 'Host=${postgresFqdn};Database=nythorion;Username=${postgresLogin};Password=${postgresPassword};Ssl Mode=Require' }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: apiImage
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            { name: 'ConnectionStrings__Default', secretRef: 'database-connection' }
            { name: 'Auth__Issuer', value: authUrl }
            { name: 'Cors__AllowedOrigins__0', value: spaUrl }
            { name: 'AzureOpenAI__Endpoint', value: openAiEndpoint }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: { path: '/health', port: containerPort }
            }
          ]
        }
      ]
      scale: {
        // The document queue lives in memory, so exactly one always-on replica
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}

output apiUrl string = 'https://${api.properties.configuration.ingress.fqdn}'
output authUrl string = 'https://${auth.properties.configuration.ingress.fqdn}'
output apiPrincipalId string = api.identity.principalId
