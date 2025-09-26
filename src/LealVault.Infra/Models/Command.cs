namespace LealVault.Infra.Models;

/// <summary>
/// Represents a command that can be executed in the CLI.
/// </summary>
public class Command
{
    private readonly Func<string?, Task<OperationType>> _execution;
    private readonly Func<string?, Task<bool>>? _validation;

    public Command(Func<string?, Task<OperationType>> execution, Func<string?, Task<bool>>? validation = null)
    {
        _execution = execution ?? throw new ArgumentNullException(nameof(execution));
        _validation = validation;
    }

    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Usage { get; init; }

    public async Task<OperationType> Execute(string? argument)
    {
        if (_validation is not null && !await _validation(argument))
            return OperationType.None;

        return await _execution(argument);
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Name,20} {Description,30}";
}