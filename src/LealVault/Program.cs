namespace LealVault;

public static class Program
{
    public static async Task<int> Main()
    {
        try
        {
            await Application.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            $"Error: {ex.Message}\n".WriteLine(ConsoleColor.Red);
            return -1;
        }
    }
}