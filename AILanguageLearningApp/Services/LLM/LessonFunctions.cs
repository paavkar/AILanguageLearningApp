using AILanguageLearningApp.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AILanguageLearningApp.Services.LLM
{
    public class LessonFunctions(
        CourseRepository courseRepository,
        LessonRepository lessonRepository,
        ExerciseRepository exerciseRepository,
        TaskRepository taskRepository,
        ILogger<LessonFunctions> logger)
    {
        [KernelFunction("CreateLesson")]
        [Description("Creates a new language lesson.")]
        public async Task<Lesson> CreateLesson(
            [Description("The target language to learn (e.g., Japanese, French).")] string language,
            [Description("The topic of the lesson.")] string topic,
            [Description("The language level (A1, A2, B1, B2, C1, C2).")] string level,
            [Description("The list of exercises. Each exercise must contain a 'tasks' array where each task strictly includes: 'taskType', 'targetLanguageContent', 'nativeLanguageContent', 'instructions', 'choices' (if multiple choice), and 'correctAnswer'.")] List<LessonExercise> exercises)
        {
            if (!Enum.TryParse(level, true, out LanguageLevel languageLevel))
            {
                throw new ArgumentException($"Level '{level}' is not a valid enumerated value.");
            }
            LanguageCourse? course = await courseRepository.GetByLanguageAsync(language);
            if (course is null)
            {
                LanguageCourse newCourse = new()
                {
                    Id = Guid.CreateVersion7(),
                    UserId = Guid.CreateVersion7(), // Placeholder, should be set to the actual user ID
                    Name = $"{language}",
                    Lessons = []
                };
                course = newCourse;
                await courseRepository.SaveItemAsync(newCourse);
            }
            Lesson lesson = new()
            {
                Language = language,
                Topic = topic,
                Level = languageLevel,
                CreatedAt = DateTimeOffset.UtcNow,
                Id = Guid.CreateVersion7(),
                CourseId = course.Id,
            };
            await lessonRepository.SaveItemAsync(lesson);
            try
            {
                await ProcessExercises(lesson.Id, exercises);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JSON ERROR] Failed to parse exercisesJson string: {ex.Message}");
            }

            return lesson;
        }

        [KernelFunction("AddLessonExercises")]
        [Description("Adds structured exercises to an existing lesson using its unique ID.")]
        public async Task<List<LessonExercise>> AddLessonExercises(
            [Description("The unique GUID string of the existing lesson.")] string lessonId,
            [Description("The array of exercises to generate for this lesson.")] List<LessonExercise> exercises)
        {
            if (!Guid.TryParse(lessonId, out Guid lessonGuid))
            {
                throw new ArgumentException("Invalid lessonId format. Must be a valid GUID.");
            }

            try
            {
                return await ProcessExercises(lessonGuid, exercises);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JSON ERROR] Failed to parse exercisesJson string: {ex.Message}");
            }

            return [];
        }

        private async Task<List<LessonExercise>> ProcessExercises(Guid lessonId, List<LessonExercise> exercises)
        {
            await using SqliteConnection connection = new(Constants.DatabasePath);
            await connection.OpenAsync();

            await using SqliteTransaction transaction = connection.BeginTransaction();

            try
            {
                foreach (LessonExercise exercise in exercises)
                {
                    exercise.Id = Guid.CreateVersion7();
                    exercise.LessonId = lessonId;

                    await exerciseRepository.SaveItemWithConnectionAsync(exercise, connection, transaction);

                    foreach (ExerciseTask task in exercise.Tasks)
                    {
                        task.ExerciseId = exercise.Id;

                        await taskRepository.SaveItemWithConnectionAsync(task, connection, transaction);
                    }
                }

                await transaction.CommitAsync();
                logger.LogInformation("All exercises and tasks saved successfully in a single transaction.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Transaction failed! All changes rolled back.");
                throw;
            }

            return exercises;
        }
    }
}
