targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention, the name of the resource group for your application will use this name, prefixed with rg-')
param environmentName string

@minLength(1)
@description('The location used for all deployed resources')
param location string

@description('Id of the user or app to assign application roles')
param principalId string = ''

param applicationinsights_samplingpercentage string
param googleanalytics_enabled string
@secure()
param googleanalytics_measurementid string
@secure()
param googlemaps_apikey string
@secure()
param mailgun_apikey string
param mailgun_domain string

var tags = {
  'azd-env-name': environmentName
}

resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}
module resources 'resources.bicep' = {
  scope: rg
  name: 'resources'
  params: {
    location: location
    tags: tags
    principalId: principalId
  }
}

module lina_dbserver 'lina-dbserver/lina-dbserver.module.bicep' = {
  name: 'lina-dbserver'
  scope: rg
  params: {
    location: location
  }
}
module lina_dbserver_roles 'lina-dbserver-roles/lina-dbserver-roles.module.bicep' = {
  name: 'lina-dbserver-roles'
  scope: rg
  params: {
    lina_dbserver_outputs_name: lina_dbserver.outputs.name
    lina_dbserver_outputs_sqlserveradminname: lina_dbserver.outputs.sqlServerAdminName
    location: location
    principalId: resources.outputs.MANAGED_IDENTITY_PRINCIPAL_ID
    principalName: resources.outputs.MANAGED_IDENTITY_NAME
    principalType: 'ServicePrincipal'
  }
}
module lina_storage 'lina-storage/lina-storage.module.bicep' = {
  name: 'lina-storage'
  scope: rg
  params: {
    location: location
  }
}
module lina_storage_roles 'lina-storage-roles/lina-storage-roles.module.bicep' = {
  name: 'lina-storage-roles'
  scope: rg
  params: {
    lina_storage_outputs_name: lina_storage.outputs.name
    location: location
    principalId: resources.outputs.MANAGED_IDENTITY_PRINCIPAL_ID
    principalType: 'ServicePrincipal'
  }
}

output MANAGED_IDENTITY_CLIENT_ID string = resources.outputs.MANAGED_IDENTITY_CLIENT_ID
output MANAGED_IDENTITY_NAME string = resources.outputs.MANAGED_IDENTITY_NAME
output AZURE_LOG_ANALYTICS_WORKSPACE_NAME string = resources.outputs.AZURE_LOG_ANALYTICS_WORKSPACE_NAME
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = resources.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
output AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = resources.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID
output AZURE_CONTAINER_REGISTRY_NAME string = resources.outputs.AZURE_CONTAINER_REGISTRY_NAME
output AZURE_CONTAINER_APPS_ENVIRONMENT_NAME string = resources.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_NAME
output AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = resources.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID
output AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = resources.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
output AZURE_APPLICATION_INSIGHTS_CONNECTION_STRING string = resources.outputs.AZURE_APPLICATION_INSIGHTS_CONNECTION_STRING
output LINA_DBSERVER_SQLSERVERFQDN string = lina_dbserver.outputs.sqlServerFqdn
output LINA_STORAGE_BLOBENDPOINT string = lina_storage.outputs.blobEndpoint
