namespace AILanguageLearningApp.Services.LLM
{
    public interface ILlmService
    {
        Task<bool> CreateNewLessonAsync(string language, string exerciseTopic, string proficiencyLevel, int exerciseCount, int taskCount);
        Task CreateNewExercisesAsync(Guid lessonId, string language, string exerciseTopic, string proficiencyLevel, int exerciseCount, int taskCount);
        Task CheckUserResponseAsync(string userResponse, string instructions, string language);
        Task<List<string>> GetAvailableModelsAsync();

        public string HeavyModelId { get; set; }
        public string LightModelId { get; set; }
    }
}
