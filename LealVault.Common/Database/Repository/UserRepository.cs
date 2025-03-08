using LealVault.Common.Database.Models;
using LealVault.Common.Database.Repository.Base;
using System.Data;

namespace LealVault.Common.Database.Repository;

/// <summary>
/// User repository class
/// </summary>
public sealed class UserRepository : Repository<User>
{
    /// <inheritdoc/>
    public override Task<IEnumerable<User>> GetAllAsync() 
        => Task.FromResult(Enumerable.Empty<User>());

    /// <inheritdoc/>
    public override async Task<User?> GetByIdAsync(string guid)
    {
        using var command = NewCommand();
        command.CommandText = @$"SELECT
                                    user_id,
                                    username,
                                    password,
                                    salt
                                FROM
                                    User
                                WHERE
                                    user_id = @ID";

        command.Parameters.AddWithValue("ID", guid);

        var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow);

        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetTypedValue<string>("user_id"),
                Username = reader.GetTypedValue<string>("username"),
                Password = reader.GetTypedValue<byte[]>("password"),
                Salt = reader.GetTypedValue<byte[]>("salt")
            };
        }

        return null;
    }

    /// <inheritdoc/>
    public override async Task InsertAsync(User entity)
    {
        using var command = NewCommand();
        command.CommandText = @$"INSERT INTO User (
                                    user_id,
                                    username,
                                    password,
                                    salt
                                ) VALUES (
                                    @ID,
                                    @Username,
                                    @Password,
                                    @Salt
                                )";

        command.Parameters.AddWithValue("ID", entity.Id);
        command.Parameters.AddWithValue("Username", entity.Username);
        command.Parameters.AddWithValue("Password", entity.Password);
        command.Parameters.AddWithValue("Salt", entity.Salt);

        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public override async Task UpdateAsync(User entity)
    {
        using var command = NewCommand();
        command.CommandText = @$"UPDATE 
                                    User 
                                SET
                                    username = @Username,
                                    password = @Password,
                                    salt = @Salt
                                WHERE
                                    user_id = @ID";

        command.Parameters.AddWithValue("ID", entity.Id);
        command.Parameters.AddWithValue("Username", entity.Username);
        command.Parameters.AddWithValue("Password", entity.Password);
        command.Parameters.AddWithValue("Salt", entity.Salt);

        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public override async Task DeleteAsync(string guid)
    {
        using var command = NewCommand();
        command.CommandText = @$"DELETE FROM 
                                    User
                                WHERE 
                                    user_id = @ID";

        command.Parameters.AddWithValue("ID", guid);

        await command.ExecuteNonQueryAsync();
    }
}