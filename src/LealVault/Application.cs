using LealVault.Infra;
using LealVault.Infra.Commands;
using LealVault.Infra.Vault;

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
        "to see available commands.\n".WriteLine(ConsoleColor.Green);

        while (true)
        {
            "> ".Write(ConsoleColor.Cyan);
            var input = (Console.ReadLine() ?? "").Trim();

            if (string.IsNullOrEmpty(input) ||
                string.IsNullOrWhiteSpace(input))
                continue;

            var executionResult = CommandHandler.Execute(input);

            if (!executionResult.Success)
                executionResult.Message.WriteLine(ConsoleColor.Red);

            if (executionResult.ShouldExit)
            {
                "Exiting...".WriteLine();
                return;
            }
        }
    }
}