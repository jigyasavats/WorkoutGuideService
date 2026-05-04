using System.Text;
using Repository;

namespace AiService;

public sealed class RecoveryAdvisor
{
    private readonly UserRepository _userRepository;
    private readonly WorkoutRepository _workoutRepository;
    private readonly WorkoutAiAdvisor _aiAdvisor;

    public RecoveryAdvisor(UserRepository userRepository, WorkoutRepository workoutRepository, WorkoutAiAdvisor aiAdvisor)
    {
        _userRepository = userRepository;
        _workoutRepository = workoutRepository;
        _aiAdvisor = aiAdvisor;
    }

    public async Task GetRecoveryAdviceAsync()
    {
        Console.Write("\nEnter your email: ");
        var email = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("Email is required.\n");
            return;
        }

        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user is null)
        {
            Console.WriteLine($"No user found with email: {email}. Please register first.\n");
            return;
        }

        Console.Write("Are you experiencing any pain? (y/n): ");
        bool hasPain = string.Equals(Console.ReadLine(), "y", StringComparison.OrdinalIgnoreCase);

        var painDetails = "";
        if (hasPain)
        {
            Console.Write("Describe the pain (location, type, severity 1-10): ");
            painDetails = Console.ReadLine() ?? "";
        }

        Console.Write("How many hours of sleep did you get last night? ");
        var sleep = Console.ReadLine();

        Console.Write("Rate your energy level today (1-10): ");
        var energy = Console.ReadLine();

        var logs = await _workoutRepository.GetWorkoutsByEmailAsync(email);

        var context = new StringBuilder();
        context.AppendLine($"User: {user.Name}");
        context.AppendLine($"Sleep last night: {sleep} hours");
        context.AppendLine($"Energy level: {energy}/10");
        context.AppendLine($"Pain reported: {(hasPain ? "Yes" : "No")}");
        if (hasPain)
        {
            context.AppendLine($"Pain details: {painDetails}");
        }

        if (logs.Count > 0)
        {
            var recentLogs = logs.Take(10).ToList();
            context.AppendLine($"\nRecent workouts:");
            foreach (var log in recentLogs)
            {
                context.AppendLine($"  {log.Day} - {log.Exercise}: {log.Sets}x{log.Reps} at {log.WeightKg} kg ({log.LoggedAt:yyyy-MM-dd})");
            }
        }

        context.AppendLine("\nProvide recovery advice including: rest day recommendations, stretches, " +
                          "mobility exercises, hydration and sleep tips. If pain is reported, suggest " +
                          "specific recovery exercises for that area.");

        try
        {
            var response = await _aiAdvisor.GetRecommendationAsync(context.ToString());
            Console.WriteLine("\nRecovery Advice:");
            Console.WriteLine(new string('-', 35));
            Console.WriteLine(response);
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting recovery advice: {ex.Message}\n");
        }
    }
}
