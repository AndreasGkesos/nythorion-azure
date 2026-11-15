targetScope = 'resourceGroup'

@description('Region for Azure OpenAI, Container Apps and logs.')
param location string = resourceGroup().location

@description('Region for PostgreSQL. Separate because trial subscriptions allow only some regions.')
param postgresLocation string = 'denmarkeast'

@description('Region for the Static Web App metadata. The site itself is served globally.')
param staticWebAppLocation string = 'centralus'

param namePrefix string = 'nyth'

@description('Full image references, for example ghcr.io/<owner>/nythorion-api:<commit sha>.')
param apiImage string
param authImage string

param postgresLogin string = 'nythadmin'

@secure()
param postgresPassword string

param seedUsername string

@secure()
param seedPassword string

@description('Optional public IP that may connect to PostgreSQL directly, for example to run psql.')
param clientIp string = ''

@description('Optional object ID of a person who may call Azure OpenAI from their own machine.')
param developerPrincipalId string = ''

var suffix = uniqueString(resourceGroup().id)

module openai 'modules/openai.bicep' = {
  name: 'openai'
  params: {
    name: 'oai-${namePrefix}-${suffix}'
    location: location
  }
}

module postgres 'modules/postgres.bicep' = {
  name: 'postgres'
  params: {
    name: 'pg-${namePrefix}-${suffix}'
    location: postgresLocation
    administratorLogin: postgresLogin
    administratorPassword: postgresPassword
    clientIp: clientIp
  }
}

module web 'modules/web.bicep' = {
  name: 'web'
  params: {
    name: 'swa-${namePrefix}-${suffix}'
    location: staticWebAppLocation
  }
}

module apps 'modules/apps.bicep' = {
  name: 'apps'
  params: {
    location: location
    environmentName: 'cae-${namePrefix}-${suffix}'
    logAnalyticsName: 'log-${namePrefix}-${suffix}'
    apiAppName: 'ca-${namePrefix}-api'
    authAppName: 'ca-${namePrefix}-auth'
    apiImage: apiImage
    authImage: authImage
    postgresFqdn: postgres.outputs.fqdn
    postgresLogin: postgresLogin
    postgresPassword: postgresPassword
    openAiEndpoint: openai.outputs.endpoint
    spaUrl: 'https://${web.outputs.hostname}'
    seedUsername: seedUsername
    seedPassword: seedPassword
  }
}

module apiOpenAiAccess 'modules/openai-access.bicep' = {
  name: 'api-openai-access'
  params: {
    accountName: openai.outputs.accountName
    principalId: apps.outputs.apiPrincipalId
  }
}

module developerOpenAiAccess 'modules/openai-access.bicep' = if (!empty(developerPrincipalId)) {
  name: 'developer-openai-access'
  params: {
    accountName: openai.outputs.accountName
    principalId: developerPrincipalId
    principalType: 'User'
  }
}

output apiUrl string = apps.outputs.apiUrl
output authUrl string = apps.outputs.authUrl
output webUrl string = 'https://${web.outputs.hostname}'
output staticWebAppName string = web.outputs.name
output openAiAccountName string = openai.outputs.accountName
output postgresFqdn string = postgres.outputs.fqdn
