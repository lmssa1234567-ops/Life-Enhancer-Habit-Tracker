namespace Life_Enhancer_Habit_Tracker.Models;

public sealed class DashboardMetrics
{
    public double WeeklyCompletionPercent { get; set; }
    public double MonthlyCompletionPercent { get; set; }
    public double YearlyCompletionPercent { get; set; }
    public double TaskPerformanceRatio { get; set; }
    public double MoodAverage { get; set; }
}

public sealed class NotificationItem
{
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "info";
}
