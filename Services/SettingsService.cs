using System.Text.RegularExpressions;
using Life_Enhancer_Habit_Tracker.Models;

namespace Life_Enhancer_Habit_Tracker.Services;

public sealed class SettingsService(IIndexedDbService indexedDbService)
{
    public async Task<AppSettings> GetSettingsAsync()
    {
        await indexedDbService.InitializeAsync();
        var settings = await indexedDbService.GetAllAsync<AppSettings>(AppConstants.SettingsStore);
        var existing = settings.FirstOrDefault(x => x.Id == AppConstants.SettingsId);
        if (existing is not null)
        {
            return existing;
        }

        var created = new AppSettings
        {
            Id = AppConstants.SettingsId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await indexedDbService.UpsertAsync(AppConstants.SettingsStore, created);
        return created;
    }

    public async Task SaveProfileAsync(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 60)
        {
            throw new InvalidOperationException("Profile name is invalid.");
        }

        if (!string.IsNullOrWhiteSpace(email) && !Regex.IsMatch(email, "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$"))
        {
            throw new InvalidOperationException("Profile email format is invalid.");
        }

        var settings = await GetSettingsAsync();
        settings.ProfileName = name.Trim();
        settings.ProfileEmail = email.Trim();
        await indexedDbService.UpsertAsync(AppConstants.SettingsStore, settings);
    }
}
