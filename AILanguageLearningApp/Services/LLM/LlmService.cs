using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace AILanguageLearningApp.Services.LLM
{
    public class LlmService : ILlmService
    {
        private readonly HttpClient _ollamaClient = new()
        {
            BaseAddress = new Uri("http://localhost:11434"),
            Timeout = TimeSpan.FromMinutes(10)
        };

        private Kernel _kernel;

        private IChatCompletionService _heavyChatService;
        private IChatCompletionService _lightChatService;

        private string _heavyModelId = "gemma4:12b";
        private string _lightModelId = "gemma4:4b";

        private readonly ChatHistory _history = [];
        private readonly ChatHistory _lightHistory = [];
        private readonly ILogger<LlmService> _logger;

        private readonly OllamaPromptExecutionSettings _heavySettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        private readonly OllamaPromptExecutionSettings _lightSettings = new();

        private string _userLanguage = "English";

        public string HeavyModelId
        {
            get => _heavyModelId;
            set { if (_heavyModelId != value) { _heavyModelId = value; InitializeServices(); } }
        }

        public string LightModelId
        {
            get => _lightModelId;
            set { if (_lightModelId != value) { _lightModelId = value; InitializeServices(); } }
        }

        public LlmService(Kernel kernel, ILogger<LlmService> logger)
        {
            _kernel = kernel;
            _logger = logger;

            InitializeServices();
            _ = InitializeAsync();
            SetupSystemPrompt();
        }

        private void InitializeServices()
        {
            try
            {
                IKernelBuilder builder = Kernel.CreateBuilder();

                if (DeviceInfo.Current.Platform == DevicePlatform.Android ||
                    DeviceInfo.Current.Platform == DevicePlatform.iOS)
                {
                    var baseModelPath = FileSystem.AppDataDirectory;
                    var heavyModelFolderPath = Path.Combine(baseModelPath, _heavyModelId);
                    var lightModelFolderPath = Path.Combine(baseModelPath, _lightModelId);

                    if (Directory.Exists(heavyModelFolderPath))
                    {
                        builder.AddOnnxRuntimeGenAIChatCompletion(
                            modelId: _heavyModelId,
                            serviceId: "OnnxHeavy",
                            modelPath: heavyModelFolderPath
                        );
                    }

                    if (Directory.Exists(lightModelFolderPath))
                    {
                        builder.AddOnnxRuntimeGenAIChatCompletion(
                            modelId: _lightModelId,
                            serviceId: "OnnxLight",
                            modelPath: lightModelFolderPath
                        );
                    }

                    Kernel internalKernel = builder.Build();

                    _heavyChatService = internalKernel.GetRequiredService<IChatCompletionService>("OnnxHeavy");
                    _lightChatService = internalKernel.GetRequiredService<IChatCompletionService>("OnnxLight");
                }
                else
                {
#pragma warning disable SKEXP0070
                    builder.AddOllamaChatCompletion(
                        serviceId: "OllamaHeavy",
                        modelId: _heavyModelId,
                        httpClient: _ollamaClient
                    );

                    builder.AddOllamaChatCompletion(
                        serviceId: "OllamaLight",
                        modelId: _lightModelId,
                        httpClient: _ollamaClient
                    );
#pragma warning restore SKEXP0070

                    Kernel internalKernel = builder.Build();

                    _heavyChatService = internalKernel.GetRequiredService<IChatCompletionService>("OllamaHeavy");
                    _lightChatService = internalKernel.GetRequiredService<IChatCompletionService>("OllamaLight");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing dual-model configurations.");
            }
        }

        private void SetupSystemPrompt()
        {
            var heavySystemMessage = $$"""
                [ROLE]
                You are an automated language content generator. Your sole function is to call tool functions using valid parameters.

                [WORKFLOW]
                1. Determine the correct function to execute:
                   - To start a new lesson, call 'CreateLesson'.
                   - To add content to an existing lesson, call 'AddLessonExercises'.
                2. Build the 'exercisesJson' parameter as a strict JSON string representing an array of exercises, matching the exact counts requested.

                [CRITICAL CONSTRAINTS]
                - User's native language is: {{_userLanguage}}. The 'instructions' and 'nativeLanguageContent' values inside tasks MUST use this language.
                - 'taskType' must be one of: Vocabulary, Grammar, Reading, Writing, Translation, Listening, Speaking.
                - The user input contains the amount of exercises and tasks in each exercise that you need to generate. Follow those counts exactly in the JSON you produce.

                [exercisesJson MINIMAL STRING SCHEMA EXAMPLE]
                [
                  {
                    "tasks": [
                      {
                            "taskType": "Translation",
                            "targetLanguageContent": "Bonjour, comment ça va ?",
                            "nativeLanguageContent": "Hello, how is it going?",
                            "instructions": "Translate the sentence into your native language.",
                            "choices": null,
                            "correctAnswer": "Hello, how is it going?"
                          },
                          {
                            "taskType": "Writing",
                            "targetLanguageContent": "",
                            "nativeLanguageContent": null,
                            "instructions": "Write a short paragraph (3-4 sentences) in the target language describing your favorite hobby.",
                            "choices": null,
                            "correctAnswer": null
                          },
                          {
                            "taskType": "Speaking",
                            "targetLanguageContent": "La vie est belle.",
                            "nativeLanguageContent": "Life is beautiful.",
                            "instructions": "Read the target language text aloud, focusing on clear pronunciation.",
                            "choices": null,
                            "correctAnswer": "La vie est belle."
                          },
                          {
                            "taskType": "Listening",
                            "targetLanguageContent": "Bonjour tout le monde. En raison de travaux sur les voies entre Paris et Lyon, le train numéro 452 départ initialement prévu à neuf heures aura du retard. Le train partira finalement à dix heures de la gare centrale, quai numéro trois. Nous nous excusons pour ce désagrément.",
                            "nativeLanguageContent": null,
                            "instructions": "Listen closely to the announcement and answer: What time will the train actually depart?",
                            "choices": {
                              "A": "At 9:00",
                              "B": "At 10:00",
                              "C": "At 3:00"
                            },
                            "correctAnswer": "B"
                          },
                          {
                            "taskType": "Vocabulary",
                            "targetLanguageContent": "pomme",
                            "nativeLanguageContent": "apple",
                            "instructions": "What is the meaning of the target language word 'pomme'?",
                            "choices": {
                              "A": "Banana",
                              "B": "Apple",
                              "C": "Orange"
                            },
                            "correctAnswer": "B"
                          },
                          {
                            "taskType": "Grammar",
                            "targetLanguageContent": "Ils ____ (être) fatigués ce soir.",
                            "nativeLanguageContent": null,
                            "instructions": "Complete the sentence by conjugating the verb 'être' correctly for the pronoun 'Ils'.",
                            "choices": {
                              "A": "suis",
                              "B": "êtes",
                              "C": "sont"
                            },
                            "correctAnswer": "C"
                          },
                          {
                            "taskType": "Reading",
                            "targetLanguageContent": "Chaque samedi matin, Lucas se rend au marché local pour acheter des fruits frais et du pain artisanal. Aujourd'hui, le marché est exceptionnellement bondé car il fait un soleil magnifique. Après avoir fait ses courses, il s'arrête toujours au petit café de la place pour lire le journal en buvant un expresso.",
                            "nativeLanguageContent": null,
                            "instructions": "Read the text carefully and answer the following question: What does Lucas always do immediately after finishing his grocery shopping at the market?",
                            "choices": {
                              "A": "He goes home to make breakfast with his fresh fruit.",
                              "B": "He buys an artisanal bread from the local bakery.",
                              "C": "He visits a small cafe on the square to drink an espresso and read."
                            },
                            "correctAnswer": "C"
                          }
                    ]
                  }
                ]

                Invoke the required function tool now with all arguments satisfied.
            """;
            _history.AddSystemMessage(heavySystemMessage);

            var lightSystemMessage = $$"""
                [ROLE]
                You are an automated language content generator.

                You will be provided with a user's answer to a task, the instructions for that task and the language that the answer should be in.
                Your function is to evaluate whether the user's answer is correct or not, and provide a brief explanation in the user's native language.
                
                [CRITICAL CONSTRAINTS]
                - User's native language is: {{_userLanguage}}. Use this language to respond to the user.
            """;
            _lightHistory.AddSystemMessage(lightSystemMessage);
        }

        private async Task InitializeAsync()
        {
            try
            {
                _userLanguage = await SecureStorage.GetAsync("userLanguage") ?? "English";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load user language from secure storage.");
            }
        }

        public async Task CreateNewLessonAsync(string language, string exerciseTopic, string proficiencyLevel, int exerciseCount, int taskCount)
        {
            var input = $$"""
                Use 'CreateLesson' to generate new lesson with exercises:
                Language: {{language}}
                Topic: {{exerciseTopic}}
                Level: {{proficiencyLevel}}
                Exercise Count: {{exerciseCount}}
                Task Count: {{taskCount}}
            """;
            _history.AddUserMessage(input);
            await GenerateResponseAsync(_heavyChatService, _heavySettings, _kernel);
        }

        public async Task CreateNewExercisesAsync(Guid lessonId, string language, string exerciseTopic, string proficiencyLevel, int exerciseCount, int taskCount)
        {
            var input = $$"""
                Use 'AddLessonExercises' to add exercises to the existing lesson:
                LessonId: {{lessonId}}
                Language: {{language}}
                Topic: {{exerciseTopic}}
                Level: {{proficiencyLevel}}
                Exercise Count: {{exerciseCount}}
                Task Count: {{taskCount}}
             """;
            _history.AddUserMessage(input);
            await GenerateResponseAsync(_heavyChatService, _heavySettings, _kernel);
        }

        public async Task CheckUserResponseAsync(string userResponse, string instructions, string language)
        {
            var input = $$"""
                Check the user's answer to a task:
                User Response: {{userResponse}}
                Instructions: {{instructions}}
                Language: {{language}}
                Provide feedback on whether the user's response is correct or not, and offer a brief explanation.
            """;
            _lightHistory.AddUserMessage(input);
            await GenerateResponseAsync(_lightChatService, _lightSettings);
        }

        public async Task GenerateResponseAsync(IChatCompletionService chat, OllamaPromptExecutionSettings settings, Kernel kernel = null)
        {
            StringBuilder assistantMessage = new();
            try
            {
                await foreach (StreamingChatMessageContent token in chat.GetStreamingChatMessageContentsAsync(
                    kernel is null ? _lightHistory : _history,
                    executionSettings: settings,
                    kernel: kernel))
                {
                    if (!string.IsNullOrEmpty(token.Content))
                    {
                        assistantMessage.Append(token.Content);
                    }
                }
                await AppShell.DisplaySnackbarAsync(assistantMessage.ToString()); // Change this to something more appropriate

                if (kernel is null)
                {
                    _lightHistory.AddAssistantMessage(assistantMessage.ToString());
                }
                else
                {
                    _history.AddAssistantMessage(assistantMessage.ToString());
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the response from the LLM.");
            }
        }

        public async Task<List<string>> GetAvailableModelsAsync()
        {
            OllamaTagsResponse? response = await _ollamaClient.GetFromJsonAsync<OllamaTagsResponse>("/api/tags");
            List<string> models = response?.Models?.Select(m => m.Name).ToList() ?? [];
            return models;
        }
    }

    public record OllamaTagsResponse([property: JsonPropertyName("models")] List<OllamaModelInfo> Models);
    public record OllamaModelInfo([property: JsonPropertyName("name")] string Name);
}
