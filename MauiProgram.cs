using Microsoft.Extensions.Logging;
using JournalApps.Data;
using JournalApps.Services;

namespace JournalApps
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            // Initialize database
            var database = new AppDatabase();
            Task.Run(async () => await database.InitAsync()).Wait(); // Initialize tables synchronously here

            // Register database and services as singletons
            builder.Services.AddSingleton(database);
            builder.Services.AddSingleton<DailyLimitService>();
            builder.Services.AddSingleton<JournalService>();
            builder.Services.AddSingleton<PdfExportService>();
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<ThemeService>();


            return builder.Build();
        }
    }
}
