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

    public static async Task<long> CreateAsync(Func<long, Task<bool>> idExists)
    {
        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        while (await idExists(id).ConfigureAwait(false))
        {
            ++id;
        }

        return id;
    }
}
