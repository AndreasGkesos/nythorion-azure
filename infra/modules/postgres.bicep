param name string
param location string
param administratorLogin string

@secure()
param administratorPassword string

@description('Optional public IP that may connect, for example your own machine. Empty adds no rule.')
param clientIp string = ''

resource server 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: name
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
  }
}

// Server-level changes on one flexible server must run one after the other
resource mainDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: server
  name: 'nythorion'
}

resource authDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: server
  name: 'nythorion_auth'
  dependsOn: [mainDatabase]
}

// pgvector must be allowlisted before the Api migration runs CREATE EXTENSION vector
resource extensions 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2024-08-01' = {
  parent: server
  name: 'azure.extensions'
  dependsOn: [authDatabase]
  properties: {
    value: 'VECTOR'
    source: 'user-override'
  }
}

// 0.0.0.0 means "Azure services": the Container Apps connect from changing Azure addresses
resource allowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = {
  parent: server
  name: 'allow-azure-services'
  dependsOn: [extensions]
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource allowClient 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = if (!empty(clientIp)) {
  parent: server
  name: 'allow-client'
  dependsOn: [allowAzureServices]
  properties: {
    startIpAddress: clientIp
    endIpAddress: clientIp
  }
}

output fqdn string = server.properties.fullyQualifiedDomainName
output administratorLogin string = administratorLogin
