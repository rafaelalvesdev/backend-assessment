targetScope = 'resourceGroup'

param location string = resourceGroup().location
param appName string = 'stores-api-${uniqueString(resourceGroup().id)}'
param sqlAdminLogin string = 'storesadmin'
@secure()
param sqlAdminPassword string

@allowed(['F1', 'B1', 'S1'])
param planSku string = 'F1'

var sqlServerName = 'sql-${appName}'
var databaseName = 'storesdb'

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

  resource database 'databases' = {
    name: databaseName
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
  }
}

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'plan-${appName}'
  location: location
  kind: 'linux'
  sku: { name: planSku }
  properties: { reserved: true }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: planSku != 'F1'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      healthCheckPath: planSku == 'F1' ? null : '/health'
      appSettings: [
        { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
        { name: 'Database__MigrateOnStartup', value: 'true' }
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: insights.properties.ConnectionString }
        { name: 'ApplicationInsightsAgent_EXTENSION_VERSION', value: '~3' }
      ]
      connectionStrings: [
        {
          name: 'StoresDb'
          type: 'SQLAzure'
          connectionString: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=${databaseName};User ID=${sqlAdminLogin};Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;'
        }
      ]
    }
  }
}

output webAppName string = webApp.name
output url string = 'https://${webApp.properties.defaultHostName}'
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
