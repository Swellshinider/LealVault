namespace LealVault.Infra.Security;

internal static class Id
{
    private static readonly Random _random = new Random();
    private static readonly char[] _chars = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

    public static string GenerateUniqueId(List<string> existingIds, int maxAttempts = 10)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var newId = GenerateId();

            if (!existingIds.Contains(newId))
                return newId;
        }

        throw new InvalidOperationException($"Failed to generate unique ID after {maxAttempts} attempts");
    }

    private static string GenerateId()
        => new([.. Enumerable.Range(0, 5).Select(_ => _chars[_random.Next(_chars.Length)])]);
}