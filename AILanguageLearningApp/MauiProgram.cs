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
            builder.Services.AddSingleton<UserAccountRepository>();
            builder.Services.AddSingleton<ILlmService, LlmService>();

            builder.Services.AddTransient<Kernel>(sp =>
            {
                IKernelBuilder builder = Kernel.CreateBuilder();

                builder.Plugins.AddFromObject(new LessonFunctions(), "LanguagePlugin");

                return builder.Build();
            });

            builder.Services.AddTransient<MainPageModel>();

            MauiApp app = builder.Build();

            return app;
        }
    }
}