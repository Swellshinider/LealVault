using LealVault.Infra.Security;
using LealVault.Infra.Vault;
using TextCopy;

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
        { "clear", CommandType.Clear },
        { "create", CommandType.Create },
        { "open", CommandType.Open },
        { "save", CommandType.Save },
        { "close", CommandType.Close },
        { "search", CommandType.Search },
        { "add", CommandType.Add },
        { "remove", CommandType.Remove },
        { "update", CommandType.Update },
        { "copy", CommandType.Copy },
        { "display", CommandType.Display }
    };

    static CommandHandler()
    {
        _vaultManager = new();
        _vaultManager.LogError += (msg) => msg.WriteLine(ConsoleColor.Red);

        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            Exit(null);
        };

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
            new (CommandType.Clear, Clear)
            {
                Description = "Clears the console.",
                Usage = "clear | Clears the console."
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
            },
            new(CommandType.Save, SaveVault, [VaultShouldBeOpen])
            {
                Description = "Saves the vault.",
                Usage = "save | Saves the vault."
            },
            new(CommandType.Close, CloseVault, [VaultShouldBeOpen])
            {
                Description = "Closes the vault.",
                Usage = "close | Closes the vault."
            },
            new(CommandType.Search, SearchEntries, [VaultShouldBeOpen])
            {
                Description = "Search for a entry in the vault.",
                Usage = "search <pattern> | Searches for a entry in the vault using the specified pattern. (optional)"
            },
            new(CommandType.Add, AddEntry, [VaultShouldBeOpen])
            {
                Description = "Add a new entry to the vault.",
                Usage = "add | Adds a new entry to the vault."
            },
            new(CommandType.Remove, RemoveEntry, [VaultShouldBeOpen, ArgumentCantBeNull])
            {
                Description = "Remove an entry from the vault.",
                Usage = "remove <id> | Removes the specified entry from the vault."
            },
            new(CommandType.Update, UpdateEntry, [VaultShouldBeOpen, ArgumentCantBeNull])
            {
                Description = "Update an entry in the vault.",
                Usage = "update <id> | Updates the specified entry in the vault."
            },
            new(CommandType.Copy, CopyEntry, [VaultShouldBeOpen, ArgumentCantBeNull])
            {
                Description = "Copy an entry data to the clipboard.",
                Usage = "copy <id> <type> | Copies the specified entry to the clipboard.\n" +
                "You have this options for <type>:\n" +
                "p - copy the password\n" +
                "e - copy the email\n"
            },
            new(CommandType.Display, DisplayEntry, [VaultShouldBeOpen, ArgumentCantBeNull])
            {
                Description = "Display all data of an entry.",
                Usage = "display <id> | Displays all data of the specified entry."
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
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
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

    private static ExecutionResult Exit(string? arg)
    {
        var closeResult = _vaultManager.IsOpen ? CloseVault(arg) : ExecutionResult.SuccessNoMessage();

        if (!closeResult.Success)
        {
            $"Unable to close vault. {closeResult.Message}".WriteLine(ConsoleColor.Red);
            if (!Util.ConfirmUserAction("Do you want to exit anyway?"))
                return Exit(arg);
        }

        return ExecutionResult.Exit();
    }

    private static ExecutionResult Clear(string? arg)
    {
        Console.Clear();
        return ExecutionResult.SuccessNoMessage();
    }

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

            if (!Util.ConfirmUserAction())
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

    private static ExecutionResult SaveVault(string? arg)
    {
        try
        {
            "Saving vault...".WriteLine();

            if (!Util.ConfirmUserAction())
                return new ExecutionResult(false, "Vault saving canceled.");

            var success = _vaultManager.Save();
            return new ExecutionResult(success, success
                                                ? "Vault saved successfully."
                                                : "Vault could not be saved.");
        }
        catch (Exception e)
        {
            return new ExecutionResult(false, e.Message);
        }
    }

    private static ExecutionResult CloseVault(string? arg)
    {
        try
        {
            "Closing vault...".WriteLine();

            if (_vaultManager.IsDirty) // user has unsaved changes
            {
                "You have unsaved changes.".WriteLine(ConsoleColor.Yellow);
                if (Util.ConfirmUserAction("Do you want to save them?"))
                {
                    var success = _vaultManager.Save();
                    if (!success)
                        return new ExecutionResult(false, "Vault could not be saved.");
                }
                else
                    "Vault will not be saved.".WriteLine(ConsoleColor.Yellow);
            }

            _vaultManager.Close();
            return new ExecutionResult(true, "Vault closed successfully.");
        }
        catch (Exception e)
        {
            return new ExecutionResult(false, e.Message);
        }
    }

    private static ExecutionResult SearchEntries(string? arg)
    {
        try
        {
            var entries = _vaultManager.VaultData!.Entries;

            if (entries.Count == 0)
                return new ExecutionResult(true, "You have no entries in your vault.");

            if (arg.IsNull()) // Searches all entries
            {
                "Entries: ".WriteLine();
                int colorRand = 1;
                foreach (var entry in entries)
                {
                    colorRand++;
                    if (colorRand > 14)
                        colorRand = 1;

                    $"    {entry}".WriteLine((ConsoleColor)colorRand);
                }
                "".WriteLine();
                
                return new ExecutionResult(true, "Search finished.");
            }

            $"Entries to match {arg!}: ".WriteLine();
            var search = entries.Where(e => e.Name.Contains(arg!, StringComparison.InvariantCultureIgnoreCase) ||
                                             e.Tag.Contains(arg!, StringComparison.InvariantCultureIgnoreCase));

            if (search is null || search?.Count() == 0)
                return new ExecutionResult(true, "No entries found.");

            foreach (var entry in search!)
                $"{entry}".WriteLine();

            return new ExecutionResult(true, "Search finished.");
        }
        catch (Exception e)
        {
            return new ExecutionResult(false, e.Message);
        }
    }

    private static ExecutionResult AddEntry(string? arg)
    {
        try
        {
            "Adding entry...".WriteLine();
            "Please enter the following informations:".WriteLine();

            var name = AskUntilUnique("Entry name", (e, r) => r is not null && e.Name.Equals(r, StringComparison.OrdinalIgnoreCase))!;
            var email = Util.ReadNotEmptyText("Email", true);
            var password = Util.ReadNotEmptyText("Password")!;
            var tag = Util.ReadNotEmptyText("Tag")!;

            "Note (optional): ".Write();
            var notes = Console.ReadLine();

            var entry = new Entry()
            {
                Id = Id.GenerateUniqueId([.. _vaultManager.VaultData!.Entries.Select(e => e.Id)]),
                Name = name,
                Email = email,
                Password = password,
                Tag = tag,
                Notes = notes,
                Modified = DateTime.Now.Ticks,
            };

            var success = _vaultManager.Add(entry);
            return new ExecutionResult(success, success
                                                ? "Entry added successfully."
                                                : "Entry could not be added.");
        }
        catch (Exception e)
        {
            return new ExecutionResult(false, e.Message);
        }
    }

    private static ExecutionResult RemoveEntry(string? arg)
    {
        try
        {
            var entry = _vaultManager.VaultData!.Entries.FirstOrDefault(e => e.Id == arg!);

            if (entry is null)
                return new ExecutionResult(false, "Entry not found.");

            "Entry found!".WriteLine();
            entry.ToString().WriteLine(ConsoleColor.Green);

            if (!Util.ConfirmUserAction("Are you sure you want to remove this entry?"))
                return new ExecutionResult(false, "Entry removal canceled.");

            var success = _vaultManager.Remove(entry);
            return new ExecutionResult(success, success
                                                ? "Entry removed successfully."
                                                : "Entry could not be removed.");
        }
        catch (Exception e)
        {
            return new ExecutionResult(false, e.Message);
        }
    }

    private static ExecutionResult UpdateEntry(string? arg)
    {
        try
        {
            var entry = _vaultManager.VaultData!.Entries.FirstOrDefault(e => e.Id == arg!);

            if (entry is null)
                return new ExecutionResult(false, "Entry not found.");

            "Entry found!".WriteLine();
            entry.ToString().WriteLine(ConsoleColor.Green);

            "Just press [Enter] to skip edition:".WriteLine();

            var name = AskUntilUnique($"Entry name: {entry.Name}, new:", (e, r) => e.Name.Equals(r, StringComparison.OrdinalIgnoreCase), true);
            var email = Util.ReadNotEmptyText($"Email: {entry.Email}, new:", true);
            var password = Util.ReadNotEmptyText($"Password: {entry.Password}, new:", true);
            var tag = Util.ReadNotEmptyText($"Tag: {entry.Tag}, new:", true);

            $"Note: {entry.Notes}\n, new:: ".Write();
            var notes = Console.ReadLine();

            var updatedEntry = new Entry()
            {
                Id = entry.Id,
                Name = name ?? entry.Name,
                Email = email ?? entry.Email,
                Password = password ?? entry.Password,
                Tag = tag ?? entry.Tag,
                Notes = notes ?? entry.Notes,
                Modified = DateTime.Now.Ticks,
            };

            "\nVerifying changes...".WriteLine();
            entry.PrintDiff(updatedEntry);
            
            if (!Util.ConfirmUserAction("Are you sure you want to update this entry?"))
                return new ExecutionResult(false, "Entry update canceled.");

            var removeOldResult = _vaultManager.Remove(entry);

            if (!removeOldResult)
                return new ExecutionResult(false, "Old entry could not be removed.");

            var addResult = _vaultManager.Add(updatedEntry);

            if (!addResult)
                return new ExecutionResult(false, "New entry could not be added.");

            return new ExecutionResult(true, "Entry updated successfully.");
        }
        catch (Exception e)
        {
            return new ExecutionResult(false, e.Message);
        }
    }

    private static ExecutionResult CopyEntry(string? arg)
    {
        try
        {   
            var split = arg!.Split(' ', StringSplitOptions.None);

            if (split.Length != 2)
                return new ExecutionResult(false, "Invalid arguments. You should provide <id> and <type> only.");

            var id = split[0].Trim().ToLower();
            var type = split[1].Trim().ToLower();

            var entry = _vaultManager.VaultData!.Entries.FirstOrDefault(e => e.Id == id);

            if (entry is null)
                return new ExecutionResult(false, "Entry not found.");

            if (type == "p")
                ClipboardService.SetText(entry.Password);
            else if (type == "e")
                ClipboardService.SetText(entry.Email ?? "");
            else
                return new ExecutionResult(false, "Invalid type. You should provide p or e.");

            return new ExecutionResult(true, "Copied to clipboard.");
        }
        catch (Exception e)
        {
            return new ExecutionResult(false, e.Message);
        }
    }

    private static ExecutionResult DisplayEntry(string? arg)
    {
        try
        {
            var entry = _vaultManager.VaultData!.Entries.FirstOrDefault(e => e.Id == arg!);

            if (entry is null)
                return new ExecutionResult(false, "Entry not found.");

            entry.PrintAll();

            return ExecutionResult.SuccessNoMessage();   
        }
        catch (Exception e)
        {
            return new ExecutionResult(false, e.Message);
        }
    }

    #region [ Util ]
    private static string? AskUntilUnique(string text, Func<Entry, string?, bool> condition, bool canBeEmpty = false)
    {
        var result = Util.ReadNotEmptyText(text, canBeEmpty);

        if (_vaultManager.VaultData!.Entries.Any(entry => condition(entry, result)))
            return AskUntilUnique(text, condition, canBeEmpty);

        return result;
    }
    #endregion
}