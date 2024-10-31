using Microsoft.Data.Sqlite;

namespace AnkiNet.CollectionFile.Database;

internal abstract class SqliteRepository<T>
{
    private readonly SqliteConnection _connection;

    protected abstract string TableName { get; }
    protected abstract string Columns { get; }

    protected abstract IReadOnlyList<object> GetValues(T item);


    protected abstract T Map(SqliteDataReader reader);

    protected SqliteRepository(SqliteConnection connection)
    {
        _connection = connection;
    }

    public async Task<List<T>> ReadAll()
    {
        var result = new List<T>();

        var readAllSqlQuery = $"SELECT {Columns} FROM {TableName}";

        try
        {
            using var command = new SqliteCommand(readAllSqlQuery, _connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var item = Map(reader);
                result.Add(item);
            }
        }
        catch (Exception e)
        {
            throw new IOException($"Cannot ReadAll {typeof(T).Name}", e);
        }

        return result;
    }

    public async Task Add(IReadOnlyList<T> items)
    {
        if (items.Count == 0)
        {
            return;
        }

        string writeSqlQuery;
        {
            var values = items.Select((item, itemIndex) =>
            {
                var itemValueCount = GetValues(item).Length;

                var @params = Enumerable.Range(0, itemValueCount)
                    .Select(paramIndex => ParamName(itemIndex, paramIndex));

                return $"({string.Join(',', @params)})";
            });

            writeSqlQuery = $"INSERT INTO {TableName} ({Columns}) VALUES {string.Join(',', values)}";
        }

        try
        {
            await using var command = new SqliteCommand(writeSqlQuery, _connection);

            foreach (var (item, itemIndex) in items.Select((item, itemIndex) => (item, itemIndex)))
            {
                var itemValues = GetValues(item);
                foreach (var (itemValue, paramIndex) in itemValues.Select((itemValue, paramIndex) => (itemValue, paramIndex)))
                {
                    var paramName = ParamName(itemIndex, paramIndex);
                    command.Parameters.AddWithValue(paramName, itemValue);
                }
            }

            var numberOfItemsInserted = await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            throw new IOException($"Cannot Add {typeof(T).Name}", e);
        }

        #region Helper function(s)

        static string ParamName(int itemIndex, int paramIndex) => $"@p{itemIndex}_{paramIndex}";

        #endregion
    }
}