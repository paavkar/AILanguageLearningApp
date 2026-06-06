using AILanguageLearningApp.PageModels;
using AILanguageLearningApp.Services.LLM;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace AILanguageLearningApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit(options =>
                {
                    options.SetShouldEnableSnackbarOnWindows(true);
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<ILlmService, LlmService>();

            builder.Services.AddTransient<MainPageModel>();

            return builder.Build();
        }
    }
}