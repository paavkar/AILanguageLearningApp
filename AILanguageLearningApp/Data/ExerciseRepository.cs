using AILanguageLearningApp.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace AILanguageLearningApp.Data
{
    public class ExerciseRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;
        private readonly TaskRepository _taskRepository;

        public ExerciseRepository(
            ILogger<ExerciseRepository> logger,
            TaskRepository taskRepository)
        {
            _logger = logger;
            _taskRepository = taskRepository;
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
                    CREATE TABLE IF NOT EXISTS Exercises (
                        Id TEXT PRIMARY KEY,
                        Finished BOOLEAN NOT NULL,
                        LessonId TEXT NOT NULL,
                        FOREIGN KEY (LessonId) REFERENCES Lessons (Id) ON DELETE CASCADE
                    );";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the exercises table.");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<int> SaveItemAsync(LessonExercise exercise)
        {
            await using SqliteConnection connection = new(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                SqliteCommand insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO Exercises (Id, Finished, LessonId) VALUES (@Id, @Finished, @LessonId);";
                insertCmd.Parameters.AddWithValue("Id", exercise.Id.ToString());
                insertCmd.Parameters.AddWithValue("Finished", exercise.Finished);
                insertCmd.Parameters.AddWithValue("LessonId", exercise.LessonId.ToString());

                return await insertCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the exercise.");
                throw;
            }
        }

        public async Task SaveItemWithConnectionAsync(
            LessonExercise exercise,
            SqliteConnection connection,
            SqliteTransaction transaction)
        {
            try
            {
                _logger.LogInformation($"Saving exercise: {exercise.Id}.");
                using SqliteCommand insertCmd = connection.CreateCommand();
                insertCmd.Transaction = transaction;
                insertCmd.CommandText = @"
                    INSERT INTO Exercises (Id, Finished, LessonId)
                    VALUES (@Id, @Finished, @LessonId);";

                insertCmd.Parameters.AddWithValue("Id", exercise.Id.ToString());
                insertCmd.Parameters.AddWithValue("Finished", exercise.Finished);
                insertCmd.Parameters.AddWithValue("LessonId", exercise.LessonId.ToString());

                await insertCmd.ExecuteNonQueryAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error saving the exercise.");
                throw;
            }
        }

        public async Task<List<LessonExercise>> ListAsync(string lessonId)
        {
            await using SqliteConnection connection = new(Constants.DatabasePath);
            await connection.OpenAsync();

            SqliteCommand selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Exercises WHERE LessonId = @LessonId";
            selectCmd.Parameters.AddWithValue("LessonId", lessonId);
            List<LessonExercise> exercises = [];

            await using SqliteDataReader reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                exercises.Add(new LessonExercise
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    Finished = bool.Parse(reader.GetString(1)),
                    LessonId = Guid.Parse(reader.GetString(2))
                });
            }

            foreach (LessonExercise exercise in exercises)
            {
                exercise.Tasks = await _taskRepository.ListAsync(exercise.Id.ToString());
            }

            return exercises;
        }
    }
}
