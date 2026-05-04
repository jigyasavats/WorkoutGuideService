using Enums;
using Newtonsoft.Json;

namespace WorkoutLogService;

public sealed class WorkoutLog
{
    [JsonProperty("id")]
    public string Id { get; set; } = default!;

    public string UserId { get; set; } = default!;

    public string UserEmail { get; set; } = default!;

    public WorkoutDay Day { get; set; }

    public Exercise Exercise { get; set; }

    public int Sets { get; set; }

    public int Reps { get; set; }

    public double WeightKg { get; set; }

    public DateTime LoggedAt { get; set; }

    public static WorkoutLog Create(
        string userId,
        string userEmail,
        WorkoutDay day,
        Exercise exercise,
        int sets,
        int reps,
        double weightKg) => new()
    {
        Id = Guid.NewGuid().ToString(),
        UserId = userId,
        UserEmail = userEmail,
        Day = day,
        Exercise = exercise,
        Sets = sets,
        Reps = reps,
        WeightKg = weightKg,
        LoggedAt = DateTime.UtcNow
    };
}
