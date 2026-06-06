namespace AILanguageLearningApp.Services.LLM
{
    public interface ILlmService
    {
        Task CreateNewLessonAsync(string language, string exerciseTopic, string proficiencyLevel, int exerciseCount, int taskCount);
        Task CreateNewExercisesAsync(Guid lessonId, string language, string exerciseTopic, string proficiencyLevel, int exerciseCount, int taskCount);
    }
}
