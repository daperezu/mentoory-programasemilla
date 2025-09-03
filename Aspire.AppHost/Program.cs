using Aspire.Hosting.Azure;
using LinaSys.Aspire.AppHost;
using Projects;

const string defaultConnectionName = "DefaultConnection";

var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<IResourceWithConnectionString> dbDefaultConnection;
IResourceBuilder<AzureBlobStorageResource> blobs;

if (builder.ExecutionContext.IsRunMode)
{
    dbDefaultConnection = builder.AddConnectionString(defaultConnectionName);
    var storage = builder.AddAzureStorage("storage")
        .RunAsEmulator();
    blobs = storage.AddBlobs("blobs");
}
else
{
    dbDefaultConnection = builder
        .AddAzureSqlServer(name: "lina-dbserver")
        .AddDatabase(name: defaultConnectionName, databaseName: "LinaDb");

    var storage = builder.AddAzureStorage("lina-storage");
    blobs = storage.AddBlobs("blobs");
}

var appSettingsJson = AppsettingsLoader.SerializeUserJsonConfiguration();

builder.AddProject<LinaSys_Web>("lina-web")
    .WithReference(dbDefaultConnection)
    .WithReference(blobs)
    .WithEnvironment("AspireAppsettings", appSettingsJson)
    .WithExternalHttpEndpoints()
    .WaitFor(dbDefaultConnection);

builder.Build().Run();
