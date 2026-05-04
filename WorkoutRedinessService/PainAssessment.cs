namespace WorkoutRedinessService;

public sealed class PainAssessment
{
    public bool KneePain { get; set; }
    public bool BackPain { get; set; }
    public bool DuringWorkoutPain { get; set; }

    public bool HasAnyPain => KneePain || BackPain || DuringWorkoutPain;
}
