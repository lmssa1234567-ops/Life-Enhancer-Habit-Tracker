using Life_Enhancer_Habit_Tracker.Models;

namespace Life_Enhancer_Habit_Tracker.Services;

public sealed class RoutineService(IIndexedDbService indexedDbService)
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

    public async Task<List<Routine>> GetRoutinesAsync() =>
        (await indexedDbService.GetAllAsync<Routine>(AppConstants.RoutinesStore)).Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToList();

    public async Task SaveRoutineAsync(Routine routine)
    {
        ValidateText(routine.Name, 60, nameof(routine.Name));
        ValidateText(routine.MeasurementType, 30, nameof(routine.MeasurementType));
        ValidateTimeRange(routine.FromTime, routine.ToTime);

        if (routine.ScheduleType == ScheduleType.Daily)
        {
            routine.SpecificDays = FullWeekDays.ToList();
        }
        else
        {
            routine.SpecificDays = routine.SpecificDays.Distinct().ToList();
            if (routine.SpecificDays.Count == 0)
            {
                throw new InvalidOperationException("Select at least one day for SpecificDays schedule.");
            }
        }

        if (routine.Id == Guid.Empty)
        {
            routine.Id = Guid.NewGuid();
            routine.CreatedAt = DateTime.UtcNow;
        }

        await indexedDbService.UpsertAsync(AppConstants.RoutinesStore, routine);
    }

    public async Task<List<RoutineLog>> GetLogsForDateRangeAsync(DateOnly from, DateOnly to)
    {
        var all = await indexedDbService.GetAllAsync<RoutineLog>(AppConstants.RoutineLogsStore);
        return all.Where(x => x.Date >= from && x.Date <= to && !x.IsDeleted).ToList();
    }

    public async Task SetStatusAsync(Guid routineId, DateOnly date, RoutineStatus status)
    {
        var all = await indexedDbService.GetAllAsync<RoutineLog>(AppConstants.RoutineLogsStore);
        var existing = all.FirstOrDefault(x => x.RoutineId == routineId && x.Date == date && !x.IsDeleted);

        if (existing is null)
        {
            existing = new RoutineLog
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                RoutineId = routineId,
                Date = date,
                Status = status
            };
        }
        else
        {
            existing.Status = status;
        }

        await indexedDbService.UpsertAsync(AppConstants.RoutineLogsStore, existing);
    }

    public async Task<int> GetPendingRoutinesCountAsync(DateOnly date)
    {
        var routines = await GetRoutinesAsync();
        var logs = await GetLogsForDateRangeAsync(date, date);

        return routines
            .Where(routine => IsScheduledOnDate(routine, date))
            .Count(routine =>
            {
                var existing = logs.FirstOrDefault(log => log.RoutineId == routine.Id);
                return existing is null || existing.Status == RoutineStatus.Default;
            });
    }

    public static bool IsScheduledOnDate(Routine routine, DateOnly date)
    {
        if (routine.ScheduleType == ScheduleType.Daily)
        {
            return true;
        }

        return routine.SpecificDays?.Contains(date.DayOfWeek) == true;
    }

    private static void ValidateText(string value, int maxLength, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength)
        {
            throw new InvalidOperationException($"{name} is invalid.");
        }
    }

    private static void ValidateTimeRange(string fromTime, string toTime)
    {
        if (!TimeOnly.TryParse(fromTime, out var from))
        {
            throw new InvalidOperationException("From time is invalid.");
        }

        if (!TimeOnly.TryParse(toTime, out var to))
        {
            throw new InvalidOperationException("To time is invalid.");
        }

        if (from >= to)
        {
            throw new InvalidOperationException("To time must be later than From time.");
        }
    }
}
