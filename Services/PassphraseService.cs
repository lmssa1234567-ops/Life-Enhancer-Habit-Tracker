using System.Security.Cryptography;
using System.Text;
using Life_Enhancer_Habit_Tracker.Models;

namespace Life_Enhancer_Habit_Tracker.Services;

public sealed class PassphraseService(IIndexedDbService indexedDbService)
{
    public async Task EnsureInitializedAsync()
    {
        await indexedDbService.InitializeAsync();

        var settings = await GetSettingsAsync();
        if (string.IsNullOrWhiteSpace(settings.PassphraseHash))
        {
            settings.PassphraseHash = ComputeHash(AppConstants.DefaultPassphrase);
            await indexedDbService.UpsertAsync(AppConstants.SettingsStore, settings);
        }
    }

    public async Task<bool> ValidateAsync(string passphrase)
    {
        if (string.IsNullOrWhiteSpace(passphrase))
        {
            return false;
        }

        var settings = await GetSettingsAsync();
        return settings.PassphraseHash == ComputeHash(passphrase);
    }

    public async Task SetPassphraseAsync(string newPassphrase)
    {
        if (string.IsNullOrWhiteSpace(newPassphrase) || newPassphrase.Length > 120)
        {
            throw new InvalidOperationException("Passphrase length is invalid.");
        }

        var settings = await GetSettingsAsync();
        settings.PassphraseHash = ComputeHash(newPassphrase);
        await indexedDbService.UpsertAsync(AppConstants.SettingsStore, settings);
    }

    private async Task<AppSettings> GetSettingsAsync()
    {
        var settings = await indexedDbService.GetAllAsync<AppSettings>(AppConstants.SettingsStore);
        var first = settings.FirstOrDefault(x => x.Id == AppConstants.SettingsId);

        if (first is not null)
        {
            return first;
        }

        return new AppSettings
        {
            Id = AppConstants.SettingsId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ThemeMode = ThemeMode.Light
        };
    }

    private static string ComputeHash(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
