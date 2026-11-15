#requires -Version 5.1
<#
.SYNOPSIS
  Deletes the resource group, then purges the soft-deleted Azure OpenAI account so
  the next deployment can reuse its name and quota.
#>
param(
    [string] $ResourceGroup = 'rg-nythorion',
    [string] $Location = 'swedencentral'
)

$ErrorActionPreference = 'Stop'

$accounts = az cognitiveservices account list --resource-group $ResourceGroup --query "[?kind=='OpenAI'].name" --output tsv

az group delete --name $ResourceGroup --yes

foreach ($account in $accounts) {
    az cognitiveservices account purge --name $account --resource-group $ResourceGroup --location $Location
}

$secretsFile = Join-Path $PSScriptRoot '.secrets.local.json'
if (Test-Path $secretsFile) { Remove-Item $secretsFile }
