using AILanguageLearningApp.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace AILanguageLearningApp.Data
{
    public class LessonRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;
        private readonly ExerciseRepository _exerciseRepository;

        public LessonRepository(
            ILogger<LessonRepository> logger,
            ExerciseRepository exerciseRepository)
        {
            _logger = logger;
            _exerciseRepository = exerciseRepository;
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
                    CREATE TABLE IF NOT EXISTS Lessons (
                        Id TEXT PRIMARY KEY,
                        Language TEXT NOT NULL,
                        Topic TEXT NOT NULL,
                        Level INTEGER NOT NULL,
                        CreatedAt TEXT NOT NULL,
                        CourseId TEXT NOT NULL,
                        FOREIGN KEY (CourseId) REFERENCES Courses (Id) ON DELETE CASCADE
                    );";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the lessons table.");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<List<Lesson>> ListAsync(string courseId)
        {
            await using SqliteConnection connection = new(Constants.DatabasePath);
            await connection.OpenAsync();

            SqliteCommand selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Lessons WHERE CourseId = @CourseId";
            selectCmd.Parameters.AddWithValue("CourseId", courseId);
            List<Lesson> lessons = [];

            await using SqliteDataReader reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lessons.Add(new Lesson
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    Language = reader.GetString(1),
                    Topic = reader.GetString(2),
                    Level = Enum.Parse<LanguageLevel>(reader.GetString(3)),
                    CreatedAt = DateTimeOffset.Parse(reader.GetString(4)),
                    CourseId = Guid.Parse(reader.GetString(5))
                });
            }

            foreach (Lesson lesson in lessons)
            {
                lesson.Exercises = await _exerciseRepository.ListAsync(lesson.Id.ToString());
            }

            return lessons;
        }

        public async Task<Lesson?> GetAsync(string id)
        {
            await using SqliteConnection connection = new(Constants.DatabasePath);
            await connection.OpenAsync();

            SqliteCommand selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Lessons WHERE Id = @Id";
            selectCmd.Parameters.AddWithValue("Id", id);
            await using SqliteDataReader reader = await selectCmd.ExecuteReaderAsync();

            return await reader.ReadAsync()
                ? new Lesson
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    Language = reader.GetString(1),
                    Topic = reader.GetString(2),
                    Level = Enum.Parse<LanguageLevel>(reader.GetString(3)),
                    CreatedAt = DateTimeOffset.Parse(reader.GetString(4)),
                    CourseId = Guid.Parse(reader.GetString(5)),
                    Exercises = await _exerciseRepository.ListAsync(id)
                }
                : null;
        }

        public async Task<int> SaveItemAsync(Lesson lesson)
        {
            try
            {
                _logger.LogInformation($"Saving lesson: {lesson.Id}.");
                await using SqliteConnection connection = new(Constants.DatabasePath);
                await connection.OpenAsync();

                SqliteCommand insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO Lessons (Id, Language, Topic, Level, CreatedAt, CourseId)
                    VALUES (@Id, @Language, @Topic, @Level, @CreatedAt, @CourseId)
                    ON CONFLICT(Id) DO UPDATE SET Language = @Language, Topic = @Topic, Level = @Level, CreatedAt = @CreatedAt, CourseId = @CourseId;";
                insertCmd.Parameters.AddWithValue("Id", lesson.Id.ToString());
                insertCmd.Parameters.AddWithValue("Language", lesson.Language);
                insertCmd.Parameters.AddWithValue("Topic", lesson.Topic);
                insertCmd.Parameters.AddWithValue("Level", lesson.Level.ToString());
                insertCmd.Parameters.AddWithValue("CreatedAt", lesson.CreatedAt.ToString("o"));
                insertCmd.Parameters.AddWithValue("CourseId", lesson.CourseId.ToString());

                return await insertCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error saving lesson.");
                throw;
            }
        }
    }
}
