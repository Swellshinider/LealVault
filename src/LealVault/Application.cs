using LealVault.Infra;
using LealVault.Infra.Models;

namespace LealVault;

public static class Application
{
    private static readonly List<Operation> history = [];
    private static readonly VaultManager vaultManager = new();
    private static readonly CancellationTokenSource cts = new();

    private static bool _readingState = false;

    public static async Task RunAsync()
    {
        Console.CancelKeyPress += (s, e) => Exit(e);

        "Welcome to LealVault CLI!\nType ".Write();
        "help ".Write(ConsoleColor.Green);
        "to see available commands.\n".WriteLine(ConsoleColor.Green);

        var commandHandler = new CommandHandler(cts.Token);

        while (!cts.IsCancellationRequested)
        {
            "> ".Write(ConsoleColor.Cyan);
            _readingState = true;
            var input = Console.ReadLine();
            _readingState = false;

            if (string.IsNullOrEmpty(input) ||
                string.IsNullOrWhiteSpace(input))
                continue;

            var operation = await commandHandler.Execute(input);

            $"EXECUTING: {operation}".WriteLine();
            HandleOperation(operation);
        }
    }

    private static void HandleOperation(OperationType operationType)
    {
        if (operationType == OperationType.None)
            return;

        if (operationType == OperationType.Exit)
        {
            Exit(null);
            return;
        }

        switch (operationType)
        {
            case OperationType.Add: AddEntry(); break;
            case OperationType.Update: UpdateEntry(); break;
            case OperationType.Delete: DeleteEntry(); break;
        }
    }

    private static void AddEntry()
    {
    }

    private static void UpdateEntry()
    {   
    }

    private static void DeleteEntry()
    {
    }

    private static void Exit(ConsoleCancelEventArgs? e)
    {
        if (e is not null && _readingState == false)
            e.Cancel = true;

        cts.Cancel();
    }
}