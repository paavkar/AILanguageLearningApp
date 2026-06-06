namespace AILanguageLearningApp.Data
{
    public static class Constants
    {
        public const string DatabaseFilename = "AILanguageSQLite.db3";

        public static string DatabasePath =>
            $"Data Source={Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename)}";
    }
}
