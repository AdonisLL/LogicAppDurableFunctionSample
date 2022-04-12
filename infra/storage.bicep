param location string = 'eastus'
param storageAccountName string = 'eastus'
param containerName string = 'logicappfiletransfer'


resource storageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: storageAccountName
  location:location
  sku: {
    name: 'Standard_LRS'
  }
  kind:'StorageV2'
  properties:{
    accessTier:'Hot'
  }
  
}

output storage object = {
  name:storageAccount.name
  apiVersion:storageAccount.apiVersion
  id:storageAccount.id
}


resource storageAccountsBlob 'Microsoft.Storage/storageAccounts/blobServices@2021-08-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
    deleteRetentionPolicy: {
      enabled: false
    }
  }
}

resource storageAccounts_sharepointtoblobstg_name_default_filetransfers 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-08-01' = {
  parent: storageAccountsBlob
  name: containerName
  properties: {
    immutableStorageWithVersioning: {
      enabled: false
    }
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }

}
