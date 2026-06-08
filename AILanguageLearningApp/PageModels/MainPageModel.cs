using AILanguageLearningApp.Models;
using AILanguageLearningApp.Services.LLM;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;

namespace AILanguageLearningApp.PageModels
{
    public partial class MainPageModel : ObservableObject, INotifyPropertyChanged, IRecipient<TextChunkReceivedMessage>
    {
        private readonly ILlmService _llmService;
        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage = "";

        public MainPageModel(ILlmService llmService)
        {
            _llmService = llmService;
        }

        public void Receive(TextChunkReceivedMessage message)
        {
            System.Diagnostics.Debug.WriteLine($"Received: {message.Value}");
            // MVVM Toolkit handles thread safety for basic property updates 
            // bound to the UI, but if you do complex UI logic, use MainThread here.
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                StatusMessage += message.Value;
            });
        }

        [RelayCommand]
        public async Task CreateLesson()
        {
            IsLoading = true;
            var input = new
            {
                Language = "Japanese",
                Topic = "Ordering at a restaurant",
                Level = "A1",
            };
            _ = Task.Run(async () =>
            {
                await _llmService.CheckUserResponseAsync("水をください", input.Topic, input.Language);
                IsLoading = false;
            });
        }
    }
}
