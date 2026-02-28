using Life_Enhancer_Habit_Tracker.Models;

namespace Life_Enhancer_Habit_Tracker.Services;

public sealed class MoodService(IIndexedDbService indexedDbService)
{
    public async Task<List<MoodLog>> GetLogsAsync() =>
        (await indexedDbService.GetAllAsync<MoodLog>(AppConstants.MoodLogsStore)).Where(x => !x.IsDeleted).OrderByDescending(x => x.Date).ToList();

    public async Task SaveTodayAsync(int scale, string notes)
    {
        if (scale < 1 || scale > 5)
        {
            throw new InvalidOperationException("Mood scale must be from 1 to 5.");
        }

        if (notes.Length > 400)
        {
            throw new InvalidOperationException("Notes must be 400 chars or less.");
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var logs = await GetLogsAsync();
        var existing = logs.FirstOrDefault(x => x.Date == today);
        if (existing is null)
        {
            existing = new MoodLog
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Date = today,
                Scale = scale,
                Notes = notes
            };
        }
        else
        {
            existing.Scale = scale;
            existing.Notes = notes;
        }

        await indexedDbService.UpsertAsync(AppConstants.MoodLogsStore, existing);
    }
}
