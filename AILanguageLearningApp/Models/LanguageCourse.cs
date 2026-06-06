using System.ComponentModel;

namespace AILanguageLearningApp.Models
{
    public class LanguageCourse
    {
        public Guid Id { get; set; }
        [Description("The name of the language course")]
        public string Name { get; set; }
        [Description("The description of the language course")]
        public string Description { get; set; }
        public List<Lesson> Lessons { get; set; } = [];
    }
}
