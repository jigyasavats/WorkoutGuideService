using System.Text;
using Enums;
using Repository;

namespace AiService;

public sealed class NutritionAdvisor
{
    private readonly UserRepository _userRepository;
    private readonly WorkoutRepository _workoutRepository;
    private readonly WorkoutAiAdvisor _aiAdvisor;

    public NutritionAdvisor(UserRepository userRepository, WorkoutRepository workoutRepository, WorkoutAiAdvisor aiAdvisor)
    {
        _userRepository = userRepository;
        _workoutRepository = workoutRepository;
        _aiAdvisor = aiAdvisor;
    }

    public async Task GetNutritionAdviceAsync()
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

        Console.WriteLine("\nWhat is your workout day today?");
        var days = Enum.GetValues<WorkoutDay>();
        for (int i = 0; i < days.Length; i++)
        {
            Console.WriteLine($"  {i + 1}. {days[i]}");
        }
        Console.Write($"  {days.Length + 1}. Rest day\n");
        Console.Write("Choice: ");
        var dayInput = Console.ReadLine();

        string workoutDay;
        if (int.TryParse(dayInput, out int dayIndex) && dayIndex >= 1 && dayIndex <= days.Length)
        {
            workoutDay = days[dayIndex - 1].ToString();
        }
        else if (dayIndex == days.Length + 1)
        {
            workoutDay = "Rest day";
        }
        else
        {
            Console.WriteLine("Invalid selection.\n");
            return;
        }

        Console.Write("What is your fitness goal? (e.g., build muscle, lose weight, maintain): ");
        var goal = Console.ReadLine();

        Console.Write("Any dietary restrictions? (e.g., vegetarian, vegan, none): ");
        var restrictions = Console.ReadLine();

        var logs = await _workoutRepository.GetWorkoutsByEmailAsync(email);

        var context = new StringBuilder();
        context.AppendLine($"User: {user.Name}");
        context.AppendLine($"Today's workout: {workoutDay}");
        context.AppendLine($"Fitness goal: {goal}");
        context.AppendLine($"Dietary restrictions: {restrictions}");

        if (logs.Count > 0)
        {
            var todayLogs = logs.Where(l => l.Day.ToString() == workoutDay).Take(5).ToList();
            if (todayLogs.Count > 0)
            {
                context.AppendLine($"\nTypical {workoutDay} exercises:");
                foreach (var log in todayLogs)
                {
                    context.AppendLine($"  {log.Exercise}: {log.Sets}x{log.Reps} at {log.WeightKg} kg");
                }
            }
        }

        context.AppendLine("\nProvide nutrition advice including: pre-workout meal, post-workout meal, " +
                          "hydration tips, and key macronutrients to focus on for this workout type. " +
                          "Give specific food suggestions.");

        try
        {
            var response = await _aiAdvisor.GetRecommendationAsync(context.ToString());
            Console.WriteLine($"\nNutrition Advice for {workoutDay}:");
            Console.WriteLine(new string('-', 35));
            Console.WriteLine(response);
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting nutrition advice: {ex.Message}\n");
        }
    }
}
