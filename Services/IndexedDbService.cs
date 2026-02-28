using System.Text.Json;
using Life_Enhancer_Habit_Tracker.Models;
using Microsoft.JSInterop;

namespace Life_Enhancer_Habit_Tracker.Services;

public sealed class IndexedDbService(IJSRuntime jsRuntime) : IIndexedDbService
{
    private bool _initialized;

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await jsRuntime.InvokeVoidAsync("habitDb.ensureDb", AppConstants.DatabaseName, AppConstants.DatabaseVersion, AppConstants.AllStores);
        _initialized = true;
    }

    public async Task<List<T>> GetAllAsync<T>(string storeName)
    {
        await InitializeAsync();
        var data = await jsRuntime.InvokeAsync<List<T>>("habitDb.getAll", AppConstants.DatabaseName, storeName);
        return data ?? new List<T>();
    }

    public async Task UpsertAsync<T>(string storeName, T entity) where T : EntityBase
    {
        await InitializeAsync();
        entity.UpdatedAt = DateTime.UtcNow;
        await jsRuntime.InvokeVoidAsync("habitDb.upsert", AppConstants.DatabaseName, storeName, entity);
    }

    public async Task DeleteAsync(string storeName, Guid id)
    {
        await InitializeAsync();
        await jsRuntime.InvokeVoidAsync("habitDb.deleteRecord", AppConstants.DatabaseName, storeName, id.ToString());
    }

    public async Task ClearStoreAsync(string storeName)
    {
        await InitializeAsync();
        await jsRuntime.InvokeVoidAsync("habitDb.clearStore", AppConstants.DatabaseName, storeName);
    }

    public async Task<string> ExportJsonAsync()
    {
        await InitializeAsync();
        var payload = await jsRuntime.InvokeAsync<object>("habitDb.exportAll", AppConstants.DatabaseName, AppConstants.AllStores);
        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task ImportJsonAsync(string json)
    {
        await InitializeAsync();
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("Import payload is empty.");
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Import payload is not valid JSON.", ex);
        }

        using (doc)
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("Import payload must be a JSON object.");
            }

            foreach (var store in AppConstants.AllStores)
            {
                if (!doc.RootElement.TryGetProperty(store, out var storeNode) || storeNode.ValueKind != JsonValueKind.Array)
                {
                    throw new InvalidOperationException($"Import payload is missing a valid '{store}' array.");
                }
            }

            await jsRuntime.InvokeVoidAsync("habitDb.importAll", AppConstants.DatabaseName, doc.RootElement, AppConstants.AllStores);
        }
    }
}
