namespace AnkiNet;

internal static class IdFactory
{
    public static long Create(Func<long, bool> idExists)
    {
        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        while (idExists(id))
        {
            ++id;
        }

        return id;
    }
}