using AILanguageLearningApp.Services.LLM;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AILanguageLearningApp.PageModels
{
    public partial class MainPageModel : ObservableObject, INotifyPropertyChanged
    {
        private readonly ILlmService _llmService;
        private string _selectedHeavyModel;
        private string _selectedLightModel;
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

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoadModelsCommand { get; }

        public MainPageModel(ILlmService llmService)
        {
            _llmService = llmService;
            LoadModelsCommand = new Command(async () => await LoadModelsAsync());
            _ = LoadModelsAsync();
        }

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
