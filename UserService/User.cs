using Newtonsoft.Json;

namespace UserService;

public sealed class User
{
    [JsonProperty("id")]
    public string Id { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string Name { get; set; } = default!;

    public static User Create(string name, string email) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = name,
        Email = email
    };
}
