namespace WorkoutRedinessService;

public sealed class ReadinessResult
{
    public bool IsReady { get; set; }
    public double CurrentWeightKg { get; set; }
    public double SuggestedWeightKg { get; set; }
    public int TotalSessions { get; set; }
    public int ConsistentMonths { get; set; }
    public PainAssessment Pain { get; set; } = new();
    public bool ConsistentWeight { get; set; }
    public List<string> Reasons { get; set; } = new();
}
