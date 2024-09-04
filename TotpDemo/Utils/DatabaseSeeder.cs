using Dapper;
using System.Data.SqlClient;

namespace TotpDemo.Utils
{
    public class DatabaseSeeder
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseSeeder> _logger;
        private readonly string _dbName = "TotpDemo";

        public DatabaseSeeder(IConfiguration config, ILogger<DatabaseSeeder> logger)
        {
            _config = config;
            _logger = logger;
            _dbName = _config.GetRequiredSection("DB:Name")?.Value ?? "TotpDemo";
        }

        public void EnsureDatabaseExists()
        {
            try
            {
                CreateDatabase(_dbName);
            }
            catch
            {
                _logger.LogDebug("Database already exists.");
            }

            try
            {
                CreateSchema(_dbName);
            }
            catch
            {
                _logger.LogDebug("Table already exists.");
            }
        }

        private void CreateDatabase(string databaseName)
        {
            var sql = $"CREATE DATABASE {databaseName};";

            using (var connection = new SqlConnection(_config.GetConnectionString("Default")))
            {
                connection.Open();
                connection.Execute(sql);
            }
        }

        private void CreateSchema(string databaseName)
        {
            var sql = $@"USE [{databaseName}];
                    CREATE TABLE Users (
                    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
                    Username NVARCHAR(255) NOT NULL UNIQUE,
                    PasswordHash NVARCHAR(255) NOT NULL,
                    TOTPSecret NVARCHAR(255) NULL,
                    Email NVARCHAR(255) NULL,
                    IsTOTPEnabled BIT NOT NULL DEFAULT 0,
                    DateCreated DATETIME NOT NULL DEFAULT GETDATE(),
                    LastLogin DATETIME NULL
                )";

            using (var connection = new SqlConnection(_config.GetConnectionString("Default")))
            {
                connection.Execute(sql);
            }
        }
    }
}
