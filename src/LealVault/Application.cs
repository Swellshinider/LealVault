using LealVault.Infra;
using LealVault.Infra.Commands;
using LealVault.Infra.Repl;

namespace LealVault;

public static class Application
{
    /// <summary>
    /// Runs the application.
    /// </summary>
    public static void Run()
    {
        "Welcome to LealVault CLI!\nType ".Write();
        "help ".Write(ConsoleColor.Green);
        "to see available commands.\n".WriteLine();

        var repl = new Repl();
        ExecutionResult? lastResult = null;

        while (true)
        {
            var input = repl.Run(lastResult);
            lastResult = CommandHandler.Execute(input);

            if (!lastResult.Message.IsNull())
            {
                var color = lastResult.Success ? ConsoleColor.Green : ConsoleColor.Red;
                lastResult.Message.WriteLine(color);
            }

            if (lastResult.ShouldExit)
            {
                "Exiting...".WriteLine();
                break;
            }
        }
    }
}