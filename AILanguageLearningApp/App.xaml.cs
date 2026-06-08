using AILanguageLearningApp.Pages;

namespace AILanguageLearningApp
{
    public partial class App : Application
    {
        private readonly DatabaseInitializer _databaseInitializer;

        public App(DatabaseInitializer databaseInitializer)
        {
            InitializeComponent();
            _databaseInitializer = databaseInitializer;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new()
            {
                Page = new LoadingPage(),
                Title = "AI Language Learning App"
            };

            _ = InitializeRootPage(window);

            return window;
        }

        private async Task InitializeRootPage(Window window)
        {
            await _databaseInitializer.InitDatabase();
            var userLanguage = await SecureStorage.GetAsync("userLanguage");

            if (string.IsNullOrWhiteSpace(userLanguage))
            {
                await SecureStorage.SetAsync("userLanguage", "English");
            }

            window.Page = new AppShell();
        }
    }
}