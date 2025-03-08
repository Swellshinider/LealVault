using Microsoft.Data.Sqlite;

namespace LealVault.Common.Database.Repository.Base;

/// <summary>
/// Base repository class for handling database operations.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public abstract class Repository<T> : IRepository<T> where T : class
{
    private bool disposedValue = false;
    private readonly DatabaseContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{T}"/> class.
    /// </summary>
    protected Repository()
    {
        _context = new();
    }

    /// <summary>
    /// Creates a new SQLite command.
    /// </summary>
    /// <returns>A new instance of <see cref="SqliteCommand"/>.</returns>
    protected SqliteCommand NewCommand() => _context.GetCommand();

    /// <inheritdoc/>
    public abstract Task<IEnumerable<T>> GetAllAsync();

    /// <inheritdoc/>
    public abstract Task<T?> GetByIdAsync(string guid);

    /// <inheritdoc/>
    public abstract Task InsertAsync(T entity);

    /// <inheritdoc/>
    public abstract Task UpdateAsync(T entity);

    /// <inheritdoc/>
    public abstract Task DeleteAsync(string guid);

    #region [ Dispose ]
    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue && disposing)
        {
            _context.Dispose();
            disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
