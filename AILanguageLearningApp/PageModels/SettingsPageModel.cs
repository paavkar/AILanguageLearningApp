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

        public ObservableCollection<string> AvailableModels { get; } = [];

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
    }
}
