using Life_Enhancer_Habit_Tracker.Models;

namespace Life_Enhancer_Habit_Tracker.Services;

public sealed class NotificationService(ActionService actionService, RoutineService routineService)
{
    public async Task<List<NotificationItem>> GetNotificationsAsync()
    {
        var notifications = new List<NotificationItem>();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);

        var actions = await actionService.GetActionsAsync();
        var overdueCount = actions.Count(x => !x.IsDone && x.DueDate < today);
        var tomorrowCount = actions.Count(x => !x.IsDone && x.DueDate == tomorrow);

        if (overdueCount > 0)
        {
            notifications.Add(new NotificationItem { Message = $"{overdueCount} overdue action(s)", Severity = "danger" });
        }

        if (tomorrowCount > 0)
        {
            notifications.Add(new NotificationItem { Message = $"{tomorrowCount} action(s) due tomorrow", Severity = "warn" });
        }

        var pendingRoutines = await routineService.GetPendingRoutinesCountAsync(today);
        if (pendingRoutines > 0)
        {
            notifications.Add(new NotificationItem { Message = $"{pendingRoutines} routine(s) pending today", Severity = "info" });
        }

        return notifications;
    }
}
