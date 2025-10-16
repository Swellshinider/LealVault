using LealVault.Infra;
using LealVault.Infra.Commands;
using LealVault.Infra.Repl;

namespace LealVault;

public static class Application
{
    /// <summary>
    /// Runs the application.
    /// </summary>
    public static async void Run()
    {
        "Welcome to LealVault CLI!\nType ".Write();
        "help ".Write(ConsoleColor.Green);
        "to see available commands.\n".WriteLine();

        var cancelationTokenSource = new CancellationTokenSource();
        var repl = new Repl(cancelationTokenSource.Token);
        ExecutionResult? lastResult = null;

        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cancelationTokenSource.Cancel();
        };

        while (!cancelationTokenSource.Token.IsCancellationRequested)
        {
            var input = await repl.Run(lastResult);
            lastResult = cancelationTokenSource.Token.IsCancellationRequested
                ? CommandHandler.Exit()
                : CommandHandler.Execute(input);

            if (ProcessExecutionResult(lastResult))
                break;
        }
    }
    
    private static bool ProcessExecutionResult(ExecutionResult result)
    {
        if (!result.Message.IsNull())
        {
            var color = result.Success ? ConsoleColor.Green : ConsoleColor.Red;
            result.Message.WriteLine(color);
        }

        if (result.ShouldExit)
        {
            "Exiting...".WriteLine();
            return true;
        }

        return false;
    }
}