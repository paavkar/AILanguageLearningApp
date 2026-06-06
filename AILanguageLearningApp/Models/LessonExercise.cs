using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AILanguageLearningApp.Models
{
    public class LessonExercise
    {
        public Guid Id { get; set; }
        public Guid LessonId { get; set; }
        public bool Finished { get; set; } = false;

        [JsonPropertyName("tasks")]
        [Description("The list of specific language tasks belonging strictly to this exercise.")]
        public List<ExerciseTask> Tasks { get; set; } = [];
    }
}
