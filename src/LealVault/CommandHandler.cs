using LealVault.Infra.Models;

namespace LealVault;

internal class CommandHandler
{
    private readonly List<Command> _commands;
    private readonly CancellationToken _cancellationToken;

    internal CommandHandler(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _commands = InitializeCommands();
    }

    private List<Command> InitializeCommands()
    {
        var cmds = new List<Command>() {
            new(DisplayHelp) // Help
            {
                Name = "help",
                Description = "Displays this help message",
                Usage = "Display a help message, can be used with an argument to get specific details about a command.\nUsage: help <command>"
            },
            new(ExitApplication) // Help
            {
                Name = "help",
                Description = "Displays this help message",
                Usage = "Display a help message, can be used with an argument to get specific details about a command.\nUsage: help <command>"
            },

        };

        return cmds;
    }

    private Command? GetCommand(string commandName)
    {
        var command = _commands.Find(p => p.Name == commandName);

        if (command == null)
        {
            "Command ".Write(ConsoleColor.Red);
            $"'{command}' ".Write(ConsoleColor.Yellow);
            "not found".WriteLine(ConsoleColor.Red);
            return null;
        }

        return command;
    }

    internal async Task<OperationType> Execute(string input)
    {
        // Parse input
        var parts = input.Split(' ', StringSplitOptions.TrimEntries);
        var commandName = parts[0];
        var commandArgument = string.Join(' ', [.. parts.Skip(1)]);
        var command = GetCommand(commandName);

        return command is null ? OperationType.None : await command.Execute(commandArgument);
    }

    private async Task<OperationType> DisplayHelp(string? arg)
    {
        await Task.Run(() =>
        {
            if (string.IsNullOrEmpty(arg) || string.IsNullOrWhiteSpace(arg))
            {
                "Commands: ".WriteLine();
                foreach (var cmd in _commands)
                    $"    {cmd}".WriteLine(ConsoleColor.Green);

                "\nYou can also type 'help <command>' to get help about a specific command".WriteLine(ConsoleColor.DarkYellow);
                return;
            }

            var command = GetCommand(arg);

            if (command is null)
                return;

            "Help ".Write();
            command.Name.WriteLine(ConsoleColor.Green);
            command.Description.WriteLine();
            command.Usage.WriteLine();
        });

        return OperationType.None;
    }

    private async Task<OperationType> ExitApplication(string? arg)
    {
        return await Task.FromResult(OperationType.Exit);
    }
}