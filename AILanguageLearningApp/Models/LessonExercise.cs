using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AILanguageLearningApp.Models
{
    public class LessonExercise
    {
        public Guid Id { get; set; }
        public Guid LessonId { get; set; }
        public bool Finished { get; set; } = false;

        [Description("The collection of distinct learning tasks for this exercise.")]
        [JsonPropertyName("tasks")]
        public List<ExerciseTask> Tasks { get; set; } = [];
    }
}
