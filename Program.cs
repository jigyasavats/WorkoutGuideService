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
var planGenerator = new WorkoutPlanGenerator(userRepository, workoutRepository, aiAdvisor);
var formTipAdvisor = new FormTipAdvisor(aiAdvisor);
var recoveryAdvisor = new RecoveryAdvisor(userRepository, workoutRepository, aiAdvisor);
var nutritionAdvisor = new NutritionAdvisor(userRepository, workoutRepository, aiAdvisor);
var chatCoach = new AiChatCoach(aiAdvisor);
var injuryPredictor = new InjuryRiskPredictor(userRepository, workoutRepository, aiAdvisor);
var alternativeAdvisor = new ExerciseAlternativeAdvisor(aiAdvisor);

Console.WriteLine("Cosmos DB and Azure OpenAI initialized.\n");

bool running = true;

while (running)
{
    Console.WriteLine("=== Workout Guide System ===");
    Console.WriteLine("1.  AI Chat Coach");
    Console.WriteLine("2.  Add a new user");
    Console.WriteLine("3.  Get user profile");
    Console.WriteLine("4.  Log workout details");
    Console.WriteLine("5.  View progress");
    Console.WriteLine("6.  Check readiness for next level");
    Console.WriteLine("7.  AI Workout Plan Generator");
    Console.WriteLine("8.  Form and Technique Tips");
    Console.WriteLine("9.  Recovery Advisor");
    Console.WriteLine("10. Nutrition Suggestions");
    Console.WriteLine("11. Injury Risk Predictor");
    Console.WriteLine("12. Exercise Alternatives");
    Console.WriteLine("13. Exit");
    Console.Write("\nSelect an option (1-13): ");
    
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await chatCoach.StartChatAsync();
            break;
        case "2":
            await userManager.AddUserAsync();
            break;
        case "3":
            await userManager.GetUserProfileAsync();
            break;
        case "4":
            await workoutLogger.LogWorkoutAsync();
            break;
        case "5":
            await progressViewer.ViewProgressAsync();
            break;
        case "6":
            await readinessChecker.CheckReadinessAsync();
            break;
        case "7":
            await planGenerator.GeneratePlanAsync();
            break;
        case "8":
            await formTipAdvisor.GetFormTipsAsync();
            break;
        case "9":
            await recoveryAdvisor.GetRecoveryAdviceAsync();
            break;
        case "10":
            await nutritionAdvisor.GetNutritionAdviceAsync();
            break;
        case "11":
            await injuryPredictor.AnalyzeRiskAsync();
            break;
        case "12":
            await alternativeAdvisor.SuggestAlternativesAsync();
            break;
        case "13":
            running = false;
            Console.WriteLine("\nGoodbye!");
            break;
        default:
            Console.WriteLine("Invalid option. Please try again.\n");
            break;
    }

    if (running)
    {
        Console.WriteLine();
    }
}

