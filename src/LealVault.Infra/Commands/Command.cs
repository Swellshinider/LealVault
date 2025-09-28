namespace LealVault.Infra.Commands;

/// <summary>
/// Represents a command.
/// </summary>
internal sealed class Command
{
    private readonly Func<string?, ExecutionResult> _action;

    public delegate bool Validator(string? input, out string message);
    private readonly List<Validator> _validators;

    /// <summary>
    /// Constructs a new instance of the <see cref="Command"/> class.
    /// </summary>
    public Command(CommandType commandType, Func<string?, ExecutionResult> action, List<Validator>? validators = null)
    {
        Type = commandType;
        _action = action;
        _validators = validators ?? [];
    }

    /// <summary>
    /// The type of the command.
    /// </summary>
    public CommandType Type { get; }

    /// <summary>
    /// The description of the command.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// The usage details of the command.
    /// </summary>
    public string Usage { get; set; } = "";

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="args">
    /// The arguments of the command.
    /// </param>
    /// <returns>
    /// True if the command was executed successfully, false otherwise.
    /// </returns>
    public ExecutionResult Execute(string? args)
    {
        foreach (var validator in _validators)
        {
            if (!validator(args, out var message))
                return ExecutionResult.FailValidation(message);
        }

        return _action(args);
    }
}