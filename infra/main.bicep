targetScope = 'subscription'

//param location string = resourceGroup().location

param location string 
param storageAccount string
param skuName string
param appServicePlanName string
param skuCapacity int 
param functionAppName string
param resourceGroupName string
param containerName string



resource rg 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName
  location: location
}

// Deploying storage account using module
module stg './storage.bicep' = {
  name: 'storageDeployment'
  scope: resourceGroup(resourceGroupName)    // Deployed in the scope of resource group we created above
  params: {
    storageAccountName: storageAccount
    location:location
    containerName:containerName
  }
  dependsOn: [
    rg
  ]
}


output storageAccount object = stg.outputs.storage

module appService 'appservice.bicep' = {
  name:'appServiceDeployment'
  scope: resourceGroup(resourceGroupName)
  params: {
    location: location
    skuName: skuName
    appServicePlanName: appServicePlanName
    webSiteName: appServicePlanName
    skuCapacity: skuCapacity
    appName: appServicePlanName
    functionAppName: functionAppName
    storageAccount: stg.outputs.storage
  }
  dependsOn: [
    stg
  ]

}
