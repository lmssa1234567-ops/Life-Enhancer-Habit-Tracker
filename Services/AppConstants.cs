namespace Life_Enhancer_Habit_Tracker.Services;

public static class AppConstants
{
    public const string DatabaseName = "HabitTrackerDB";
    public const int DatabaseVersion = 1;

    public const string RoutinesStore = "routines";
    public const string RoutineLogsStore = "routineLogs";
    public const string TasksStore = "tasks";
    public const string TaskLogsStore = "taskLogs";
    public const string ActionsStore = "actions";
    public const string GoalsStore = "goals";
    public const string GoalCategoriesStore = "goalCategories";
    public const string LifePrinciplesStore = "lifePrinciples";
    public const string VisualizationsStore = "visualizations";
    public const string MoodLogsStore = "moodLogs";
    public const string SettingsStore = "settings";

    public const string DefaultPassphrase = "Jay Shree Krushna";
    public static readonly Guid SettingsId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static readonly string[] AllStores =
    {
        RoutinesStore,
        RoutineLogsStore,
        TasksStore,
        TaskLogsStore,
        ActionsStore,
        GoalsStore,
        GoalCategoriesStore,
        LifePrinciplesStore,
        VisualizationsStore,
        MoodLogsStore,
        SettingsStore
    };
}
