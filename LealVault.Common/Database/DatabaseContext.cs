using LealVault.Common.Properties;
using Microsoft.Data.Sqlite;

namespace LealVault.Common.Database
{
    internal sealed class DatabaseContext : IDisposable
    {
        private bool disposedValue = false;
        private readonly SqliteConnection _connection;

        internal DatabaseContext()
        {
            var filePath = Path.Combine(Configuration.BaseDirectory, "Data/lealvault.sqlite3");

            if (!File.Exists(filePath))
                File.Create(filePath);

            _connection = new($"Data Source={filePath}");
            _connection.Open();
            Initialize();
        }

        internal SqliteCommand GetCommand() => _connection.CreateCommand();

        private void Initialize()
        {
            if (!Configuration.DatabaseCreated)
                return;

            using var command = GetCommand();

            // Create tables
            command.CommandText = $@"
                CREATE TABLE IF NOT EXISTS User (
                    user_id TEXT PRIMARY KEY,
                    username TEXT UNIQUE NOT NULL,
                    password BLOB NOT NULL,
                    salt BLOB NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Register (
                    register_id TEXT PRIMARY KEY,
                    user_id TEXT NOT NULL,
                    name TEXT NOT NULL,
                    email TEXT,
                    password TEXT NOT NULL,
                    description TEXT,
                    FOREIGN KEY (user_id) REFERENCES users(user_id)
                );

                CREATE TABLE IF NOT EXISTS CreditCard (
                    card_id TEXT PRIMARY KEY,
                    user_id TEXT NOT NULL,
                    card_number TEXT NOT NULL,
                    expiration_date TEXT NOT NULL, -- MM/YYYY
                    cvv TEXT NOT NULL,
                    cardholder_name TEXT NOT NULL,
                    billing_address TEXT,
                    FOREIGN KEY (user_id) REFERENCES users(user_id)
                );
            ";

            command.ExecuteNonQuery();
            Configuration.DatabaseCreated = true;
        }

        #region [ Dispose ]
        private void Dispose(bool disposing)
        {
            if (!disposedValue && disposing)
            {
                _connection.Close();
                _connection.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}