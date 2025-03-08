namespace LealVault.Common.Database.Models;

/// <summary>
/// Represents a user in the database.
/// </summary>
public sealed class User
{
    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the username of the user.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Gets the hashed password of the user.
    /// </summary>
    public required byte[] Password { get; init; }

    /// <summary>
    /// Gets the salt used for hashing the password.
    /// </summary>
    public required byte[] Salt { get; init; }
}