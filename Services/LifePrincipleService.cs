using Life_Enhancer_Habit_Tracker.Models;

namespace Life_Enhancer_Habit_Tracker.Services;

public sealed class LifePrincipleService(IIndexedDbService indexedDbService)
{
    public async Task<List<LifePrinciple>> GetAsync() =>
        (await indexedDbService.GetAllAsync<LifePrinciple>(AppConstants.LifePrinciplesStore)).Where(x => !x.IsDeleted).OrderBy(x => x.Text).ToList();

    public async Task SaveAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > 280)
        {
            throw new InvalidOperationException("Principle text is invalid.");
        }

        var item = new LifePrinciple
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Text = text.Trim()
        };

        await indexedDbService.UpsertAsync(AppConstants.LifePrinciplesStore, item);
    }
}
