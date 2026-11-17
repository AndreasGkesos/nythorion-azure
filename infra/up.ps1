#requires -Version 5.1
<#
.SYNOPSIS
  Creates the resource group and deploys infra/main.bicep.
.EXAMPLE
  ./infra/up.ps1 -ApiImage ghcr.io/me/nythorion-api:abc123 -AuthImage ghcr.io/me/nythorion-auth:abc123
#>
param(
    [Parameter(Mandatory)] [string] $ApiImage,
    [Parameter(Mandatory)] [string] $AuthImage,
    [string] $ResourceGroup = 'rg-nythorion',
    [string] $Location = 'swedencentral',
    [string] $SeedUsername = 'demo',
    [string] $ClientIp = '',
    [switch] $WhatIf
)

# Native stderr output (CLI warnings) must not abort the script, so exit codes are checked instead
function Assert-Success([string] $step) {
    if ($LASTEXITCODE -ne 0) { throw "$step failed with exit code $LASTEXITCODE" }
}

function New-RandomPassword {
    $chars = (48..57) + (65..90) + (97..122)
    -join ($chars | Get-Random -Count 24 | ForEach-Object { [char]$_ })
}

# Passwords are generated per run and written to a gitignored file next to this script
$secretsFile = Join-Path $PSScriptRoot '.secrets.local.json'
if (Test-Path $secretsFile) {
    $secrets = Get-Content $secretsFile -Raw | ConvertFrom-Json
} else {
    $secrets = [pscustomobject]@{ postgresPassword = New-RandomPassword; seedPassword = New-RandomPassword }
    $secrets | ConvertTo-Json | Set-Content $secretsFile -Encoding utf8
}

az group create --name $ResourceGroup --location $Location --output none
Assert-Success 'Creating the resource group'

$deployment = @(
    'deployment', 'group', $(if ($WhatIf) { 'what-if' } else { 'create' }),
    '--resource-group', $ResourceGroup,
    '--name', 'nythorion',
    '--template-file', (Join-Path $PSScriptRoot 'main.bicep'),
    '--parameters',
    "apiImage=$ApiImage",
    "authImage=$AuthImage",
    "seedUsername=$SeedUsername",
    "postgresPassword=$($secrets.postgresPassword)",
    "seedPassword=$($secrets.seedPassword)",
    "clientIp=$ClientIp"
)

az @deployment
Assert-Success 'The deployment'
