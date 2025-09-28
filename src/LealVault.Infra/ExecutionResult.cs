namespace LealVault.Infra;

/// <summary>
/// Represents the execution result of a command.
/// </summary>
public sealed class ExecutionResult
{
    /// <summary>
    /// Execution Result Fail.
    /// </summary>
    public static ExecutionResult FailValidation(string message) => new(false, message);

    /// <summary>
    /// Execution Result Success.
    /// </summary>
    public static ExecutionResult SuccessNoMessage() => new(true);

    /// <summary>
    /// Execution Exit
    /// </summary>
    public static ExecutionResult Exit() => new(true)
    {
        ShouldExit = true
    };

    /// <summary>
    /// Initialize a new instance of the <see cref="ExecutionResult"/> class.
    /// </summary>
    public ExecutionResult(bool success = true, string? message = null)
    {
        Success = success;
        Message = message;
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
}