using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace AiService;

public sealed class WorkoutAiAdvisor
{
    private readonly ChatClient _chatClient;

    public WorkoutAiAdvisor(string endpoint, string apiKey, string deploymentName)
    {
        var client = new AzureOpenAIClient(
            new Uri(endpoint),
            new ApiKeyCredential(apiKey));
        _chatClient = client.GetChatClient(deploymentName);
    }

    public async Task<string> GetRecommendationAsync(string prompt)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "You are a professional fitness coach and workout advisor. " +
                "Provide concise, actionable advice based on the user's workout data. " +
                "Keep responses under 200 words. " +
                "Focus on safety, proper progression, and recovery."),
            new UserChatMessage(prompt)
        };

        var response = await _chatClient.CompleteChatAsync(messages);
        var rawText = response.Value.Content[0].Text;
        return CleanMarkdown(rawText);
    }

    private static string CleanMarkdown(string text)
    {
        // Remove bold/italic markers
        text = text.Replace("**", "");
        text = text.Replace("__", "");
        text = text.Replace("*", "");
        text = text.Replace("_", "");
        // Remove heading markers
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^#{1,6}\s*", "", System.Text.RegularExpressions.RegexOptions.Multiline);
        return text.Trim();
    }

    public async Task GetProgressInsightAsync(string userProgress)
    {
        var prompt = $"Analyze this workout progress and give insights:\n\n{userProgress}";
        var response = await GetRecommendationAsync(prompt);
        Console.WriteLine("\nAI Coach Insight:");
        Console.WriteLine(new string('-', 35));
        Console.WriteLine(response);
        Console.WriteLine();
    }

    public async Task GetReadinessAdviceAsync(string readinessData)
    {
        var prompt = $"Based on this readiness assessment, provide advice:\n\n{readinessData}";
        var response = await GetRecommendationAsync(prompt);
        Console.WriteLine("\nAI Coach Advice:");
        Console.WriteLine(new string('-', 35));
        Console.WriteLine(response);
        Console.WriteLine();
    }

    public async Task GetWorkoutRecommendationAsync(string userHistory)
    {
        var prompt = $"Based on this workout history, suggest what to focus on next:\n\n{userHistory}";
        var response = await GetRecommendationAsync(prompt);
        Console.WriteLine("\nAI Workout Recommendation:");
        Console.WriteLine(new string('-', 35));
        Console.WriteLine(response);
        Console.WriteLine();
    }
}
