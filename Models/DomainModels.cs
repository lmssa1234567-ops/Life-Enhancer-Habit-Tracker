namespace Life_Enhancer_Habit_Tracker.Models;

public sealed class Routine : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public ScheduleType ScheduleType { get; set; } = ScheduleType.Daily;
    public string MeasurementType { get; set; } = "count";
    public bool IsRecurring { get; set; } = true;
    public List<DayOfWeek> SpecificDays { get; set; } = new();
    public string FromTime { get; set; } = "06:00";
    public string ToTime { get; set; } = "07:00";
}

public sealed class RoutineLog : EntityBase
{
    public Guid RoutineId { get; set; }
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public RoutineStatus Status { get; set; } = RoutineStatus.Default;
}

public sealed class TaskItem : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public decimal TargetHours { get; set; }
    public ScheduleType ScheduleType { get; set; } = ScheduleType.Daily;
    public List<DayOfWeek> SpecificDays { get; set; } = new();
}

public sealed class TaskLog : EntityBase
{
    public Guid TaskId { get; set; }
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public decimal Hours { get; set; }
    public bool Ignored { get; set; }
}

public sealed class ActionItem : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public bool IsDone { get; set; }
}

public sealed class GoalCategory : EntityBase
{
    public string Name { get; set; } = string.Empty;
}

public sealed class Goal : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public DateOnly TargetDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddMonths(1));
    public bool IsCompleted { get; set; }
}

public sealed class LifePrinciple : EntityBase
{
    public string Text { get; set; } = string.Empty;
}

public sealed class VisualizationItem : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public bool IsTangible { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsAiGenerated { get; set; }
    public string AiProvider { get; set; } = string.Empty;
}

public sealed class MoodLog : EntityBase
{
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public int Scale { get; set; } = 3;
    public string Notes { get; set; } = string.Empty;
}

public sealed class AppSettings : EntityBase
{
    public string PassphraseHash { get; set; } = string.Empty;
    public ThemeMode ThemeMode { get; set; } = ThemeMode.Light;
    public string ProfileName { get; set; } = "User";
    public string ProfileEmail { get; set; } = string.Empty;
}
