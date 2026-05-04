using Enums;
using WorkoutLogService;

namespace AiService;

public sealed class ExerciseAlternativeAdvisor
{
    private readonly WorkoutAiAdvisor _aiAdvisor;

    public ExerciseAlternativeAdvisor(WorkoutAiAdvisor aiAdvisor)
    {
        _aiAdvisor = aiAdvisor;
    }

    public async Task SuggestAlternativesAsync()
    {
        Console.WriteLine("\nSelect workout day:");
        var days = Enum.GetValues<WorkoutDay>();
        for (int i = 0; i < days.Length; i++)
        {
            Console.WriteLine($"  {i + 1}. {days[i]}");
        }
        Console.Write("Choice: ");
        var dayInput = Console.ReadLine();

        if (!int.TryParse(dayInput, out int dayIndex) || dayIndex < 1 || dayIndex > days.Length)
        {
            Console.WriteLine("Invalid selection.\n");
            return;
        }
        var selectedDay = days[dayIndex - 1];

        var exercises = WorkoutDayExercises.GetExercises(selectedDay);
        Console.WriteLine($"\nExercises for {selectedDay} day:");
        for (int i = 0; i < exercises.Length; i++)
        {
            Console.WriteLine($"  {i + 1}. {exercises[i]}");
        }
        Console.Write("Choice: ");
        var exInput = Console.ReadLine();

        if (!int.TryParse(exInput, out int exIndex) || exIndex < 1 || exIndex > exercises.Length)
        {
            Console.WriteLine("Invalid selection.\n");
            return;
        }
        var selectedExercise = exercises[exIndex - 1];

        Console.WriteLine("\nWhy do you need an alternative?");
        Console.WriteLine("  1. Joint pain or injury");
        Console.WriteLine("  2. No equipment available");
        Console.WriteLine("  3. Want variety");
        Console.WriteLine("  4. Too difficult");
        Console.WriteLine("  5. Other");
        Console.Write("Choice: ");
        var reasonInput = Console.ReadLine();

        var reason = reasonInput switch
        {
            "1" => "joint pain or injury",
            "2" => "no equipment available",
            "3" => "want variety in routine",
            "4" => "exercise is too difficult at current level",
            _ => "general preference"
        };

        var prompt = $"Suggest 3-4 alternative exercises for {selectedExercise} ({selectedDay} day). " +
                     $"The reason for needing alternatives: {reason}. " +
                     $"For each alternative, explain: what muscles it targets, difficulty level, " +
                     $"equipment needed, and why it is a good substitute. " +
                     $"Include at least one bodyweight option.";

        try
        {
            var response = await _aiAdvisor.GetRecommendationAsync(prompt);
            Console.WriteLine($"\nAlternatives for {selectedExercise}:");
            Console.WriteLine(new string('-', 35));
            Console.WriteLine(response);
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting alternatives: {ex.Message}\n");
        }
    }
}
