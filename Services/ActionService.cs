using Life_Enhancer_Habit_Tracker.Models;

namespace Life_Enhancer_Habit_Tracker.Services;

public sealed class ActionService(IIndexedDbService indexedDbService)
{
    public async Task<List<ActionItem>> GetActionsAsync() =>
        (await indexedDbService.GetAllAsync<ActionItem>(AppConstants.ActionsStore)).Where(x => !x.IsDeleted).OrderBy(x => x.DueDate).ToList();

    public async Task SaveAsync(ActionItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Name) || item.Name.Length > 80)
        {
            throw new InvalidOperationException("Action name is invalid.");
        }

        if (item.Id == Guid.Empty)
        {
            item.Id = Guid.NewGuid();
            item.CreatedAt = DateTime.UtcNow;
        }

        await indexedDbService.UpsertAsync(AppConstants.ActionsStore, item);
    }

    public async Task ToggleDoneAsync(Guid id)
    {
        var all = await GetActionsAsync();
        var item = all.FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            return;
        }

        item.IsDone = !item.IsDone;
        await indexedDbService.UpsertAsync(AppConstants.ActionsStore, item);
    }
}
