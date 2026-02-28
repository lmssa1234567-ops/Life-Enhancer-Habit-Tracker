using Life_Enhancer_Habit_Tracker.Models;

namespace Life_Enhancer_Habit_Tracker.Services;

public sealed class TaskService(IIndexedDbService indexedDbService)
{
    private static readonly List<DayOfWeek> FullWeekDays =
    [
        DayOfWeek.Sunday,
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday
    ];

    public async Task<List<TaskItem>> GetTasksAsync() =>
        (await indexedDbService.GetAllAsync<TaskItem>(AppConstants.TasksStore)).Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToList();

    public async Task SaveTaskAsync(TaskItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Name) || item.Name.Length > 60)
        {
            throw new InvalidOperationException("Task name is invalid.");
        }

        if (item.TargetHours < 0 || item.TargetHours > 24)
        {
            throw new InvalidOperationException("TargetHours must be between 0 and 24.");
        }

        if (item.ScheduleType == ScheduleType.Daily)
        {
            item.SpecificDays = FullWeekDays.ToList();
        }
        else
        {
            item.SpecificDays = item.SpecificDays.Distinct().ToList();
            if (item.SpecificDays.Count == 0)
            {
                throw new InvalidOperationException("Select at least one day for SpecificDays schedule.");
            }
        }

        if (item.Id == Guid.Empty)
        {
            item.Id = Guid.NewGuid();
            item.CreatedAt = DateTime.UtcNow;
        }

        await indexedDbService.UpsertAsync(AppConstants.TasksStore, item);
    }

    public async Task<List<TaskLog>> GetLogsAsync(DateOnly from, DateOnly to)
    {
        var all = await indexedDbService.GetAllAsync<TaskLog>(AppConstants.TaskLogsStore);
        return all.Where(x => x.Date >= from && x.Date <= to && !x.IsDeleted).ToList();
    }

    public async Task UpsertLogAsync(Guid taskId, DateOnly date, decimal hours, bool ignored)
    {
        if (hours < 0 || hours > 24)
        {
            throw new InvalidOperationException("Hours must be between 0 and 24.");
        }

        var all = await indexedDbService.GetAllAsync<TaskLog>(AppConstants.TaskLogsStore);
        var existing = all.FirstOrDefault(x => x.TaskId == taskId && x.Date == date && !x.IsDeleted);
        if (existing is null)
        {
            existing = new TaskLog
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                TaskId = taskId,
                Date = date
            };
        }

        existing.Hours = hours;
        existing.Ignored = ignored;
        await indexedDbService.UpsertAsync(AppConstants.TaskLogsStore, existing);
    }

    public static bool IsScheduledOnDate(TaskItem task, DateOnly date)
    {
        if (task.ScheduleType == ScheduleType.Daily)
        {
            return true;
        }

        return task.SpecificDays?.Contains(date.DayOfWeek) == true;
    }
}
