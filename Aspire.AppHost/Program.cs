using Aspire.Hosting.Azure;
using LinaSys.Aspire.AppHost;
using Projects;

const string defaultConnectionName = "DefaultConnection";

IResourceBuilder<IResourceWithConnectionString> dbDefaultConnection;
IResourceBuilder<AzureBlobStorageResource> blobs;
IResourceBuilder<ParameterResource>? mailgunDomain = null;
IResourceBuilder<ParameterResource>? mailgunApiKey = null;

var builder = DistributedApplication.CreateBuilder(args);

if (builder.ExecutionContext.IsRunMode) //// Running locally
{
    dbDefaultConnection = builder.AddConnectionString(defaultConnectionName);

    var storage = builder
        .AddAzureStorage("storage")
        .RunAsEmulator();
    blobs = storage.AddBlobs("blobs");
}
else //// Running in Azure
{
    dbDefaultConnection = builder
        .AddAzureSqlServer(name: "lina-dbserver")
        .AddDatabase(name: defaultConnectionName, databaseName: "LinaDb");

    var storage = builder.AddAzureStorage("lina-storage");
    blobs = storage.AddBlobs("blobs");

    mailgunDomain = builder.AddParameter("mailgun-domain");
    mailgunApiKey = builder.AddParameter("mailgun-apikey", secret: true);
}

var appSettingsJson = AppsettingsLoader.SerializeUserJsonConfiguration(builder.ExecutionContext.IsRunMode, out var appSettingsJsonHash);

var webProject = builder.AddProject<LinaSys_Web>("lina-web")
    .WithReference(dbDefaultConnection)
    .WithReference(blobs)
    .WithEnvironment("AspireAppsettings", appSettingsJson)
    .WithEnvironment("APPSETTINGS_HASH", appSettingsJsonHash) // <— forces a spec change
    .WithExternalHttpEndpoints()
    .WaitFor(dbDefaultConnection);

if (mailgunDomain != null && mailgunApiKey != null)
{
    webProject
        .WithEnvironment("Mailgun__Domain", mailgunDomain)
        .WithEnvironment("Mailgun__ApiKey", mailgunApiKey);
}

builder.Build().Run();
