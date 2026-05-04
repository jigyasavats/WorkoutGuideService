using Microsoft.Azure.Cosmos;

namespace Repository;

public sealed class UserRepository
{
    private readonly Container _container;

    public UserRepository(Container container)
    {
        _container = container;
    }

    public async Task<UserService.User> CreateUserAsync(UserService.User user)
    {
        var response = await _container.CreateItemAsync(user, new PartitionKey(user.Email));
        return response.Resource;
    }

    public async Task<UserService.User?> GetUserByEmailAsync(string email)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.Email = @email")
            .WithParameter("@email", email);

        var requestOptions = new QueryRequestOptions
        {
            PartitionKey = new PartitionKey(email)
        };

        using var iterator = _container.GetItemQueryIterator<UserService.User>(query, requestOptions: requestOptions);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            if (response.Resource.Any())
            {
                return response.Resource.First();
            }
        }

        return null;
    }
}
