using Enums;

namespace WorkoutLogService;

public static class WorkoutDayExercises
{
    private static readonly Dictionary<WorkoutDay, Exercise[]> _exercises = new()
    {
        [WorkoutDay.Leg] = new[]
        {
            Exercise.Squats,
            Exercise.Lunges,
            Exercise.LegPress,
            Exercise.CalfRaises,
            Exercise.LegCurls
        },
        [WorkoutDay.Hand] = new[]
        {
            Exercise.BicepCurls,
            Exercise.HammerCurls,
            Exercise.TricepDips
        },
        [WorkoutDay.Chest] = new[]
        {
            Exercise.BenchPress,
            Exercise.PushUps,
            Exercise.ChestFly
        },
        [WorkoutDay.Core] = new[]
        {
            Exercise.Planks,
            Exercise.Crunches,
            Exercise.LegRaises
        },
        [WorkoutDay.Shoulder] = new[]
        {
            Exercise.OverheadPress,
            Exercise.LateralRaises,
            Exercise.FrontRaises
        },
        [WorkoutDay.Back] = new[]
        {
            Exercise.Deadlifts,
            Exercise.PullUps,
            Exercise.Rows
        }
    };

    public static Exercise[] GetExercises(WorkoutDay day) => _exercises[day];
}
