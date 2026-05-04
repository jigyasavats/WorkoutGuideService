using Enums;
using Repository;

namespace WorkoutLogService;

public sealed class WorkoutLogger
{
    private readonly UserRepository _userRepository;
    private readonly WorkoutRepository _workoutRepository;

    public WorkoutLogger(UserRepository userRepository, WorkoutRepository workoutRepository)
    {
        _userRepository = userRepository;
        _workoutRepository = workoutRepository;
    }

    public async Task LogWorkoutAsync()
    {
        Console.Write("\nEnter your email: ");
        var email = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("❌ Email is required.\n");
            return;
        }

        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user is null)
        {
            Console.WriteLine($"❌ No user found with email: {email}. Please register first.\n");
            return;
        }

        bool logMore = true;
        while (logMore)
        {
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
                Console.WriteLine("❌ Invalid selection.\n");
                continue;
            }
            var selectedDay = days[dayIndex - 1];

            // Select exercise for that day
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
                Console.WriteLine("❌ Invalid selection.\n");
                continue;
            }
            var selectedExercise = exercises[exIndex - 1];

            // Collect sets, reps, weight
            Console.Write("Sets: ");
            if (!int.TryParse(Console.ReadLine(), out int sets) || sets <= 0)
            {
                Console.WriteLine("❌ Invalid sets value.\n");
                continue;
            }

            Console.Write("Reps: ");
            if (!int.TryParse(Console.ReadLine(), out int reps) || reps <= 0)
            {
                Console.WriteLine("❌ Invalid reps value.\n");
                continue;
            }

            Console.Write("Weight (kg): ");
            if (!double.TryParse(Console.ReadLine(), out double weight) || weight < 0)
            {
                Console.WriteLine("❌ Invalid weight value.\n");
                continue;
            }

            try
            {
                var log = WorkoutLog.Create(user.Id, user.Email, selectedDay, selectedExercise, sets, reps, weight);
                var created = await _workoutRepository.LogWorkoutAsync(log);
                Console.WriteLine($"\n✓ Workout logged!");
                Console.WriteLine($"  Day: {created.Day}");
                Console.WriteLine($"  Exercise: {created.Exercise}");
                Console.WriteLine($"  Sets: {created.Sets} | Reps: {created.Reps} | Weight: {created.WeightKg} kg");
                Console.WriteLine($"  Logged at: {created.LoggedAt:yyyy-MM-dd HH:mm} UTC");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error logging workout: {ex.Message}");
            }

            Console.Write("\nLog another exercise? (y/n): ");
            var again = Console.ReadLine();
            logMore = string.Equals(again, "y", StringComparison.OrdinalIgnoreCase);
        }

        Console.WriteLine();
    }
}
