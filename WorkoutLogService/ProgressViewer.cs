using Repository;

namespace WorkoutLogService;

public sealed class ProgressViewer
{
    private readonly UserRepository _userRepository;
    private readonly WorkoutRepository _workoutRepository;

    public ProgressViewer(UserRepository userRepository, WorkoutRepository workoutRepository)
    {
        _userRepository = userRepository;
        _workoutRepository = workoutRepository;
    }

    public async Task ViewProgressAsync()
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

        try
        {
            var logs = await _workoutRepository.GetWorkoutsByEmailAsync(email);

            if (logs.Count == 0)
            {
                Console.WriteLine($"\nNo workout logs found for {user.Name}.\n");
                return;
            }

            Console.WriteLine($"\nWorkout Progress for {user.Name} ({user.Email})");
            Console.WriteLine(new string('-', 50));

            var groupedByDay = logs
                .GroupBy(l => l.Day)
                .OrderBy(g => g.Key);

            foreach (var dayGroup in groupedByDay)
            {
                Console.WriteLine($"\n  [{dayGroup.Key} Day] - {dayGroup.Count()} log(s)");

                foreach (var log in dayGroup.OrderByDescending(l => l.LoggedAt))
                {
                    Console.WriteLine($"    {log.Exercise,-20} | Sets: {log.Sets} | Reps: {log.Reps} | Weight: {log.WeightKg} kg | {log.LoggedAt:yyyy-MM-dd HH:mm} UTC");
                }
            }

            Console.WriteLine($"\nTotal workouts logged: {logs.Count}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving progress: {ex.Message}\n");
        }
    }
}
