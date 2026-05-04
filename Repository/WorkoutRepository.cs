using Microsoft.Azure.Cosmos;
using WorkoutLogService;

namespace Repository;

public sealed class WorkoutRepository
{
    private readonly Container _container;

    public WorkoutRepository(Container container)
    {
        _container = container;
    }

    public async Task<WorkoutLog> LogWorkoutAsync(WorkoutLog log)
    {
        var response = await _container.CreateItemAsync(log, new PartitionKey(log.UserEmail));
        return response.Resource;
    }

    public async Task<List<WorkoutLog>> GetWorkoutsByEmailAsync(string email)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.UserEmail = @email ORDER BY c.LoggedAt DESC")
            .WithParameter("@email", email);

        var requestOptions = new QueryRequestOptions
        {
            PartitionKey = new PartitionKey(email)
        };

        var results = new List<WorkoutLog>();
        using var iterator = _container.GetItemQueryIterator<WorkoutLog>(query, requestOptions: requestOptions);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        return results;
    }
}
