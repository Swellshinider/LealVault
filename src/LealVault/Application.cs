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
        "You can press Ctrl + C anytime to exit.\n".WriteLine();

        var repl = new Repl();

        while (true)
        {
            var input = repl.Run();
            var executionResult = CommandHandler.Execute(input);

            $"{executionResult.Message}".WriteLine(executionResult.Success
                                                                ? ConsoleColor.Green 
                                                                : ConsoleColor.Red);
            if (executionResult.ShouldExit)
            {
                "Exiting...\n".WriteLine();
                break;
            }
        }
    }
}