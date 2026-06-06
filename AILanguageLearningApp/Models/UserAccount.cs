namespace AILanguageLearningApp.Models
{
    public class UserAccount
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }
}
