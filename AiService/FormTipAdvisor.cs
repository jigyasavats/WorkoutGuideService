using Enums;
using WorkoutLogService;

namespace AiService;

public sealed class FormTipAdvisor
{
    private readonly WorkoutAiAdvisor _aiAdvisor;

    public FormTipAdvisor(WorkoutAiAdvisor aiAdvisor)
    {
        _aiAdvisor = aiAdvisor;
    }

    public async Task GetFormTipsAsync()
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

        var prompt = $"Provide detailed form and technique tips for the exercise: {selectedExercise}. " +
                     $"Include: proper body positioning, common mistakes to avoid, breathing pattern, " +
                     $"and tips to maximize muscle engagement. Keep it practical and beginner-friendly.";

        try
        {
            var response = await _aiAdvisor.GetRecommendationAsync(prompt);
            Console.WriteLine($"\nForm Tips: {selectedExercise} ({selectedDay} Day)");
            Console.WriteLine(new string('-', 45));
            Console.WriteLine(response);
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting form tips: {ex.Message}\n");
        }
    }
}
