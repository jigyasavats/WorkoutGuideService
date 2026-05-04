using System.Text;
using AiService;
using Enums;
using Repository;

namespace WorkoutRedinessService;

public sealed class ReadyForNextLevel
{
    private readonly UserRepository _userRepository;
    private readonly WorkoutRepository _workoutRepository;
    private readonly WorkoutAiAdvisor _aiAdvisor;
    private const double WeightIncrement = 2.5;
    private const int MinimumMonths = 2;
    private const int MinimumSessions = 8;

    public ReadyForNextLevel(UserRepository userRepository, WorkoutRepository workoutRepository, WorkoutAiAdvisor aiAdvisor)
    {
        _userRepository = userRepository;
        _workoutRepository = workoutRepository;
        _aiAdvisor = aiAdvisor;
    }

    public async Task CheckReadinessAsync()
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
            Console.WriteLine($"No workout logs found for {user.Name}. Start logging workouts first.\n");
            return;
        }

        // Select workout day
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

        // Select exercise
        var exercises = WorkoutLogService.WorkoutDayExercises.GetExercises(selectedDay);
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

        // Ask about pain areas
        Console.WriteLine("\nDid you experience any of the following pain during this exercise?");
        Console.Write("  Knee pain? (y/n): ");
        bool kneePain = string.Equals(Console.ReadLine(), "y", StringComparison.OrdinalIgnoreCase);

        Console.Write("  Back pain? (y/n): ");
        bool backPain = string.Equals(Console.ReadLine(), "y", StringComparison.OrdinalIgnoreCase);

        Console.Write("  Pain during the workout itself? (y/n): ");
        bool duringWorkoutPain = string.Equals(Console.ReadLine(), "y", StringComparison.OrdinalIgnoreCase);

        var painAreas = new PainAssessment
        {
            KneePain = kneePain,
            BackPain = backPain,
            DuringWorkoutPain = duringWorkoutPain
        };

        // Evaluate readiness
        var result = EvaluateReadiness(logs, selectedDay, selectedExercise, painAreas);

        // Display result
        DisplayResult(selectedDay, selectedExercise, result);

        // AI-powered advice
        var readinessData = new StringBuilder();
        readinessData.AppendLine($"Exercise: {selectedExercise} ({selectedDay} Day)");
        readinessData.AppendLine($"Total sessions: {result.TotalSessions}");
        readinessData.AppendLine($"Months active: {result.ConsistentMonths}");
        readinessData.AppendLine($"Current weight: {result.CurrentWeightKg} kg");
        readinessData.AppendLine($"Knee pain: {(result.Pain.KneePain ? "Yes" : "No")}");
        readinessData.AppendLine($"Back pain: {(result.Pain.BackPain ? "Yes" : "No")}");
        readinessData.AppendLine($"Pain during workout: {(result.Pain.DuringWorkoutPain ? "Yes" : "No")}");
        readinessData.AppendLine($"Consistent at weight: {(result.ConsistentWeight ? "Yes" : "No")}");
        readinessData.AppendLine($"Ready: {(result.IsReady ? "Yes" : "No")}");

        await _aiAdvisor.GetReadinessAdviceAsync(readinessData.ToString());
    }

    private ReadinessResult EvaluateReadiness(
        List<WorkoutLogService.WorkoutLog> allLogs,
        WorkoutDay day,
        Exercise exercise,
        PainAssessment pain)
    {
        var exerciseLogs = allLogs
            .Where(l => l.Day == day && l.Exercise == exercise)
            .OrderByDescending(l => l.LoggedAt)
            .ToList();

        var result = new ReadinessResult
        {
            TotalSessions = exerciseLogs.Count,
            Pain = pain
        };

        if (exerciseLogs.Count == 0)
        {
            result.IsReady = false;
            result.Reasons.Add("No logs found for this exercise.");
            return result;
        }

        var latestWeight = exerciseLogs.First().WeightKg;
        result.CurrentWeightKg = latestWeight;
        result.SuggestedWeightKg = latestWeight + WeightIncrement;

        // Check consistency: how many months has the user been doing this exercise
        var firstLog = exerciseLogs.Last().LoggedAt;
        var monthsActive = (int)((DateTime.UtcNow - firstLog).TotalDays / 30);
        result.ConsistentMonths = monthsActive;

        // Check if user has been consistent with the current weight
        var logsAtCurrentWeight = exerciseLogs.Where(l => l.WeightKg == latestWeight).ToList();
        result.ConsistentWeight = logsAtCurrentWeight.Count >= MinimumSessions;

        // Evaluate readiness
        result.IsReady = true;

        if (pain.KneePain)
        {
            result.IsReady = false;
            result.Reasons.Add("You reported knee pain. Consider reducing weight and consulting a professional.");
        }

        if (pain.BackPain)
        {
            result.IsReady = false;
            result.Reasons.Add("You reported back pain. Check your form and avoid increasing weight until resolved.");
        }

        if (pain.DuringWorkoutPain)
        {
            result.IsReady = false;
            result.Reasons.Add("You experienced pain during the workout. Do not increase weight until pain-free.");
        }

        if (monthsActive < MinimumMonths)
        {
            result.IsReady = false;
            result.Reasons.Add($"You have been doing this exercise for {monthsActive} month(s). Minimum {MinimumMonths} months recommended.");
        }

        if (!result.ConsistentWeight)
        {
            result.IsReady = false;
            result.Reasons.Add($"You have {logsAtCurrentWeight.Count} session(s) at {latestWeight} kg. Minimum {MinimumSessions} sessions recommended before increasing.");
        }

        if (result.IsReady)
        {
            result.Reasons.Add("You have been consistent and pain-free. You are ready to move up!");
        }

        return result;
    }

    private void DisplayResult(WorkoutDay day, Exercise exercise, ReadinessResult result)
    {
        Console.WriteLine($"\nReadiness Report: {exercise} ({day} Day)");
        Console.WriteLine(new string('-', 45));
        Console.WriteLine($"  Total sessions logged : {result.TotalSessions}");
        Console.WriteLine($"  Months active         : {result.ConsistentMonths}");
        Console.WriteLine($"  Current weight        : {result.CurrentWeightKg} kg");
        Console.WriteLine($"  Knee pain             : {(result.Pain.KneePain ? "Yes" : "No")}");
        Console.WriteLine($"  Back pain             : {(result.Pain.BackPain ? "Yes" : "No")}");
        Console.WriteLine($"  Pain during workout   : {(result.Pain.DuringWorkoutPain ? "Yes" : "No")}");
        Console.WriteLine($"  Consistent at weight  : {(result.ConsistentWeight ? "Yes" : "No")}");
        Console.WriteLine();

        if (result.IsReady)
        {
            Console.WriteLine($"  READY to move to next level!");
            Console.WriteLine($"  Suggested weight: {result.SuggestedWeightKg} kg (+{WeightIncrement} kg)");
        }
        else
        {
            Console.WriteLine("  NOT READY yet. Here is why:");
            foreach (var reason in result.Reasons)
            {
                Console.WriteLine($"    - {reason}");
            }
        }

        Console.WriteLine();
    }
}