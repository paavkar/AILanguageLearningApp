using AILanguageLearningApp.PageModels;

namespace AILanguageLearningApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}
