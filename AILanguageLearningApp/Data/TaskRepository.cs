using AILanguageLearningApp.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace AILanguageLearningApp.Data
{
    public class TaskRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public TaskRepository(
            ILogger<TaskRepository> logger)
        {
            _logger = logger;
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
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the tasks table.");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<int> SaveItemAsync(ExerciseTask task)
        {
            await using SqliteConnection connection = new(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                SqliteCommand insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO Tasks (Id, TargetLanguageContent, NativeLanguageContent, Instructions, TaskType, CorrectAnswer, ExerciseId)
                    VALUES (@Id, @TargetLanguageContent, @NativeLanguageContent, @Instructions, @TaskType, @CorrectAnswer, @ExerciseId);";
                insertCmd.Parameters.AddWithValue("Id", task.Id.ToString());
                insertCmd.Parameters.AddWithValue("TargetLanguageContent", task.TargetLanguageContent);
                insertCmd.Parameters.AddWithValue("NativeLanguageContent", task.NativeLanguageContent);
                insertCmd.Parameters.AddWithValue("Instructions", task.Instructions);
                insertCmd.Parameters.AddWithValue("TaskType", task.TaskType);
                insertCmd.Parameters.AddWithValue("CorrectAnswer", task.CorrectAnswer);
                insertCmd.Parameters.AddWithValue("ExerciseId", task.ExerciseId.ToString());

                return await insertCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the task.");
                throw;
            }
        }

        public async Task SaveItemWithConnectionAsync(
            ExerciseTask task,
            SqliteConnection connection,
            SqliteTransaction transaction)
        {
            try
            {
                _logger.LogInformation($"Saving task: {task.Id}.");
                using SqliteCommand insertCmd = connection.CreateCommand();
                insertCmd.Transaction = transaction;
                insertCmd.CommandText = @"
                    INSERT INTO Tasks (Id, TargetLanguageContent, NativeLanguageContent, Instructions, TaskType, CorrectAnswer, ExerciseId)
                    VALUES (@Id, @TargetLanguageContent, @NativeLanguageContent, @Instructions, @TaskType, @CorrectAnswer, @ExerciseId);";

                insertCmd.Parameters.AddWithValue("Id", task.Id.ToString());
                insertCmd.Parameters.AddWithValue("TargetLanguageContent", task.TargetLanguageContent ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("NativeLanguageContent", task.NativeLanguageContent ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("Instructions", task.Instructions ?? "");
                insertCmd.Parameters.AddWithValue("TaskType", task.TaskType ?? "");
                insertCmd.Parameters.AddWithValue("CorrectAnswer", task.CorrectAnswer ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("ExerciseId", task.ExerciseId.ToString());

                await insertCmd.ExecuteNonQueryAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error saving the task.");
                throw;
            }
        }

        public async Task<List<ExerciseTask>> ListAsync(string exerciseId)
        {
            await using SqliteConnection connection = new(Constants.DatabasePath);
            await connection.OpenAsync();

            SqliteCommand selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Tasks WHERE ExerciseId = @ExerciseId";
            selectCmd.Parameters.AddWithValue("ExerciseId", exerciseId);
            List<ExerciseTask> tasks = [];

            await using SqliteDataReader reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tasks.Add(new ExerciseTask
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    TargetLanguageContent = reader.GetString(1),
                    NativeLanguageContent = reader.GetString(2),
                    Instructions = reader.GetString(3),
                    TaskType = reader.GetString(4),
                    CorrectAnswer = reader.GetString(5),
                    ExerciseId = Guid.Parse(reader.GetString(6))
                });
            }

            return tasks;
        }
    }
}
