param skuName string 
param skuCapacity int
param location string 
param appName string
param storageAccount object
param appServicePlanName string
param webSiteName string
param functionAppName string
var appInsightsName = '${appName}${uniqueString(resourceGroup().id)}'




resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: { 
    Application_Type: 'web'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
  tags: {
    // circular dependency means we can't reference functionApp directly  /subscriptions/<subscriptionId>/resourceGroups/<rg-name>/providers/Microsoft.Web/sites/<appName>"
     'hidden-link:/subscriptions/${subscription().id}/resourceGroups/${resourceGroup().name}/providers/Microsoft.Web/sites/${functionAppName}': 'Resource'
  }
  dependsOn:[
    
  ]
}

resource appServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: appServicePlanName // app serivce plan name
  location: location // Azure Region
  sku: {
    name: skuName
    capacity: skuCapacity
  }
  tags: {
    displayName: 'HostingPlan'
    ProjectName: appName
    storage: storageAccount.name
  }
}

resource appService 'Microsoft.Web/sites@2021-03-01' = {
  name: webSiteName // Globally unique app serivce name
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  tags: {
    displayName: 'Website'
    ProjectName: appName
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      minTlsVersion: '1.2'
      cors: {
         allowedOrigins: [
          '*'
         ]
      }
      apiDefinition: {
         url:'https://${functionAppName}.azurewebsites.net/api/swagger.json'
      }
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
        }
        // WEBSITE_CONTENTSHARE will also be auto-generated - https://docs.microsoft.com/en-us/azure/azure-functions/functions-app-settings#website_contentshare
        // WEBSITE_RUN_FROM_PACKAGE will be set to 1 by func azure functionapp publish
      ]
    }
  }
}

// resource srcControls 'Microsoft.Web/sites/sourcecontrols@2021-03-01' = {
//   name: appService.name
//   properties: {
//     repoUrl: repositoryUrl
//     branch: branch
//     isGitHubAction:true
//   }
// }



