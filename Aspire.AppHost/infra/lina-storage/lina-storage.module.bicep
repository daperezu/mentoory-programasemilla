@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource lina_storage 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: take('linastorage${uniqueString(resourceGroup().id)}', 24)
  kind: 'StorageV2'
  location: location
  sku: {
    name: 'Standard_GRS'
  }
  properties: {
    accessTier: 'Hot'
    allowSharedKeyAccess: false
    minimumTlsVersion: 'TLS1_2'
    networkAcls: {
      defaultAction: 'Allow'
    }
  }
  tags: {
    'aspire-resource-name': 'lina-storage'
  }
}

output blobEndpoint string = lina_storage.properties.primaryEndpoints.blob

output queueEndpoint string = lina_storage.properties.primaryEndpoints.queue

output tableEndpoint string = lina_storage.properties.primaryEndpoints.table

output name string = lina_storage.name