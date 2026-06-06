using AILanguageLearningApp.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace AILanguageLearningApp.Services.LLM
{
    public class LessonFunctions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        [KernelFunction]
        [Description("Creates a new language lesson.")]
        public Lesson CreateLesson(
            [Description("The target language to learn (e.g., Japanese, French).")] string language,
            [Description("The topic of the lesson.")] string topic,
            [Description("The language level (e.g., A1, A2, B1).")] string level,
            [Description("The exercises payload raw JSON string. MUST match the requested schema layout exactly.")] string exercisesJson)
        {
            if (!Enum.TryParse(level, true, out LanguageLevel languageLevel))
            {
                throw new ArgumentException($"Level '{level}' is not a valid enumerated value.");
            }
            Lesson lesson = new()
            {
                Language = language,
                Topic = topic,
                Level = languageLevel,
                CreatedAt = DateTimeOffset.UtcNow,
                Id = Guid.CreateVersion7()
            };
            try
            {
                // Manually deserialize the flattened string
                List<LessonExercise>? exercises = JsonSerializer.Deserialize<List<LessonExercise>>(exercisesJson, _jsonOptions);
                if (exercises != null)
                {
                    ProcessExercises(lesson.Id, exercises);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JSON ERROR] Failed to parse exercisesJson string: {ex.Message}");
            }

            return lesson;
        }

        [KernelFunction]
        [Description("Adds structured exercises to an existing lesson using its unique ID.")]
        public List<LessonExercise> AddLessonExercises(
            [Description("The unique GUID string of the existing lesson.")] string lessonId,
            [Description("The exercises payload raw JSON string. MUST match the requested schema layout exactly.")] string exercisesJson)
        {
            if (!Guid.TryParse(lessonId, out Guid lessonGuid))
            {
                throw new ArgumentException("Invalid lessonId format. Must be a valid GUID.");
            }

            try
            {
                List<LessonExercise>? exercises = JsonSerializer.Deserialize<List<LessonExercise>>(exercisesJson, _jsonOptions);
                if (exercises != null)
                {
                    return ProcessExercises(lessonGuid, exercises);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JSON ERROR] Failed to parse exercisesJson string: {ex.Message}");
            }

            return [];
        }

        private List<LessonExercise> ProcessExercises(Guid lessonId, List<LessonExercise> exercises)
        {
            foreach (LessonExercise exercise in exercises)
            {
                exercise.Id = Guid.CreateVersion7();
                exercise.LessonId = lessonId;

                if (exercise.Tasks == null) continue;

                foreach (ExerciseTask task in exercise.Tasks)
                {
                    task.ExerciseId = exercise.Id;
                }
            }

            return exercises;
        }
    }
}
