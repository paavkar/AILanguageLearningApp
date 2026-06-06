using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Text;

namespace AILanguageLearningApp.Services.LLM
{
    public class LlmService : ILlmService
    {
        readonly IKernelBuilder builder = Kernel.CreateBuilder();
        readonly HttpClient ollamaClient = new()
        {
            BaseAddress = new Uri("http://localhost:11434"),
            Timeout = TimeSpan.FromMinutes(10) // Allow up to 10 minutes for massive nested JSON generations
        };
        readonly Kernel Kernel;
        readonly IChatCompletionService Chat;
        readonly ChatHistory History = [];
        readonly OllamaPromptExecutionSettings Settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        string userLanguage = SecureStorage.GetAsync("userLanguage").GetAwaiter().GetResult() ?? "English";
        ILogger<LlmService> Logger;

        public LlmService(ILogger<LlmService> logger)
        {
            builder.AddOllamaChatCompletion(
                modelId: "gemma4:12b",
                httpClient: ollamaClient
            );
            Kernel = builder.Build();

            Chat = Kernel.GetRequiredService<IChatCompletionService>();

            var systemMessage = $$"""
                [ROLE]
                You are an automated language content generator. Your sole function is to call tool functions using valid parameters. Do not converse with the user.

                [WORKFLOW]
                1. Determine the correct function to execute:
                   - To start a new lesson, call 'CreateLesson'.
                   - To add content to an existing lesson, call 'AddLessonExercises'.
                2. Build the 'exercisesJson' parameter as a strict JSON string representing an array of exercises, matching the exact counts requested.

                [CRITICAL CONSTRAINTS]
                - User's native language is: {{userLanguage}}. The 'instructions' and 'nativeLanguageContent' values inside tasks MUST use this language.
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
            History.AddSystemMessage(systemMessage);
            Logger = logger;
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
            History.AddUserMessage(input);
            await GenerateResponseAsync();
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
            History.AddUserMessage(input);
            await GenerateResponseAsync();
        }

        public async Task GenerateResponseAsync()
        {
            StringBuilder assistantMessage = new();
            try
            {
                await foreach (StreamingChatMessageContent token in Chat.GetStreamingChatMessageContentsAsync(
                    History,
                    executionSettings: Settings,
                    kernel: Kernel))
                {
                    if (!string.IsNullOrEmpty(token.Content))
                    {
                        assistantMessage.Append(token.Content);
                    }
                }
                History.AddAssistantMessage(assistantMessage.ToString());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred while generating the response from the LLM.");
            }
        }
    }
}
