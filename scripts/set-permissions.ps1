#!/bin/bash

# Function app and storage account names must be unique.
#Get a random number between 100 and 300 to more easily be able to distinguish between several trials
$rand = Get-Random -Minimum 300 -Maximum 500

$token = Get-AzAccessToken -ResourceUrl "https://graph.microsoft.com"
Connect-MgGraph -AccessToken $token.Token


#Set values
$resourceGroup = "$($env:RESOURCE_GROUP_NAME)"
$storage = "$($env:STORAGE_NAME)" 
$functionapp = "$($env:FUNCTION_APP_NAME)"
$region="$($env:REGION)"
$containerName = "$($env:CONTAINER_NAME)"


#Set App Config Variables
Write-Host "Set App Config Variables" -ForegroundColor Green
$storageUri = az storage account list --query "[?name=='$storage'].[primaryEndpoints.blob]" --output tsv
az webapp config appsettings set -g $resourceGroup -n $functionapp --settings BlobConfiguration:ContainerName=$containerName
az webapp config appsettings set -g $resourceGroup -n $functionapp --settings BlobConfiguration:StorageUri=$storageUri


#Get Graph Api service provider 
Write-Host "Get Graph Api service provider " -BackgroundColor Green
$graphspurl = "https://graph.microsoft.com/v1.0/servicePrincipals?$filter=displayName eq 'Microsoft Graph'&$select=appId"

Write-Host "Get Graph Id" -BackgroundColor Green
#Set values
Write-Host "Set Graph Resource Values" -BackgroundColor Green
$webAppName = $functionapp
$principalId = $(az resource list -n $webAppName --query [*].identity.principalId --out tsv)
$graphResourceId = (Get-AzADServicePrincipal -DisplayName "Microsoft Graph" -Select "Id").Id


#Get appRoleIds for Sites.ReadWrite.All
Write-Host "Get Graph Role Ids" -BackgroundColor Green
$appRoleId = (((Get-MgServicePrincipal -Filter "DisplayName eq 'Microsoft Graph'").AppRoles) | Where-Object {$_.Value -eq 'Sites.ReadWrite.All'} | Select-Object Id).Id


$body = "{'principalId':'$principalId','resourceId':'$graphResourceId','appRoleId':'$appRoleId'}"
$uri = "https://graph.microsoft.com/v1.0/servicePrincipals/$principalId/appRoleAssignments"


Write-Host "Call Graph to set permissions" -BackgroundColor Green
az rest --method post --uri $uri --body $body --headers Content-Type=application/json

Write-Host "Set Web App as Blob Contributor" -BackgroundColor Green
$roleId = az role definition list --name "Storage Blob Data Contributor" --query [0].id
$storageResourceId = az storage account show --resource-group $resourceGroup --name $storage --query id

Write-Host "Assigning Role Permissions"
az role assignment create --assignee $principalId `
--role $roleId `
--scope $storageResourceId

# az webapp cors add -g $resourceGroup -n $functionapp --allowed-origins '*'

# $StorageUri = "https://$storage.blob.core.windows.net"
# az webapp cors add -g $resourceGroup -n $functionapp --allowed-origins '*'
# az webapp config appsettings set -g $resourceGroup -n $functionapp --settings StorageUri=$StorageUri
# az webapp config appsettings set -g $resourceGroup -n $functionapp --settings ContainerName="sharepointfiletransfer"

