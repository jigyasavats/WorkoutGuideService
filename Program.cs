using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Repository;
using UserService;
using WorkoutLogService;
using WorkoutRedinessService;
using AiService;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var vaultUri = config["KeyVault:VaultUri"];
var secretName = config["CosmosDb:ConnectionStringSecretName"];
var databaseId = config["CosmosDb:DatabaseId"];
var containerId = config["CosmosDb:ContainerId"];
var workoutContainerId = config["CosmosDb:WorkoutContainerId"];
var openAiEndpoint = config["AzureOpenAi:Endpoint"];
var openAiKeySecretName = config["AzureOpenAi:ApiKeySecretName"];
var openAiDeploymentName = config["AzureOpenAi:DeploymentName"];

if (string.IsNullOrWhiteSpace(vaultUri)
    || string.IsNullOrWhiteSpace(secretName)
    || string.IsNullOrWhiteSpace(databaseId)
    || string.IsNullOrWhiteSpace(containerId)
    || string.IsNullOrWhiteSpace(workoutContainerId)
    || string.IsNullOrWhiteSpace(openAiEndpoint)
    || string.IsNullOrWhiteSpace(openAiKeySecretName)
    || string.IsNullOrWhiteSpace(openAiDeploymentName))
{
    Console.WriteLine("Configuration is missing. Please update appsettings.json.");
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

var openAiKeySecret = await secretClient.GetSecretAsync(openAiKeySecretName);
var openAiApiKey = openAiKeySecret.Value.Value;

if (string.IsNullOrWhiteSpace(openAiApiKey))
{
    Console.WriteLine("Azure OpenAI API key secret is empty.");
    return;
}

var cosmosService = await CosmosDbService.CreateAsync(cosmosConnectionString, databaseId);

var userContainerResponse = await cosmosService.Database.CreateContainerIfNotExistsAsync(containerId, "/Email");
var userRepository = new UserRepository(userContainerResponse.Container);

var workoutContainerResponse = await cosmosService.Database.CreateContainerIfNotExistsAsync(workoutContainerId, "/UserEmail");
var workoutRepository = new WorkoutRepository(workoutContainerResponse.Container);

var userManager = new UserManager(userRepository);
var workoutLogger = new WorkoutLogger(userRepository, workoutRepository);
var aiAdvisor = new WorkoutAiAdvisor(openAiEndpoint, openAiApiKey, openAiDeploymentName);
var progressViewer = new ProgressViewer(userRepository, workoutRepository, aiAdvisor);
var readinessChecker = new ReadyForNextLevel(userRepository, workoutRepository, aiAdvisor);

Console.WriteLine("Cosmos DB and Azure OpenAI initialized.\n");

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
            await userManager.AddUserAsync();
            break;
        case "2":
            await userManager.GetUserProfileAsync();
            break;
        case "3":
            await workoutLogger.LogWorkoutAsync();
            break;
        case "4":
            await progressViewer.ViewProgressAsync();
            break;
        case "5":
            await readinessChecker.CheckReadinessAsync();
            break;
        case "6":
            running = false;
            Console.WriteLine("\nGoodbye!");
            break;
        default:
            Console.WriteLine("Invalid option. Please try again.\n");
            break;
    }

    if (running && choice != "1" && choice != "2" && choice != "3" && choice != "4")
    {
        Console.WriteLine();
    }
}

