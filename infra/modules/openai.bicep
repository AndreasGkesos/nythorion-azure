@description('Name of the Azure OpenAI account. Also used as the custom subdomain.')
param name string
param location string

// These names are what the Api reads from AzureOpenAI:ChatDeployment and AzureOpenAI:EmbeddingDeployment
var chatDeploymentName = 'gpt-4.1-mini'
var embeddingDeploymentName = 'text-embedding-3-small'

resource account 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: name
  location: location
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName: name
    publicNetworkAccess: 'Enabled'
    // Managed identity only, no account keys
    disableLocalAuth: true
  }
}

resource chat 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: account
  name: chatDeploymentName
  sku: {
    name: 'GlobalStandard'
    capacity: 10
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4.1-mini'
      version: '2025-04-14'
    }
  }
}

// Deployments on one account must be created one after the other
resource embeddings 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: account
  name: embeddingDeploymentName
  dependsOn: [chat]
  sku: {
    name: 'GlobalStandard'
    capacity: 10
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'text-embedding-3-small'
      version: '1'
    }
  }
}

output endpoint string = account.properties.endpoint
output accountName string = account.name
