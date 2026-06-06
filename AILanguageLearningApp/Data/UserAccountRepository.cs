using AILanguageLearningApp.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace AILanguageLearningApp.Data
{
    public class UserAccountRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public UserAccountRepository(ILogger<UserAccountRepository> logger)
        {
            _logger = logger;
        }

        private async Task Init()
        {
            if (_hasBeenInitialized)
                return;

            await using SqliteConnection connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                SqliteCommand createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS UserAccounts (
                        Id TEXT PRIMARY KEY,
                        Username TEXT NOT NULL,
                        PasswordHash TEXT NOT NULL
                    );";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the user account table.");
                throw;
            }

            _hasBeenInitialized = true;
        }

        private async Task<UserAccount?> GetByUsernameAsync(string username)
        {
            await Init();
            await using SqliteConnection connection = new(Constants.DatabasePath);
            await connection.OpenAsync();

            SqliteCommand selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM UserAccounts WHERE Username = @Username";
            selectCmd.Parameters.AddWithValue("Username", username);
            UserAccount account = null;

            await using SqliteDataReader reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                account = new UserAccount
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2)
                };
            }
            return account;
        }
    }
}
