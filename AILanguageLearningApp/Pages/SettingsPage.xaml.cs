namespace AILanguageLearningApp.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}