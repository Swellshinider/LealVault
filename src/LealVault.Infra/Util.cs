using LealVault.Infra.Models;

namespace LealVault.Infra;

public static class Util
{
    public static Entry ToEntry(this object data)
    {
        if (data is not Entry entry)
            throw new TypeLoadException($"Unexpected data type <{data.GetType()}>");

        return entry;
    }
}