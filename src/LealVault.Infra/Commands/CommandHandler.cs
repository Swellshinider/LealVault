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
                Usage = "copy <id> <type> | Copies the specified entry data to the clipboard.\n\n" +
                "You have this options for <type>:\n" +
                "    p - copy the password\n" +
                "    e - copy the email\n"
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
            return ExecutionResult.Fail(_vaultManager, $"Unknown command: {inputCommand}");

        var command = GetCommand(commandType);

        if (command is null)
            return ExecutionResult.Fail(_vaultManager, $"Unknown command: {inputCommand}");

        var execResult = command.Execute(arguments, out var validationMessage);

        if (execResult is null)
            return ExecutionResult.Fail(_vaultManager, validationMessage);

        return execResult;
    }

    /// <summary>
    /// Executes the exit command.
    /// </summary>
    public static ExecutionResult Exit() => Exit(null);

    #region [ Validation Methods ]
    private static bool VaultShouldBeClosed(string? input, out string message)
    {
        if (_vaultManager.IsOpen)
        {
            message = "A vault is already open.";
            return false;
        }

        message = "";
        return true;
    }

    private static bool VaultShouldBeOpen(string? input, out string message)
    {
        if (!_vaultManager.IsOpen)
        {
            message = "No vault is currently open.";
            return false;
        }

        message = "";
        return true;
    }

    private static bool ArgumentCantBeNull(string? input, out string message)
    {
        if (input.IsNull())
        {
            message = "This command requires an argument, please see help <command> for more details.";
            return false;
        }

        message = "";
        return true;
    }
    #endregion

    #region [ Commands ]
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
            return ExecutionResult.SuccessResult(_vaultManager);
        }

        var command = GetCommand(arg!);

        if (command is null)
            return ExecutionResult.Fail(_vaultManager, $"Unknown command: {arg}");

        "Help ".Write();
        $"{arg}:".WriteLine(ConsoleColor.Green);

        $"    {command.Usage}".WriteLine();
        "".WriteLine();

        return ExecutionResult.SuccessResult(_vaultManager);
    }

    private static ExecutionResult Exit(string? arg)
    {
        var closeResult = _vaultManager.IsOpen ? CloseVault(arg) : ExecutionResult.SuccessResult(_vaultManager);

        if (!closeResult.Success)
        {
            $"Unable to close vault. {closeResult.Message}".WriteLine(ConsoleColor.Red);
            if (!Util.ConfirmUserAction("Do you want to exit anyway?"))
                return Exit(arg);
        }

        return ExecutionResult.Exit(_vaultManager);
    }

    private static ExecutionResult Clear(string? arg)
    {
        Console.Clear();
        return ExecutionResult.SuccessResult(_vaultManager);
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
                return ExecutionResult.Fail(_vaultManager, "Passwords do not match.");

            "You'll not be able to change it later.".WriteLine(ConsoleColor.Yellow);
            "If you forgot your master password, you'll have to create a new vault.".WriteLine(ConsoleColor.Yellow);

            if (!Util.ConfirmUserAction())
                return ExecutionResult.Fail(_vaultManager, "Vault creation canceled.");

            _vaultManager.MasterPassword = masterPassword;

            var success = _vaultManager.Create(arg!);

            if (!success)
                return ExecutionResult.Fail(_vaultManager, "Vault could not be created.");
       
            return ExecutionResult.Fail(_vaultManager, "Vault creation finished.");
        }
        catch (Exception e)
        {
            return ExecutionResult.Error(_vaultManager, e);
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

            if (!success)
                return ExecutionResult.Fail(_vaultManager, "Vault could not be opened.");

            return ExecutionResult.SuccessResult(_vaultManager, "Vault opened successfully.");
        }
        catch (Exception e)
        {
            return ExecutionResult.Error(_vaultManager, e);
        }
    }

    private static ExecutionResult SaveVault(string? arg)
    {
        try
        {
            "Saving vault...".WriteLine();

            if (!Util.ConfirmUserAction())
                return ExecutionResult.Fail(_vaultManager, "Vault saving canceled.");

            var success = _vaultManager.Save();

            if (!success)
                return ExecutionResult.Fail(_vaultManager, "Vault could not be saved.");

            return ExecutionResult.SuccessResult(_vaultManager, "Vault saved successfully.");
        }
        catch (Exception e)
        {
            return ExecutionResult.Error(_vaultManager, e);
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
                        return ExecutionResult.Fail(_vaultManager, "Vault could not be saved.");
                }
                else
                    "Vault will not be saved.".WriteLine(ConsoleColor.Yellow);
            }

            _vaultManager.Close();
            return ExecutionResult.SuccessResult(_vaultManager, "Vault closed successfully.");
        }
        catch (Exception e)
        {
            return ExecutionResult.Error(_vaultManager, e);
        }
    }

    private static ExecutionResult SearchEntries(string? arg)
    {
        try
        {
            var entries = _vaultManager.VaultData!.Entries;

            if (entries.Count == 0)
                return ExecutionResult.SuccessResult(_vaultManager, "You have no entries in your vault.");

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

                return ExecutionResult.SuccessResult(_vaultManager, "Search finished.");
            }

            $"Searcing for: ".Write();
            $"{arg!}".WriteLine(ConsoleColor.Green);
            var search = entries.Where(e => e.Name.Contains(arg!, StringComparison.InvariantCultureIgnoreCase) ||
                                             e.Tag.Contains(arg!, StringComparison.InvariantCultureIgnoreCase));

            if (search is null || search?.Count() == 0)
                return ExecutionResult.SuccessResult(_vaultManager, "No entries found.");

            foreach (var entry in search!)
                $"{entry}".WriteLine();

            return ExecutionResult.SuccessResult(_vaultManager, $"Search finished, {search.Count()} entries found.");
        }
        catch (Exception e)
        {
            return ExecutionResult.Error(_vaultManager, e);
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

            if (!success)
                return ExecutionResult.Fail(_vaultManager, "Entry could not be added.");

            return ExecutionResult.SuccessResult(_vaultManager, "Entry added successfully.");
        }
        catch (Exception e)
        {
            return ExecutionResult.Error(_vaultManager, e);
        }
    }

    private static ExecutionResult RemoveEntry(string? arg)
    {
        try
        {
            var entry = _vaultManager.VaultData!.Entries.FirstOrDefault(e => e.Id == arg!);

            if (entry is null)
                return ExecutionResult.Fail(_vaultManager, "Entry not found.");

            "Entry found!".WriteLine();
            entry.ToString().WriteLine(ConsoleColor.Green);

            if (!Util.ConfirmUserAction("Are you sure you want to remove this entry?"))
                return ExecutionResult.Fail(_vaultManager, "Entry removal canceled.");

            var success = _vaultManager.Remove(entry);

            if (!success)
                return ExecutionResult.Fail(_vaultManager, "Entry could not be removed.");

            return ExecutionResult.SuccessResult(_vaultManager, "Entry removed successfully.");
        }
        catch (Exception e)
        {
            return ExecutionResult.Error(_vaultManager, e);
        }
    }

    private static ExecutionResult UpdateEntry(string? arg)
    {
        try
        {
            var entry = _vaultManager.VaultData!.Entries.FirstOrDefault(e => e.Id == arg!);

            if (entry is null)
                return ExecutionResult.Fail(_vaultManager, "Entry not found.");

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
                return ExecutionResult.Fail(_vaultManager, "Entry update canceled.");

            var removeOldResult = _vaultManager.Remove(entry);

            if (!removeOldResult)
                return ExecutionResult.Fail(_vaultManager, "Old entry could not be removed.");

            var addResult = _vaultManager.Add(updatedEntry);

            if (!addResult)
                return ExecutionResult.Fail(_vaultManager, "New entry could not be added.");

            return ExecutionResult.SuccessResult(_vaultManager, "Entry updated successfully.");
        }
        catch (Exception e)
        {
            return ExecutionResult.Error(_vaultManager, e);
        }
    }

    private static ExecutionResult CopyEntry(string? arg)
    {
        try
        {
            var split = arg!.Split(' ', StringSplitOptions.None);

            if (split.Length != 2)
                return ExecutionResult.Fail(_vaultManager, "Invalid arguments. You should provide <id> and <type> only.");

            var id = split[0].Trim().ToLower();
            var type = split[1].Trim().ToLower();

            var entry = _vaultManager.VaultData!.Entries.FirstOrDefault(e => e.Id == id);

            if (entry is null)
                return ExecutionResult.Fail(_vaultManager, $"Entry not found with id: {id}.");

            if (type == "p")
            {
                ClipboardService.SetText(entry.Password);
                return ExecutionResult.SuccessResult(_vaultManager, "Password copied to clipboard.");
            }

            if (type == "e")
            {
                ClipboardService.SetText(entry.Email ?? "");
                return ExecutionResult.SuccessResult(_vaultManager, "Email copied to clipboard.");
            }

            return ExecutionResult.Fail(_vaultManager, "Invalid type. You should provide p or e.");
        }
        catch (Exception e)
        {
            return ExecutionResult.Error(_vaultManager, e);
        }
    }

    private static ExecutionResult DisplayEntry(string? arg)
    {
        try
        {
            var entry = _vaultManager.VaultData!.Entries.FirstOrDefault(e => e.Id == arg!);

            if (entry is null)
                return ExecutionResult.Fail(_vaultManager, "Entry not found.");

            entry.PrintAll();

            return ExecutionResult.SuccessResult(_vaultManager);
        }
        catch (Exception e)
        {
            return ExecutionResult.Error(_vaultManager, e);
        }
    }
    #endregion

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