using Life_Enhancer_Habit_Tracker.Models;

namespace Life_Enhancer_Habit_Tracker.Services;

public sealed class GoalService(IIndexedDbService indexedDbService)
{
    public async Task<List<GoalCategory>> GetCategoriesAsync() =>
        (await indexedDbService.GetAllAsync<GoalCategory>(AppConstants.GoalCategoriesStore)).Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToList();

    public async Task<List<Goal>> GetGoalsAsync() =>
        (await indexedDbService.GetAllAsync<Goal>(AppConstants.GoalsStore)).Where(x => !x.IsDeleted).OrderBy(x => x.TargetDate).ToList();

    public async Task SaveCategoryAsync(GoalCategory category)
    {
        if (string.IsNullOrWhiteSpace(category.Name) || category.Name.Length > 40)
        {
            throw new InvalidOperationException("Category name is invalid.");
        }

        if (category.Id == Guid.Empty)
        {
            category.Id = Guid.NewGuid();
            category.CreatedAt = DateTime.UtcNow;
        }

        await indexedDbService.UpsertAsync(AppConstants.GoalCategoriesStore, category);
    }

    public async Task SaveGoalAsync(Goal goal)
    {
        if (string.IsNullOrWhiteSpace(goal.Name) || goal.Name.Length > 80)
        {
            throw new InvalidOperationException("Goal name is invalid.");
        }

        if (goal.Id == Guid.Empty)
        {
            goal.Id = Guid.NewGuid();
            goal.CreatedAt = DateTime.UtcNow;
        }

        await indexedDbService.UpsertAsync(AppConstants.GoalsStore, goal);
    }

    public async Task ToggleGoalAsync(Guid goalId)
    {
        var goals = await GetGoalsAsync();
        var goal = goals.FirstOrDefault(x => x.Id == goalId);
        if (goal is null)
        {
            return;
        }

        goal.IsCompleted = !goal.IsCompleted;
        await indexedDbService.UpsertAsync(AppConstants.GoalsStore, goal);
    }
}
