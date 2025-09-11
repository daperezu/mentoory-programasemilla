@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource sqlServerAdminManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: take('lina_dbserver-admin-${uniqueString(resourceGroup().id)}', 63)
  location: location
}

resource lina_dbserver 'Microsoft.Sql/servers@2023-08-01' = {
  name: take('linadbserver-${uniqueString(resourceGroup().id)}', 63)
  location: location
  properties: {
    administrators: {
      administratorType: 'ActiveDirectory'
      login: sqlServerAdminManagedIdentity.name
      sid: sqlServerAdminManagedIdentity.properties.principalId
      tenantId: subscription().tenantId
      azureADOnlyAuthentication: false
    }
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    version: '12.0'
  }
  tags: {
    'aspire-resource-name': 'lina-dbserver'
  }
}

resource sqlFirewallRule_AllowAllAzureIps 'Microsoft.Sql/servers/firewallRules@2023-08-01' = {
  name: 'AllowAllAzureIps'
  properties: {
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
  parent: lina_dbserver
}

resource DefaultConnection 'Microsoft.Sql/servers/databases@2023-08-01' = {
  name: 'LinaDb'
  location: location
  properties: {
    freeLimitExhaustionBehavior: 'BillOverUsage'
    useFreeLimit: true
  }
  sku: {
    name: 'GP_S_Gen5_2'
  }
  parent: lina_dbserver
}

output sqlServerFqdn string = lina_dbserver.properties.fullyQualifiedDomainName

output name string = lina_dbserver.name

output sqlServerAdminName string = sqlServerAdminManagedIdentity.name