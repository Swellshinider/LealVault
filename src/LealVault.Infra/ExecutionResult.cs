using LealVault.Infra.Vault;

namespace LealVault.Infra;

/// <summary>
/// Represents the execution result of a command.
/// </summary>
public sealed class ExecutionResult
{
    /// <summary>
    /// Success execution result.
    /// </summary>
    public static ExecutionResult SuccessResult(VaultManager vaultManager, string? message = null) => new(vaultManager, true, message);

    /// <summary>
    /// Execution Exit
    /// </summary>
    public static ExecutionResult Exit(VaultManager vault) => new(vault, true)
    {
        ShouldExit = true
    };

    /// <summary>
    /// Execution Result Fail.
    /// </summary>
    public static ExecutionResult Fail(VaultManager vault, string message) => new(vault, false, message);

    /// <summary>
    /// Execution Result Error.
    /// </summary>
    public static ExecutionResult Error(VaultManager vault, Exception ex) => new(vault, false, ex.Message, ex);

    /// <summary>
    /// Initialize a new instance of the <see cref="ExecutionResult"/> class.
    /// </summary>
    private ExecutionResult(VaultManager vault, bool success = true, string? message = null, Exception? exception = null)
    {
        Success = success;
        Message = message;
        Exception = exception;
        VaultIsOpen = vault.IsOpen;
        VaultPath = vault.VaultPath;
        VaultIsDirty = vault.IsDirty;
    }

    /// <summary>
    /// True if the command was executed successfully, false otherwise.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// True if the application should exit, false otherwise.
    /// </summary>
    public bool ShouldExit { get; set; }

    /// <summary>
    /// The message of the command.
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// The exception of the command.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// If during last execution vault was open.
    /// </summary>
    public bool VaultIsOpen { get; private set; }

    /// <summary>
    /// Vault path.
    /// </summary>
    public string VaultPath { get; private set; }

    /// <summary>
    /// If during last execution vault was dirty.
    /// </summary>
    public bool VaultIsDirty { get; private set; }
}