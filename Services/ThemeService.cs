using Microsoft.JSInterop;

namespace Life_Enhancer_Habit_Tracker.Services;

public sealed class ThemeService(IJSRuntime jsRuntime)
{
    public ValueTask ApplyThemeAsync(string mode) => jsRuntime.InvokeVoidAsync("habitDb.applyTheme", mode);

    public ValueTask<string> GetThemeAsync() => jsRuntime.InvokeAsync<string>("habitDb.getTheme");
}
