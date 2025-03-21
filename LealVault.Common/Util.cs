using Microsoft.Data.Sqlite;
using System.ComponentModel;
using System.Reflection;

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

        /// <summary>
        /// Converts the enum value to a friendly string representation.
        /// </summary>
        /// <param name="value">The enum value to convert.</param>
        /// <returns>A friendly string representation of the enum value.</returns>
        public static string ToFriendlyString(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        /// <summary>
        /// Generates a new GUID
        /// </summary>
        public static string GenerateGuid() 
            => Guid.NewGuid().ToString();
    }
}