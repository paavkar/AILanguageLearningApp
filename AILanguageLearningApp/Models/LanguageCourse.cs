using System.ComponentModel;

namespace AILanguageLearningApp.Models
{
    public class LanguageCourse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [Description("The name of the language course")]
        public string Name { get; set; }
        public List<Lesson> Lessons { get; set; } = [];
    }
}
