using System.Text;
using Enums;
using Repository;

namespace AiService;

public sealed class WorkoutPlanGenerator
{
    private readonly UserRepository _userRepository;
    private readonly WorkoutRepository _workoutRepository;
    private readonly WorkoutAiAdvisor _aiAdvisor;

    public WorkoutPlanGenerator(UserRepository userRepository, WorkoutRepository workoutRepository, WorkoutAiAdvisor aiAdvisor)
    {
        _userRepository = userRepository;
        _workoutRepository = workoutRepository;
        _aiAdvisor = aiAdvisor;
    }

    public async Task GeneratePlanAsync()
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

        Console.Write("What is your fitness goal? (e.g., build muscle, lose weight, improve endurance): ");
        var goal = Console.ReadLine();

        Console.Write("How many days per week can you train? (e.g., 3, 4, 5): ");
        var daysInput = Console.ReadLine();

        var logs = await _workoutRepository.GetWorkoutsByEmailAsync(email);

        var context = new StringBuilder();
        context.AppendLine($"User: {user.Name}");
        context.AppendLine($"Goal: {goal}");
        context.AppendLine($"Available training days per week: {daysInput}");
        context.AppendLine($"Available workout days: {string.Join(", ", Enum.GetValues<WorkoutDay>())}");

        if (logs.Count > 0)
        {
            context.AppendLine($"\nRecent workout history ({logs.Count} total sessions):");
            foreach (var group in logs.GroupBy(l => l.Day).OrderBy(g => g.Key))
            {
                var latestLog = group.OrderByDescending(l => l.LoggedAt).First();
                context.AppendLine($"  {group.Key}: {group.Count()} sessions, latest weight: {latestLog.WeightKg} kg ({latestLog.Exercise})");
            }
        }
        else
        {
            context.AppendLine("No previous workout history. This is a beginner.");
        }

        context.AppendLine("\nGenerate a structured weekly workout plan with specific exercises, sets, reps, and rest days.");

        try
        {
            var response = await _aiAdvisor.GetRecommendationAsync(context.ToString());
            Console.WriteLine("\nYour AI-Generated Weekly Workout Plan:");
            Console.WriteLine(new string('-', 45));
            Console.WriteLine(response);
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating plan: {ex.Message}\n");
        }
    }
}
