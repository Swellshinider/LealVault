
using LealVault.Infra.Vault;

namespace LealVault.Infra.Commands;

/// <summary>
/// Represents a command handler.
/// </summary>
public static class CommandHandler
{
    private static readonly List<Command> _commands;
    private static readonly VaultManager _vaultManager;
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

    static CommandHandler()
    {
        _vaultManager = new();
        _vaultManager.LogError += (msg) => msg.WriteLine(ConsoleColor.Red);

        _commands =
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
            new(CommandType.Create, CreateVault, [VaultShouldBeClosed, ArgumentCantBeNull])
            {
                Description = "Creates a new vault.",
                Usage = "create <path> | Creates a new vault at the specified path, file must have .lv extension."
            },
            new(CommandType.Open, OpenVault, [VaultShouldBeClosed, ArgumentCantBeNull])
            {
                Description = "Opens an existing vault.",
                Usage = "open <path> | Opens an existing vault at the specified path, file must have .lv extension."
            }
        ];
    }

    private static Command? GetCommand(CommandType commandType)
        => _commands.FirstOrDefault(c => c.Type == commandType);

    private static Command? GetCommand(string commandName)
        => _commands.FirstOrDefault(c => c.Type.ToFriendlyName() == commandName);

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

        var command = GetCommand(commandType);

        if (command is null)
            return ExecutionResult.FailValidation($"Unknown command: {inputCommand}");

        return command.Execute(arguments);
    }

    #region [ Validation Methods ]
    private static bool VaultShouldBeClosed(string? input, out string message)
    {
        if (_vaultManager.IsOpen)
        {
            message = "Vault is open.";
            return false;
        }

        message = "";
        return true;
    }

    private static bool VaultShouldBeOpen(string? input, out string message)
    {
        if (!_vaultManager.IsOpen)
        {
            message = "Vault is closed.";
            return false;
        }

        message = "";
        return true;
    }

    private static bool ArgumentCantBeNull(string? input, out string message)
    {
        if (input.IsNull())
        {
            message = "Argument cannot be null.";
            return false;
        }

        message = "";
        return true;
    }
    #endregion

    private static ExecutionResult DisplayHelp(string? arg)
    {
        if (arg.IsNull())
        {
            "Available commands:".WriteLine();

            foreach (var cmd in _commands)
                $"{cmd.Type.ToFriendlyName(),10}: {cmd.Description,-20}".WriteLine(ConsoleColor.Green);

            "\nYou can type".Write();
            " help <command> ".Write(ConsoleColor.Green);
            "to get more details about a command.\n".WriteLine();
            return ExecutionResult.SuccessNoMessage();
        }

        var command = GetCommand(arg!);

        if (command is null)
            return ExecutionResult.FailValidation($"Unknown command: {arg}");

        "Help ".Write();
        $"{arg}:".WriteLine(ConsoleColor.Green);

        $"    {command.Usage}".WriteLine();
        "".WriteLine();

        return ExecutionResult.SuccessNoMessage();
    }

    private static ExecutionResult Exit(string? arg) => ExecutionResult.Exit();

    private static ExecutionResult CreateVault(string? arg)
    {
        try
        {
            "Enter master password: ".Write();
            var masterPassword = Util.ReadPassword();

            "Confirm master password: ".Write();
            var confirmMasterPassword = Util.ReadPassword();

            if (masterPassword != confirmMasterPassword)
                return new ExecutionResult(false, "Passwords do not match.");

            "You'll not be able to change it later.".WriteLine(ConsoleColor.Yellow);
            "If you forgot your master password, you'll have to create a new vault.".WriteLine(ConsoleColor.Yellow);
            "Are you sure you want to continue? [y/n] ".Write();
            var input = (Console.ReadLine() ?? "").Trim().ToLower();

            if (input != "y")
                return new ExecutionResult(false, "Vault creation canceled.");

            _vaultManager.MasterPassword = masterPassword;

            var success = _vaultManager.Create(arg!);
            return new ExecutionResult(success, success
                                                ? "Vault creation finished."
                                                : "Vault could not be created.");
        }
        catch (Exception e)
        {
            return new ExecutionResult(false, e.Message);
        }
    }

    private static ExecutionResult OpenVault(string? arg)
    {
        try
        {
            "Enter master password: ".Write();
            var masterPassword = Util.ReadPassword();
            _vaultManager.MasterPassword = masterPassword;

            var success = _vaultManager.Open(arg!);
            return new ExecutionResult(success, success
                                                ? "Vault opened successfully."
                                                : "Vault could not be opened.");
        }
        catch (Exception e)
        {
            return new ExecutionResult(false, e.Message);
        }
    }
}