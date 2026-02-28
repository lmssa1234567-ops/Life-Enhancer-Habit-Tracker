using Life_Enhancer_Habit_Tracker.Models;

namespace Life_Enhancer_Habit_Tracker.Services;

public interface IIndexedDbService
{
    Task InitializeAsync();
    Task<List<T>> GetAllAsync<T>(string storeName);
    Task UpsertAsync<T>(string storeName, T entity) where T : EntityBase;
    Task DeleteAsync(string storeName, Guid id);
    Task ClearStoreAsync(string storeName);
    Task<string> ExportJsonAsync();
    Task ImportJsonAsync(string json);
}
