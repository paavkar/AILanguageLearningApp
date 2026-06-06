using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AILanguageLearningApp.Models
{
    public class ExerciseTask
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid ExerciseId { get; set; }

        [JsonPropertyName("targetLanguageContent")]
        [Description("The content in the target language for the task. For Listening/Speaking, this is the text to be spoken/heard.")]
        public string TargetLanguageContent { get; set; }

        [JsonPropertyName("nativeLanguageContent")]
        [Description("The content in the native language for the task.")]
        public string? NativeLanguageContent { get; set; }

        [JsonPropertyName("instructions")]
        [Description("The instructions for the task.")]
        public string Instructions { get; set; }

        [JsonPropertyName("taskType")]
        [Description("The type of the task. MUST be exactly one of these strings: Vocabulary, Grammar, Reading, Writing, Translation, Listening, Speaking")]
        public string TaskType { get; set; }

        [JsonPropertyName("correctAnswer")]
        [Description("Optional. The exact expected answer text or correct choice key (e.g., 'A', 'true', or a word).")]
        public string? CorrectAnswer { get; set; }

        [JsonPropertyName("choices")]
        [Description("Optional. Multiple choice options provided as a dictionary (e.g., {\"A\": \"Option 1\", \"B\": \"Option 2\"}). Set to null if not a multiple-choice task.")]
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
