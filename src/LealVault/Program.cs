using LealVault.Infra;

namespace LealVault;

public static class Program
{
    /// <summary>
    /// The entry point of the application.
    /// </summary>
    public static int Main()
    {
        try
        {
            Application.Run();
            return 0;
        }
        catch (Exception ex)
        {
            $"Error: {ex.Message}\n".WriteLine(ConsoleColor.Red);
            return -1;
        }
    }
}