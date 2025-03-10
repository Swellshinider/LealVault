﻿using Microsoft.Data.Sqlite;

namespace LealVault.Common
{
    /// <summary>
    /// Provides utility methods
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Gets the value of the specified column as the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert the column value to.</typeparam>
        /// <param name="reader">The SQLite data reader.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <returns>The value of the column converted to the specified type.</returns>
        public static T GetTypedValue<T>(this SqliteDataReader reader, string columnName)
        {
            var value = reader[columnName];

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}