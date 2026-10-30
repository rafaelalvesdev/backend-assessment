targetScope = 'resourceGroup'

param location string = resourceGroup().location
param appName string = 'products-func-${uniqueString(resourceGroup().id)}'
param sqlAdminLogin string = 'productsadmin'
@secure()
param sqlAdminPassword string

// Set by deploy.sh so re-provisioning doesn't drop the package each slot is currently running.
@secure()
param productionPackageUrl string = ''
@secure()
param stagingPackageUrl string = ''

var sqlServerName = 'sql-${appName}'
var storageName = take('st${uniqueString(resourceGroup().id, appName)}', 24)
var environments = {
  production: 'productsdb'
  staging: 'productsdb-staging'
}

resource logs 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'log-${appName}'
  location: location
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

resource insights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-${appName}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logs.id
  }
}

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageName
  location: location
  kind: 'StorageV2'
  sku: { name: 'Standard_LRS' }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }

  resource blobs 'blobServices' = {
    name: 'default'

    resource packages 'containers' = {
      name: 'function-releases'
    }
  }
}

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
  }

  resource allowAzure 'firewallRules' = {
    name: 'AllowAzureServices'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '0.0.0.0'
    }
  }
}

resource databases 'Microsoft.Sql/servers/databases@2023-08-01-preview' = [for env in items(environments): {
  parent: sqlServer
  name: env.value
  location: location
  sku: {
    name: 'GP_S_Gen5_2'
    tier: 'GeneralPurpose'
  }
  properties: {
    useFreeLimit: true
    freeLimitExhaustionBehavior: 'AutoPause'
    autoPauseDelay: 60
    minCapacity: json('0.5')
  }
}]

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'plan-${appName}'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
}

var storageConnection = 'DefaultEndpointsProtocol=https;AccountName=${storage.name};AccountKey=${storage.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'

func siteConfig(slot string, packageUrl string, database string, storageConnection string, insightsConnection string, sqlServer string, login string, password string) object => {
  netFrameworkVersion: 'v10.0'
  use32BitWorkerProcess: false
  ftpsState: 'Disabled'
  minTlsVersion: '1.2'
  appSettings: concat([
    { name: 'AzureWebJobsStorage', value: storageConnection }
    { name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING', value: storageConnection }
    { name: 'WEBSITE_CONTENTSHARE', value: toLower('products-${slot}') }
    { name: 'FUNCTIONS_EXTENSION_VERSION', value: '~4' }
    { name: 'FUNCTIONS_WORKER_RUNTIME', value: 'dotnet-isolated' }
    { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: insightsConnection }
    { name: 'SLOT_NAME', value: slot }
  ], empty(packageUrl) ? [] : [
    { name: 'WEBSITE_RUN_FROM_PACKAGE', value: packageUrl }
  ])
  connectionStrings: [
    {
      name: 'ProductsDb'
      type: 'SQLAzure'
      connectionString: 'Server=tcp:${sqlServer},1433;Database=${database};User ID=${login};Password=${password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;'
    }
  ]
}

resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: siteConfig('production', productionPackageUrl, environments.production, storageConnection, insights.properties.ConnectionString, sqlServer.properties.fullyQualifiedDomainName, sqlAdminLogin, sqlAdminPassword)
  }
  dependsOn: [databases]
}

resource staging 'Microsoft.Web/sites/slots@2023-12-01' = {
  parent: functionApp
  name: 'staging'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: siteConfig('staging', stagingPackageUrl, environments.staging, storageConnection, insights.properties.ConnectionString, sqlServer.properties.fullyQualifiedDomainName, sqlAdminLogin, sqlAdminPassword)
  }
}

// Each slot keeps its own database when the slots are swapped.
resource stickySettings 'Microsoft.Web/sites/config@2023-12-01' = {
  parent: functionApp
  name: 'slotConfigNames'
  properties: {
    connectionStringNames: ['ProductsDb']
    appSettingNames: ['SLOT_NAME']
  }
}

output functionAppName string = functionApp.name
output productionUrl string = 'https://${functionApp.properties.defaultHostName}'
output stagingUrl string = 'https://${staging.properties.defaultHostName}'
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output storageAccountName string = storage.name
