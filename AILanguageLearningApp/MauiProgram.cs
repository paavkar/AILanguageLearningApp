using AILanguageLearningApp.Services.LLM;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Syncfusion.Maui.Toolkit.Hosting;

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
                .ConfigureSyncfusionToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });
#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<DatabaseInitializer>();
            builder.Services.AddSingleton<UserAccountRepository>();
            builder.Services.AddSingleton<TaskRepository>();
            builder.Services.AddSingleton<ExerciseRepository>();
            builder.Services.AddSingleton<LessonRepository>();
            builder.Services.AddSingleton<CourseRepository>();
            builder.Services.AddSingleton<ILlmService, LlmService>();
            builder.Services.AddTransient<LessonFunctions>();

            IKernelBuilder kernelBuilder = builder.Services.AddKernel();

            kernelBuilder.Plugins.AddFromType<LessonFunctions>("LanguagePlugin");

            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<SettingsPageModel>();

            MauiApp app = builder.Build();

            return app;
        }
    }
}