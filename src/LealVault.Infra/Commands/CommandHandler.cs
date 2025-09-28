
using LealVault.Infra.Vault;

namespace LealVault.Infra.Commands;

/// <summary>
/// Represents a command handler.
/// </summary>
public static class CommandHandler
{
    private static readonly VaultManager _vaultManager = new();
    private static readonly Dictionary<string, CommandType> _commandMap = new()
    {
        { "help", CommandType.Help },
        { "exit", CommandType.Exit },
        { "create", CommandType.Create },
        { "open", CommandType.Open },
        { "save", CommandType.Save },
        { "close", CommandType.Close },
        { "list", CommandType.List }
    };
    private static readonly Lazy<List<Command>> _commands = new(InitializeCommandsList);

    private static List<Command> InitializeCommandsList()
    {
        return
        [
            new(CommandType.Help, DisplayHelp)
            {
                Description = "Displays this help message.",
                Usage = "help <command> | You can use with a command to get more details about it."
            },
            new(CommandType.Exit, Exit)
            {
                Description = "Exits the application.",
                Usage = "Exits the application."
            },
        ];
    }

    /// <summary>
    /// Executes a command.
    /// </summary>
    public static ExecutionResult Execute(string input)
    {
        var parts = input.Split(' ', StringSplitOptions.TrimEntries);
        var inputCommand = parts[0];
        var arguments = parts.Length <= 1
                            ? null
                            : string.Join(' ', parts.Skip(1));

        if (!_commandMap.TryGetValue(inputCommand, out var commandType))
            return ExecutionResult.FailValidation($"Unknown command: {inputCommand}");

        var command = _commands.Value.FirstOrDefault(c => c.Type == commandType);

        if (command is null)
            return ExecutionResult.FailValidation($"Unknown command: {inputCommand}");

        return command.Execute(arguments);
    }

    private static ExecutionResult Exit(string? arg) => ExecutionResult.Exit();

    private static ExecutionResult DisplayHelp(string? arg)
    {
        if (arg.IsNull())
        {
            "Available commands:".WriteLine();

            foreach (var cmd in _commands.Value)
                $"{cmd.Type.ToFriendlyName(),10}: {cmd.Description,-20}".WriteLine(ConsoleColor.Green);

            "\nYou can type".Write();
            " help <command> ".Write(ConsoleColor.Green);
            "to get more details about a command.\n".WriteLine();
            return ExecutionResult.SuccessNoMessage();
        }

        var command = _commands.Value.FirstOrDefault(c => c.Type.ToFriendlyName() == arg);

        if (command is null)
            return ExecutionResult.FailValidation($"Unknown command: {arg}");

        "Help ".Write();
        $"{arg}:".WriteLine(ConsoleColor.Green);
        
        $"    {command.Usage}".WriteLine();
        "".WriteLine();

        return ExecutionResult.SuccessNoMessage();
    }
}