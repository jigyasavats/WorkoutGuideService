using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using UserService;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var vaultUri = config["KeyVault:VaultUri"];
var secretName = config["CosmosDb:ConnectionStringSecretName"];
var databaseId = config["CosmosDb:DatabaseId"];
var containerId = config["CosmosDb:ContainerId"];

if (string.IsNullOrWhiteSpace(vaultUri)
    || string.IsNullOrWhiteSpace(secretName)
    || string.IsNullOrWhiteSpace(databaseId)
    || string.IsNullOrWhiteSpace(containerId))
{
    Console.WriteLine("Key Vault or Cosmos DB configuration is missing. Please update appsettings.json.");
    return;
}

var secretClient = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential());
var secret = await secretClient.GetSecretAsync(secretName);
var cosmosConnectionString = secret.Value.Value;

if (string.IsNullOrWhiteSpace(cosmosConnectionString))
{
    Console.WriteLine("Cosmos DB connection string secret is empty.");
    return;
}

var cosmosService = await CosmosDbService.CreateAsync(cosmosConnectionString, databaseId, containerId);
var userRepository = new UserRepository(cosmosService.Container);

Console.WriteLine("✓ Cosmos DB connection initialized.\n");

bool running = true;

while (running)
{
    Console.WriteLine("=== Workout Guide System ===");
    Console.WriteLine("1. Add a new user");
    Console.WriteLine("2. Get user profile");
    Console.WriteLine("3. Log workout details");
    Console.WriteLine("4. View progress");
    Console.WriteLine("5. Check readiness for next level");
    Console.WriteLine("6. Exit");
    Console.Write("\nSelect an option (1-6): ");
    
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await AddUser(userRepository);
            break;
        case "2":
            await GetUserProfile(userRepository);
            break;
        case "3":
            Console.WriteLine("📝 Log workout functionality - Coming soon");
            break;
        case "4":
            Console.WriteLine("📊 View progress functionality - Coming soon");
            break;
        case "5":
            Console.WriteLine("⭐ Check readiness functionality - Coming soon");
            break;
        case "6":
            running = false;
            Console.WriteLine("\nGoodbye!");
            break;
        default:
            Console.WriteLine("❌ Invalid option. Please try again.\n");
            break;
    }

    if (running && choice != "1" && choice != "2")
    {
        Console.WriteLine();
    }
}

async Task AddUser(UserRepository repository)
{
    Console.Write("\nEnter user name: ");
    var name = Console.ReadLine();
    
    Console.Write("Enter email: ");
    var email = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
    {
        Console.WriteLine("❌ Name and email are required.\n");
        return;
    }

    try
    {
        var user = User.Create(name, email);
        var createdUser = await repository.CreateUserAsync(user);
        Console.WriteLine($"✓ User created successfully!");
        Console.WriteLine($"  ID: {createdUser.Id}");
        Console.WriteLine($"  Email: {createdUser.Email}");
        Console.WriteLine($"  Name: {createdUser.Name}\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error creating user: {ex.Message}\n");
    }
}

async Task GetUserProfile(UserRepository repository)
{
    Console.Write("\nEnter email to search: ");
    var email = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(email))
    {
        Console.WriteLine("❌ Email is required.\n");
        return;
    }

    try
    {
        var user = await repository.GetUserByEmailAsync(email);
        if (user is null)
        {
            Console.WriteLine($"❌ No user found with email: {email}\n");
        }
        else
        {
            Console.WriteLine($"\n✓ User found:");
            Console.WriteLine($"  ID: {user.Id}");
            Console.WriteLine($"  Email: {user.Email}");
            Console.WriteLine($"  Name: {user.Name}\n");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error retrieving user: {ex.Message}\n");
    }
}
