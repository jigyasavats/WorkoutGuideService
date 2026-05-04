using System.Text;
using Repository;

namespace AiService;

public sealed class InjuryRiskPredictor
{
    private readonly UserRepository _userRepository;
    private readonly WorkoutRepository _workoutRepository;
    private readonly WorkoutAiAdvisor _aiAdvisor;

    public InjuryRiskPredictor(UserRepository userRepository, WorkoutRepository workoutRepository, WorkoutAiAdvisor aiAdvisor)
    {
        _userRepository = userRepository;
        _workoutRepository = workoutRepository;
        _aiAdvisor = aiAdvisor;
    }

    public async Task AnalyzeRiskAsync()
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

        var logs = await _workoutRepository.GetWorkoutsByEmailAsync(email);
        if (logs.Count == 0)
        {
            Console.WriteLine("No workout logs found. Start logging workouts first.\n");
            return;
        }

        var context = new StringBuilder();
        context.AppendLine($"User: {user.Name}");
        context.AppendLine($"Total workout sessions: {logs.Count}");

        // Analyze frequency per muscle group in the last 7 days
        var recentLogs = logs.Where(l => l.LoggedAt >= DateTime.UtcNow.AddDays(-7)).ToList();
        context.AppendLine($"\nWorkouts in the last 7 days: {recentLogs.Count}");

        var dayFrequency = recentLogs.GroupBy(l => l.Day).Select(g => new
        {
            Day = g.Key,
            Count = g.Count(),
            Exercises = g.Select(l => l.Exercise).Distinct().Count()
        });

        foreach (var day in dayFrequency)
        {
            context.AppendLine($"  {day.Day}: {day.Count} sessions, {day.Exercises} different exercises");
        }

        // Check for rapid weight increases
        var exerciseGroups = logs.GroupBy(l => l.Exercise);
        context.AppendLine("\nWeight progression per exercise (last 5 sessions):");
        foreach (var group in exerciseGroups)
        {
            var recent = group.OrderByDescending(l => l.LoggedAt).Take(5).ToList();
            if (recent.Count >= 2)
            {
                var weights = recent.Select(l => l.WeightKg).ToList();
                context.AppendLine($"  {group.Key}: {string.Join(" -> ", weights.Select(w => $"{w}kg"))}");
            }
        }

        // Check for consecutive days without rest
        var workoutDates = logs.Select(l => l.LoggedAt.Date).Distinct().OrderByDescending(d => d).Take(14).ToList();
        int consecutiveDays = 0;
        for (int i = 0; i < workoutDates.Count - 1; i++)
        {
            if ((workoutDates[i] - workoutDates[i + 1]).Days == 1)
                consecutiveDays++;
            else
                break;
        }
        context.AppendLine($"\nConsecutive workout days (current streak): {consecutiveDays + 1}");

        context.AppendLine("\nAnalyze this workout data for injury risks. Look for:");
        context.AppendLine("- Overtraining specific muscle groups (same group more than 2x per week)");
        context.AppendLine("- Too many consecutive days without rest");
        context.AppendLine("- Rapid weight increases that could cause injury");
        context.AppendLine("- Muscle imbalances (neglecting opposing muscle groups)");
        context.AppendLine("Provide specific warnings and recommendations.");

        try
        {
            var response = await _aiAdvisor.GetRecommendationAsync(context.ToString());
            Console.WriteLine("\nInjury Risk Analysis:");
            Console.WriteLine(new string('-', 35));
            Console.WriteLine(response);
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error analyzing risk: {ex.Message}\n");
        }
    }
}
