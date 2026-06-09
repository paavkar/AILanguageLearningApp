using AILanguageLearningApp.Models;
using AILanguageLearningApp.Services.LLM;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AILanguageLearningApp.PageModels
{
    public partial class SettingsPageModel : ObservableObject, INotifyPropertyChanged
    {
        private readonly ILlmService _llmService;
        private string _selectedHeavyModel;
        private string _selectedLightModel;

        [ObservableProperty]
        private bool _isLoading;
        [ObservableProperty]
        private string _userLanguage = "";

        public ObservableCollection<string> AvailableModels { get; } = [];

        public ObservableCollection<LanguageOption> AvailableLanguages { get; } = [];

        private LanguageOption _selectedLanguageOption;
        public LanguageOption SelectedLanguageOption
        {
            get => _selectedLanguageOption;
            set
            {
                if (SetProperty(ref _selectedLanguageOption, value))
                {
                    if (value != null)
                    {
                        UserLanguage = value.EnglishName;
                    }
                }
            }
        }

        public string SelectedHeavyModel
        {
            get => _selectedHeavyModel;
            set
            {
                if (SetProperty(ref _selectedHeavyModel, value))
                {
                    _llmService.HeavyModelId = value;
                }
            }
        }

        public string SelectedLightModel
        {
            get => _selectedLightModel;
            set
            {
                if (SetProperty(ref _selectedLightModel, value))
                {
                    _llmService.LightModelId = value;
                }
            }
        }

        public SettingsPageModel(ILlmService llmService)
        {
            _llmService = llmService;
            _ = LoadModelsAsync();
            List<LanguageOption> languages =
            [
                new() { NativeName = "English", EnglishName = "English" },
                new() { NativeName = "Deutsch", EnglishName = "German" },
                new() { NativeName = "Español", EnglishName = "Spanish" },
                new() { NativeName = "Français", EnglishName = "French" },
                new() { NativeName = "中文", EnglishName = "Chinese" },
                new() { NativeName = "Português", EnglishName = "Portuguese" },
                new() { NativeName = "日本語", EnglishName = "Japanese" },
            ];

            foreach (LanguageOption l in languages)
                AvailableLanguages.Add(l);

            Task.Run(async () =>
            {
                var saved = await SecureStorage.GetAsync("userLanguage") ?? "English";
                UserLanguage = saved;
                SelectedLanguageOption = AvailableLanguages.FirstOrDefault(a => a.EnglishName == saved)
                    ?? AvailableLanguages.FirstOrDefault()!;
            });
        }

        [RelayCommand]
        private async Task LoadModelsAsync()
        {
            IsLoading = true;
            try
            {
                List<string> models = await _llmService.GetAvailableModelsAsync();

                AvailableModels.Clear();
                foreach (var model in models)
                {
                    AvailableModels.Add(model);
                }

                if (AvailableModels.Count > 0)
                {
                    SelectedHeavyModel = AvailableModels.Contains("gemma4:12b") ? "gemma4:12b" : AvailableModels.First();
                    SelectedLightModel = AvailableModels.Contains("gemma4:e4b") ? "gemma4:e4b" : AvailableModels.First();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading Ollama models: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SetUserLanguage()
        {
            var toSave = SelectedLanguageOption?.EnglishName ?? UserLanguage ?? "English";
            await SecureStorage.SetAsync("userLanguage", toSave);
            _llmService.UserLanguage = toSave;
        }
    }
}
