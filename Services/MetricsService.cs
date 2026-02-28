using Life_Enhancer_Habit_Tracker.Models;

namespace Life_Enhancer_Habit_Tracker.Services;

public sealed class MetricsService(RoutineService routineService, TaskService taskService, MoodService moodService)
{
    public async Task<DashboardMetrics> GetMetricsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var weekStart = today.AddDays(-6);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var yearStart = new DateOnly(today.Year, 1, 1);

        var weeklyLogs = await routineService.GetLogsForDateRangeAsync(weekStart, today);
        var monthlyLogs = await routineService.GetLogsForDateRangeAsync(monthStart, today);
        var yearlyLogs = await routineService.GetLogsForDateRangeAsync(yearStart, today);

        var tasks = await taskService.GetTasksAsync();
        var taskLogs = await taskService.GetLogsAsync(weekStart, today);
        var moodLogs = (await moodService.GetLogsAsync()).Take(14).ToList();

        return new DashboardMetrics
        {
            WeeklyCompletionPercent = Completion(weeklyLogs),
            MonthlyCompletionPercent = Completion(monthlyLogs),
            YearlyCompletionPercent = Completion(yearlyLogs),
            TaskPerformanceRatio = TaskRatio(tasks, taskLogs),
            MoodAverage = moodLogs.Count == 0 ? 0 : moodLogs.Average(x => x.Scale)
        };
    }

    private static double Completion(IEnumerable<RoutineLog> logs)
    {
        var data = logs.ToList();
        if (data.Count == 0)
        {
            return 0;
        }

        var done = data.Count(x => x.Status == RoutineStatus.Followed);
        return Math.Round((double)done / data.Count * 100, 1);
    }

    private static double TaskRatio(IEnumerable<TaskItem> tasks, IEnumerable<TaskLog> logs)
    {
        var taskList = tasks.ToList();
        var logList = logs.Where(x => !x.Ignored).ToList();
        if (taskList.Count == 0 || logList.Count == 0)
        {
            return 0;
        }

        var totalTarget = taskList.Sum(x => x.TargetHours);
        if (totalTarget <= 0)
        {
            return 0;
        }

        var totalActual = logList.Sum(x => x.Hours);
        return Math.Round((double)(totalActual / totalTarget), 2);
    }
}
