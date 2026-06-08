using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace AILanguageLearningApp.Data
{
    public class DatabaseInitializer
    {
        private bool _hasBeenInitialized = false;

        private readonly ILogger _logger;

        public DatabaseInitializer(
            ILogger<DatabaseInitializer> logger)
        {
            _logger = logger;
        }

        public async Task InitDatabase()
        {
            if (_hasBeenInitialized)
                return;

            await using SqliteConnection connection = new(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                SqliteCommand pragmaCmd = connection.CreateCommand();
                pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
                pragmaCmd.ExecuteNonQuery();

                SqliteCommand createUserTableCmd = connection.CreateCommand();
                createUserTableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS UserAccounts (
                        Id TEXT PRIMARY KEY,
                        Username TEXT NOT NULL,
                        PasswordHash TEXT NOT NULL
                    );";
                await createUserTableCmd.ExecuteNonQueryAsync();

                SqliteCommand createCourseTableCmd = connection.CreateCommand();
                createCourseTableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Courses (
                        Id TEXT PRIMARY KEY,
                        Name TEXT NOT NULL,
                        UserId TEXT NOT NULL
                    );";
                await createCourseTableCmd.ExecuteNonQueryAsync();

                SqliteCommand createLessonTableCmd = connection.CreateCommand();
                createLessonTableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Lessons (
                        Id TEXT PRIMARY KEY,
                        Language TEXT NOT NULL,
                        Topic TEXT NOT NULL,
                        Level INTEGER NOT NULL,
                        CreatedAt TEXT NOT NULL,
                        CourseId TEXT NOT NULL,
                        FOREIGN KEY (CourseId) REFERENCES Courses (Id) ON DELETE CASCADE
                    );";
                await createLessonTableCmd.ExecuteNonQueryAsync();

                SqliteCommand createExerciseTableCmd = connection.CreateCommand();
                createExerciseTableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Exercises (
                        Id TEXT PRIMARY KEY,
                        Finished BOOLEAN NOT NULL,
                        LessonId TEXT NOT NULL,
                        FOREIGN KEY (LessonId) REFERENCES Lessons (Id) ON DELETE CASCADE
                    );";
                await createExerciseTableCmd.ExecuteNonQueryAsync();

                SqliteCommand createTaskTableCmd = connection.CreateCommand();
                createTaskTableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id TEXT PRIMARY KEY,
                        TargetLanguageContent TEXT,
                        NativeLanguageContent TEXT,
                        Instructions TEXT NOT NULL,
                        TaskType TEXT NOT NULL,
                        CorrectAnswer TEXT,
                        ExerciseId TEXT NOT NULL,
                        FOREIGN KEY (ExerciseId) REFERENCES Exercises (Id) ON DELETE CASCADE
                    );";
                await createTaskTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the courses table.");
                throw;
            }

            _hasBeenInitialized = true;
        }
    }
}
