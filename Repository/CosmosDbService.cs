using Microsoft.Azure.Cosmos;

namespace Repository;

public sealed class CosmosDbService
{
    public CosmosClient Client { get; }
    public Database Database { get; }

    private CosmosDbService(CosmosClient client, Database database)
    {
        Client = client;
        Database = database;
    }

    public static async Task<CosmosDbService> CreateAsync(string connectionString, string databaseId)
    {
        var client = new CosmosClient(connectionString);
        var databaseResponse = await client.CreateDatabaseIfNotExistsAsync(databaseId);

        return new CosmosDbService(client, databaseResponse.Database);
    }
}
