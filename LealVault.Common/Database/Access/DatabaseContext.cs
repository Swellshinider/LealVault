using LealVault.Common.Properties;
using Microsoft.Data.Sqlite;

namespace LealVault.Common.Database.Access
{
    internal sealed class DatabaseContext : IDisposable
    {
        internal const string USER_TABLE = "User";
        internal const string REGISTER_TABLE = "Register";
        internal const string CREDITCARD_TABLE = "CreditCard";

        private bool disposedValue = false;
        private readonly SqliteConnection _connection;

        internal DatabaseContext()
        {
            var filePath = Path.Combine(Configuration.BaseDirectory, "Data/lealvault.sqlite3");

            if (!File.Exists(filePath))
                File.Create(filePath);

            _connection = new($"Data Source={filePath}");
            _connection.Open();
        }

        internal SqliteCommand GetCommand() => _connection.CreateCommand();

        internal void Initialize()
        {
            using var command = GetCommand();

            // Create tables
            command.CommandText = $@"
                CREATE TABLE IF NOT EXISTS {USER_TABLE} (
                    user_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT UNIQUE NOT NULL,
                    password BLOB NOT NULL,
                    salt BLOB NOT NULL
                );

                CREATE TABLE IF NOT EXISTS {REGISTER_TABLE} (
                    register_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    user_id INTEGER NOT NULL,
                    name TEXT NOT NULL,
                    email TEXT,
                    password TEXT NOT NULL,
                    description TEXT,
                    FOREIGN KEY (user_id) REFERENCES users(user_id)
                );

                CREATE TABLE IF NOT EXISTS {CREDITCARD_TABLE} (
                    card_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    user_id INTEGER NOT NULL,
                    card_number TEXT NOT NULL,
                    expiration_date TEXT NOT NULL, -- MM/YYYY
                    cvv TEXT NOT NULL,
                    cardholder_name TEXT NOT NULL,
                    billing_address TEXT,
                    FOREIGN KEY (user_id) REFERENCES users(user_id)
                );
            ";

            command.ExecuteNonQuery();
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