namespace AiService;

public sealed class AiChatCoach
{
    private readonly WorkoutAiAdvisor _aiAdvisor;

    public AiChatCoach(WorkoutAiAdvisor aiAdvisor)
    {
        _aiAdvisor = aiAdvisor;
    }

    public async Task StartChatAsync()
    {
        Console.WriteLine("\nAI Fitness Chat Coach");
        Console.WriteLine(new string('-', 35));
        Console.WriteLine("Ask me anything about fitness, workouts, nutrition, or recovery.");
        Console.WriteLine("Type 'exit' to return to the main menu.\n");

        while (true)
        {
            Console.Write("You: ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Chat ended.\n");
                break;
            }

            try
            {
                var response = await _aiAdvisor.GetRecommendationAsync(input);
                Console.WriteLine($"\nCoach: {response}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }
        }
    }
}
