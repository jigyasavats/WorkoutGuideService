# AI-WorkoutGuideService

A .NET 8 console application that helps users track workouts, monitor progress, and get AI-powered fitness advice. Built with Azure Cosmos DB for data storage and Azure OpenAI for intelligent recommendations.

## Features

| # | Feature | Description |
|---|---------|-------------|
| 1 | AI Chat Coach | Free-form chat with an AI fitness coach. Ask anything about workouts, nutrition, or recovery |
| 2 | Add a new user | Register with name and email |
| 3 | Get user profile | Look up a user by email |
| 4 | Log workout details | Log exercises with sets, reps, and weight. Supports multiple logs in one session |
| 5 | View progress | View all workout logs grouped by day with AI-powered insights |
| 6 | Check readiness for next level | Evaluates if you are ready to increase weight based on consistency, duration, and pain assessment |
| 7 | AI Workout Plan Generator | Generates a weekly workout plan based on your history and goals |
| 8 | Form and Technique Tips | Get proper form guidance for any exercise |
| 9 | Recovery Advisor | Personalized recovery advice based on sleep, energy, pain, and recent workouts |
| 10 | Nutrition Suggestions | Pre/post workout meal suggestions based on workout type, goals, and dietary restrictions |
| 11 | Injury Risk Predictor | Analyzes workout patterns to warn about overtraining, rapid weight jumps, and muscle imbalances |
| 12 | Exercise Alternatives | Suggests substitute exercises based on your reason (injury, no equipment, variety, difficulty) |

## Tech Stack

- .NET 8
- Azure Cosmos DB (data storage)
- Azure Key Vault (secrets management)
- Azure OpenAI - GPT-4o (AI-powered features)

## Project Structure

```
WorkoutGuideService/
  Program.cs                        # App entry point, menu routing
  appsettings.json                  # Configuration (Key Vault, Cosmos DB, Azure OpenAI)

  Enums/
    WorkoutDay.cs                   # Leg, Hand, Chest, Core, Shoulder, Back
    Exercise.cs                     # All exercises grouped by workout day

  Repository/
    CosmosDbService.cs              # Cosmos DB client and database initialization
    UserRepository.cs               # User CRUD operations
    WorkoutRepository.cs            # Workout log CRUD operations

  UserService/
    User.cs                         # User model
    UserManager.cs                  # Add user, get user profile

  WorkoutLogService/
    WorkoutLog.cs                   # Workout log model
    WorkoutDayExercises.cs          # Maps each workout day to its exercises
    WorkoutLogger.cs                # Log workouts with looping support
    ProgressViewer.cs               # View progress with AI insights

  WorkoutRedinessService/
    PainAssessment.cs               # Knee, back, and during-workout pain model
    ReadinessResult.cs              # Readiness evaluation result model
    ReadyForNextLevel.cs            # Readiness check logic with AI advice

  AiService/
    WorkoutAiAdvisor.cs             # Core Azure OpenAI client wrapper
    AiChatCoach.cs                  # Free-form AI fitness chat
    WorkoutPlanGenerator.cs         # AI weekly workout plan generation
    FormTipAdvisor.cs               # AI form and technique tips
    RecoveryAdvisor.cs              # AI recovery advice
    NutritionAdvisor.cs             # AI nutrition suggestions
    InjuryRiskPredictor.cs          # AI overtraining and injury risk analysis
    ExerciseAlternativeAdvisor.cs   # AI exercise substitute suggestions
```

## Workout Days and Exercises

| Day | Exercises |
|-----|-----------|
| Leg | Squats, Lunges, Leg Press, Calf Raises, Leg Curls |
| Hand | Bicep Curls, Hammer Curls, Tricep Dips |
| Chest | Bench Press, Push-Ups, Chest Fly |
| Core | Planks, Crunches, Leg Raises |
| Shoulder | Overhead Press, Lateral Raises, Front Raises |
| Back | Deadlifts, Pull-Ups, Rows |

## Prerequisites

- .NET 8 SDK
- Azure subscription with the following resources:
  - Azure Cosmos DB account
  - Azure Key Vault
  - Azure OpenAI resource with a deployed model (e.g., gpt-4o)

## Configuration

Update `appsettings.json` with your Azure resource details:

```json
{
  "KeyVault": {
    "VaultUri": "https://your-vault.vault.azure.net/"
  },
  "CosmosDb": {
    "DatabaseId": "FitnessPlatform",
    "ContainerId": "Users",
    "WorkoutContainerId": "Workouts",
    "ConnectionStringSecretName": "CosmosDbConnectionString"
  },
  "AzureOpenAi": {
    "Endpoint": "https://your-openai.openai.azure.com/",
    "ApiKeySecretName": "OpenAiApiKey",
    "DeploymentName": "your-deployment-name"
  }
}
```

### Key Vault Secrets

Store the following secrets in your Azure Key Vault:

| Secret Name | Value |
|-------------|-------|
| CosmosDbConnectionString | Your Cosmos DB connection string |
| OpenAiApiKey | Your Azure OpenAI API key |

## Readiness Check Logic

The readiness checker evaluates whether you should increase weight based on:

- No knee pain, back pain, or pain during workout
- At least 2 months of doing the exercise
- At least 8 sessions at the current weight
- If ready, suggests a +2.5 kg weight increase

## Running the Application

```bash
dotnet build
dotnet run
```

## Authentication

The application uses `DefaultAzureCredential` from Azure Identity to authenticate with Azure Key Vault. This supports:

- Azure CLI credentials (for local development)
- Managed Identity (for deployed environments)
- Visual Studio / VS Code credentials
