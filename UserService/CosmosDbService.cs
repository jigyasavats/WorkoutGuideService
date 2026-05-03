using Microsoft.Azure.Cosmos;

namespace UserService;

public sealed class CosmosDbService
{
    public CosmosClient Client { get; }
    public Database Database { get; }
    public Container Container { get; }

    public const string PartitionKeyPath = "/Email";

    private CosmosDbService(CosmosClient client, Database database, Container container)
    {
        Client = client;
        Database = database;
        Container = container;
    }

    public static async Task<CosmosDbService> CreateAsync(string connectionString, string databaseId, string containerId)
    {
        var client = new CosmosClient(connectionString);
        var databaseResponse = await client.CreateDatabaseIfNotExistsAsync(databaseId);
        var containerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(containerId, PartitionKeyPath);

        return new CosmosDbService(client, databaseResponse.Database, containerResponse.Container);
    }
}
