using System.Net.Http.Headers;
using System.Text.Json;
using Life_Enhancer_Habit_Tracker.Models;

namespace Life_Enhancer_Habit_Tracker.Services;

public sealed class VisualizationService(
    IIndexedDbService indexedDbService,
    GoalService goalService,
    MoodService moodService,
    HttpClient httpClient)
{
    public async Task<List<VisualizationItem>> GetAsync() =>
        (await indexedDbService.GetAllAsync<VisualizationItem>(AppConstants.VisualizationsStore)).Where(x => !x.IsDeleted).OrderByDescending(x => x.CreatedAt).ToList();

    public async Task SaveAsync(VisualizationItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Title) || item.Title.Length > 80)
        {
            throw new InvalidOperationException("Visualization title is invalid.");
        }

        if (string.IsNullOrWhiteSpace(item.Content) || item.Content.Length > 1200)
        {
            throw new InvalidOperationException("Visualization content is invalid.");
        }

        if (item.Id == Guid.Empty)
        {
            item.Id = Guid.NewGuid();
            item.CreatedAt = DateTime.UtcNow;
        }

        await indexedDbService.UpsertAsync(AppConstants.VisualizationsStore, item);
    }

    public async Task<VisualizationItem> GenerateAiVisualizationAsync()
    {
        var goals = await goalService.GetGoalsAsync();
        var moods = await moodService.GetLogsAsync();

        var activeGoals = goals.Where(x => !x.IsCompleted).Take(3).Select(x => x.Name).ToList();
        var moodAverage = moods.Take(7).Select(x => x.Scale).DefaultIfEmpty(3).Average();

        var prompt = BuildPrompt(activeGoals, moodAverage);
        var (externalText, provider) = await TryGenerateFromFreeApiAsync(prompt);

        var content = string.IsNullOrWhiteSpace(externalText)
            ? BuildFallbackContent(activeGoals, moodAverage)
            : externalText;

        return new VisualizationItem
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Title = "AI Motivation Snapshot",
            IsTangible = false,
            IsAiGenerated = true,
            AiProvider = string.IsNullOrWhiteSpace(externalText) ? "Local Fallback" : provider,
            Content = Truncate(content.Trim(), 1200)
        };
    }

    private static string BuildPrompt(IReadOnlyCollection<string> activeGoals, double moodAverage)
    {
        var goalsText = activeGoals.Count == 0
            ? "create momentum with one meaningful habit today"
            : string.Join(", ", activeGoals);

        var moodText = moodAverage >= 4 ? "high" : moodAverage >= 3 ? "steady" : "low";

        return $"Write a short motivational visualization (80-140 words) for a habit tracker user. Mood level: {moodText}. Focus goals: {goalsText}. Keep it practical, positive, and specific.";
    }

    private async Task<(string text, string provider)> TryGenerateFromFreeApiAsync(string prompt)
    {
        var encodedPrompt = Uri.EscapeDataString(prompt);
        var candidates = new (string Url, string Provider)[]
        {
            ($"https://gen.pollinations.ai/text/{encodedPrompt}", "Pollinations Gen API (Free)"),
            ($"https://text.pollinations.ai/prompt/{encodedPrompt}", "Pollinations Legacy API (Free)")
        };

        foreach (var candidate in candidates)
        {
            var raw = await TryRequestTextAsync(candidate.Url);
            var normalized = NormalizeExternalText(raw);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                return (normalized, candidate.Provider);
            }
        }

        return (string.Empty, string.Empty);
    }

    private async Task<string> TryRequestTextAsync(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            using var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }

            var content = await response.Content.ReadAsStringAsync();
            return content?.Trim() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string NormalizeExternalText(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var text = raw.Trim();
        if (text.StartsWith('{') || text.StartsWith('['))
        {
            var extracted = TryExtractTextFromJson(text);
            if (!string.IsNullOrWhiteSpace(extracted))
            {
                text = extracted.Trim();
            }
        }

        if (LooksLikeApiNotice(text))
        {
            return string.Empty;
        }

        return text.Length < 40 ? string.Empty : text;
    }

    private static string TryExtractTextFromJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var key in new[] { "text", "response", "output", "content", "message", "result" })
                {
                    if (root.TryGetProperty(key, out var property) && property.ValueKind == JsonValueKind.String)
                    {
                        return property.GetString() ?? string.Empty;
                    }
                }

                if (root.TryGetProperty("choices", out var choices) &&
                    choices.ValueKind == JsonValueKind.Array &&
                    choices.GetArrayLength() > 0)
                {
                    var first = choices[0];
                    if (first.TryGetProperty("text", out var textNode) && textNode.ValueKind == JsonValueKind.String)
                    {
                        return textNode.GetString() ?? string.Empty;
                    }

                    if (first.TryGetProperty("message", out var messageNode) &&
                        messageNode.ValueKind == JsonValueKind.Object &&
                        messageNode.TryGetProperty("content", out var contentNode) &&
                        contentNode.ValueKind == JsonValueKind.String)
                    {
                        return contentNode.GetString() ?? string.Empty;
                    }
                }
            }

            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0 && root[0].ValueKind == JsonValueKind.String)
            {
                return root[0].GetString() ?? string.Empty;
            }
        }
        catch
        {
            return string.Empty;
        }

        return string.Empty;
    }

    private static bool LooksLikeApiNotice(string text)
    {
        var lowered = text.ToLowerInvariant();
        return lowered.Contains("important notice") ||
               lowered.Contains("deprecated") ||
               lowered.Contains("migrate to our new service") ||
               lowered.Contains("enter.pollinations.ai");
    }

    private static string BuildFallbackContent(IReadOnlyCollection<string> activeGoals, double moodAverage)
    {
        var tone = moodAverage >= 4 ? "confident" : moodAverage >= 3 ? "steady" : "gentle";
        var focus = activeGoals.Count == 0 ? "one meaningful win today" : string.Join(", ", activeGoals);

        return $"Visualize a {tone} day where you complete {focus}. Start with 20 focused minutes, remove one distraction, and finish with a short review. Keep the goal small but non-negotiable so momentum compounds by evening.";
    }

    private static string Truncate(string text, int maxLength)
    {
        if (text.Length <= maxLength)
        {
            return text;
        }

        return text[..maxLength];
    }
}
