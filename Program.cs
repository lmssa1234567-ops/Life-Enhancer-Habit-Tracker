using Life_Enhancer_Habit_Tracker;
using Life_Enhancer_Habit_Tracker.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IIndexedDbService, IndexedDbService>();
builder.Services.AddScoped<PassphraseService>();
builder.Services.AddScoped<RoutineService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<ActionService>();
builder.Services.AddScoped<GoalService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<MoodService>();
builder.Services.AddScoped<LifePrincipleService>();
builder.Services.AddScoped<VisualizationService>();
builder.Services.AddScoped<MetricsService>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<SettingsService>();

await builder.Build().RunAsync();
