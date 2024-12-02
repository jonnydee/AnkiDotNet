using System.Reflection;
using Microsoft.Data.Sqlite;
using System.Collections.Immutable;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Model;

namespace AnkiNet.Infrastructure.Persistence.AnkiFile.Database;

internal sealed class DatabaseReader
{
    public DatabaseReader()
    {
    }

    public async Task<DatabaseExtract> ReadDbAsync(string dbFile)
    {
        SQLitePCL.Batteries.Init();

        await using var conn = new SqliteConnection($"Data Source={dbFile};");
        await conn.OpenAsync();

        var col = (await new ColRepository(conn).ReadAll()).Single();
        var cards = await new CardRepository(conn).ReadAll();
        var graves = await new GraveRepository(conn).ReadAll();
        var notes = await new NoteRepository(conn).ReadAll();
        var revLogs = await new RevLogRepository(conn).ReadAll();

        return new(
            col,
            cards.ToImmutableArray(),
            graves.ToImmutableArray(),
            notes.ToImmutableArray(),
            revLogs.ToImmutableArray());
    }

    public async Task CreateAndPopulateDatabaseTables(string dbFile, DatabaseExtract dbExtract)
    {
        SqliteConnection? conn = null;

        try
        {
            SQLitePCL.Batteries.Init();

            conn = new SqliteConnection($"Data Source={dbFile};");
            await conn.OpenAsync();

            var col = ReadResource("AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Sql.ColTable.sql");
            var notes = ReadResource("AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Sql.NotesTable.sql");
            var cards = ReadResource("AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Sql.CardsTable.sql");
            var revLogs = ReadResource("AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Sql.RevLogTable.sql");
            var graves = ReadResource("AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Sql.GravesTable.sql");
            var indexes = ReadResource("AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Sql.Indexes.sql");

            await using var colCommand = new SqliteCommand(col, conn);
            await colCommand.ExecuteNonQueryAsync();
            await using var notesCommand = new SqliteCommand(notes, conn);
            await notesCommand.ExecuteNonQueryAsync();
            await using var cardsCommand = new SqliteCommand(cards, conn);
            await cardsCommand.ExecuteNonQueryAsync();
            await using var revLogsCommand = new SqliteCommand(revLogs, conn);
            await revLogsCommand.ExecuteNonQueryAsync();
            await using var gravesCommand = new SqliteCommand(graves, conn);
            await gravesCommand.ExecuteNonQueryAsync();
            await using var indexesCommand = new SqliteCommand(indexes, conn);
            await indexesCommand.ExecuteNonQueryAsync();

            await new ColRepository(conn).Add(new List<col> { dbExtract.col });
            await new NoteRepository(conn).Add(dbExtract.notes);
            await new CardRepository(conn).Add(dbExtract.cards);
            await new RevLogRepository(conn).Add(dbExtract.revLogs);
            await new GraveRepository(conn).Add(dbExtract.graves);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            conn?.Close();
            conn?.Dispose();
            SqliteConnection.ClearAllPools();
        }
    }

    private static string ReadResource(string path)
    {
        var a = Assembly.GetExecutingAssembly();
        var resourceStream = a.GetManifestResourceStream(path);
        if (resourceStream == null)
        {
            throw new FileNotFoundException($"Cannot find Embedded Resource '{path}' in assembly '{a.GetName().Name}'");
        }

        return new StreamReader(resourceStream).ReadToEnd();
    }
}