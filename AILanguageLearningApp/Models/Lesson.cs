using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AILanguageLearningApp.Models
{
    public class Lesson
    {
        public Guid Id { get; set; }

        [JsonPropertyName("language")]
        [Description("The language being learned (e.g., Spanish, Japanese)")]
        public string Language { get; set; }

        [JsonPropertyName("topic")]
        [Description("The topic of the lesson")]
        public string Topic { get; set; }

        [Description("The CEFR language proficiency level. MUST be exactly one of these strings: A1, A2, B1, B2, C1, C2")]
        public LanguageLevel Level { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<LessonExercise> Contents { get; set; } = [];
    }

    public enum LanguageLevel
    {
        [Description("Beginner / Breakthrough")]
        A1,
        [Description("Elementary / Waystage")]
        A2,
        [Description("Intermediate / Threshold")]
        B1,
        [Description("Upper-Intermediate / Vantage")]
        B2,
        [Description("Advanced / Effective Operational Proficiency")]
        C1,
        [Description("Mastery / Proficiency")]
        C2
    }
}
