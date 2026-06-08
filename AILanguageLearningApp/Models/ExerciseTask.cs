using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AILanguageLearningApp.Models
{
    public class ExerciseTask
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid ExerciseId { get; set; }

        [Description("The content in the target language (i.e. language from user input) for the task. For Listening/Speaking, this is the text to be spoken/heard.")]
        [JsonPropertyName("targetLanguageContent")]
        public string TargetLanguageContent { get; set; }

        [Description("Translation or helper text in the user's native language.")]
        [JsonPropertyName("nativeLanguageContent")]
        public string? NativeLanguageContent { get; set; }

        [Description("Instructions for the user, written in their native language. MUST be provided.")]
        [JsonPropertyName("instructions")]
        public string Instructions { get; set; }

        [Description("The type of task. Allowed values: Vocabulary, Grammar, Reading, Writing, Translation, Listening, Speaking. MUST be provided.")]
        [JsonPropertyName("taskType")]
        public string TaskType { get; set; }

        [Description("The correct answer key (e.g., 'A', 'B', or 'C').")]
        [JsonPropertyName("correctAnswer")]
        public string? CorrectAnswer { get; set; }

        [Description("A key-value dictionary of multiple-choice options (e.g., A: Option 1, B: Option 2, C: Option 3). Null if not a multiple-choice question.")]
        [JsonPropertyName("choices")]
        public Dictionary<string, string>? Choices { get; set; }

        public override string ToString()
        {
            return $"""
                -------
                Task ID: {Id},
                Exercise ID: {ExerciseId},
                Target Language Content: {TargetLanguageContent},
                Native Language Content: {NativeLanguageContent},
                Instructions: {Instructions},
                Task Type: {TaskType},
                Options: {(Choices != null && Choices.Count > 0 ? string.Join(", ", Choices.Select(c => $"{c.Key}: {c.Value}")) : "None")},
                Correct Answer: {CorrectAnswer}
                """;
        }
    }
}
