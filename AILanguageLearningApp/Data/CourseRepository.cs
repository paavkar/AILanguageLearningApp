using AILanguageLearningApp.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace AILanguageLearningApp.Data
{
    public class CourseRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;
        private readonly LessonRepository _lessonRepository;

        public CourseRepository(
            ILogger<CourseRepository> logger,
            LessonRepository lessonRepository)
        {
            _logger = logger;
            _lessonRepository = lessonRepository;
        }

        private async Task Init()
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

                SqliteCommand createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Courses (
                        Id TEXT PRIMARY KEY,
                        Name TEXT NOT NULL,
                        UserId TEXT NOT NULL
                    );";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the courses table.");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<int> SaveItemAsync(LanguageCourse course)
        {
            try
            {
                _logger.LogInformation($"Saving course: {course.Id}");
                await using SqliteConnection connection = new(Constants.DatabasePath);
                await connection.OpenAsync();

                SqliteCommand insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO Courses (Id, Name, UserId) VALUES (@Id, @Name, @UserId)
                    ON CONFLICT(Id) DO UPDATE SET Name = @Name, UserId = @UserId;";
                insertCmd.Parameters.AddWithValue("Id", course.Id.ToString());
                insertCmd.Parameters.AddWithValue("Name", course.Name);
                insertCmd.Parameters.AddWithValue("UserId", course.UserId.ToString());

                return await insertCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error saving course.");
                throw;
            }
        }

        public async Task<List<LanguageCourse>> ListAsync(string userId)
        {
            await using SqliteConnection connection = new(Constants.DatabasePath);
            await connection.OpenAsync();

            SqliteCommand selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Courses WHERE UserId = @UserId";
            selectCmd.Parameters.AddWithValue("UserId", userId);
            List<LanguageCourse> courses = [];

            await using SqliteDataReader reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                courses.Add(new LanguageCourse
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    Name = reader.GetString(1),
                    UserId = Guid.Parse(reader.GetString(2))
                });
            }

            foreach (LanguageCourse course in courses)
            {
                course.Lessons = await _lessonRepository.ListAsync(course.Id.ToString());
            }

            return courses;
        }

        public async Task<LanguageCourse?> GetByLanguageAsync(string language)
        {
            await using SqliteConnection connection = new(Constants.DatabasePath);
            await connection.OpenAsync();

            SqliteCommand selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Courses WHERE Name = @Language";
            selectCmd.Parameters.AddWithValue("Language", language);
            await using SqliteDataReader reader = await selectCmd.ExecuteReaderAsync();

            return await reader.ReadAsync()
                ? new LanguageCourse
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    Name = reader.GetString(1),
                    UserId = Guid.Parse(reader.GetString(2))
                }
                : null;
        }

        public async Task<LanguageCourse?> GetByIdAsync(string id)
        {
            await using SqliteConnection connection = new(Constants.DatabasePath);
            await connection.OpenAsync();

            SqliteCommand selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Courses WHERE Id = @Id";
            selectCmd.Parameters.AddWithValue("Id", id);
            await using SqliteDataReader reader = await selectCmd.ExecuteReaderAsync();

            return await reader.ReadAsync()
                ? new LanguageCourse
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    Name = reader.GetString(1),
                    UserId = Guid.Parse(reader.GetString(2))
                }
                : null;
        }
    }
}
