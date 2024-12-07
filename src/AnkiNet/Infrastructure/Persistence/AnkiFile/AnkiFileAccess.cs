using AnkiNet.DomainModel;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Database;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Model;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Mappers;
using AnkiNet.Infrastructure.Persistence.AnkiFile.MediaFile;
using System.Collections.Immutable;
using System.IO.Compression;
using ZstdSharp;

namespace AnkiNet.Infrastructure.Persistence.AnkiFile;

public sealed class AnkiFileAccess
{
    private readonly DatabaseReader _dbReader = new();
    private readonly IDeckRepository _deckRepository;
    private readonly INoteRepository _noteRepository;
    private readonly INoteTypeRepository _noteTypeRepository;
    private readonly ICollectionRepository _collectionRepository;

    public AnkiFileAccess(
        IDeckRepository deckRepository,
        INoteRepository noteRepository,
        INoteTypeRepository noteTypeRepository,
        ICollectionRepository collectionRepository)
    {
        ArgumentNullException.ThrowIfNull(deckRepository);
        ArgumentNullException.ThrowIfNull(noteRepository);
        ArgumentNullException.ThrowIfNull(noteTypeRepository);
        ArgumentNullException.ThrowIfNull(collectionRepository);

        _deckRepository = deckRepository;
        _noteRepository = noteRepository;
        _noteTypeRepository = noteTypeRepository;
        _collectionRepository = collectionRepository;
    }

    public async Task LoadAsync(string ankiFile)
    {
        await using var stream = File.OpenRead(ankiFile);
        await LoadAsync(stream).ConfigureAwait(false);
    }

    public async Task LoadAsync(Stream ankiFileStream)
    {
        string? dbFile = null;
        try
        {
            dbFile = ExtractDbFileFromAnkiFileStream(ankiFileStream);
            var dbExtract = await _dbReader.ReadDbAsync(dbFile).ConfigureAwait(false);
            await SaveDatabaseExtractAsync(dbExtract).ConfigureAwait(false);
        }
        finally
        {
            if (dbFile is not null)
                File.Delete(dbFile);
        }
    }

    public async Task SaveAsync(string ankiFile)
    {
        await using var stream = File.OpenWrite(ankiFile);
        await SaveAsync(stream).ConfigureAwait(false);
    }

    public async Task SaveAsync(Stream ankiFileStream)
    {
        string? ankiFilePath = null, mediaFilePath = null;
        try
        {
            var dbExtract = await LoadDatabaseExtractAsync().ConfigureAwait(false);

            ankiFilePath = Path.GetTempFileName();
            await _dbReader.CreateAndPopulateDatabaseTables(ankiFilePath, dbExtract).ConfigureAwait(false);

            mediaFilePath = Path.GetTempFileName();
            await MediaFileHandler.WriteMediaFile(mediaFilePath, null!);

            using var archive = new ZipArchive(ankiFileStream, ZipArchiveMode.Create, true);
            archive.CreateEntryFromFile(ankiFilePath, "collection.anki21");
            archive.CreateEntryFromFile(mediaFilePath, "media");
        }
        finally
        {
            if (ankiFilePath is not null)
                File.Delete(ankiFilePath);

            if (mediaFilePath is not null)
                File.Delete(mediaFilePath);
        }
    }

    private async Task SaveDatabaseExtractAsync(DatabaseExtract dbExtract)
    {
        var collection = CollectionMapper.CollectionFromDb(dbExtract.col);

        // Add note types.
        var noteTypes = CollectionMapper.NoteTypesFromDb(dbExtract.col).ToArray();
        foreach (var noteType in noteTypes)
        {
            collection.AddNoteType(noteType);
            await _noteTypeRepository.AddAsync(noteType).ConfigureAwait(false);
        }

        // Add notes.
        var notes = dbExtract.notes
            .Select(dbNote =>
            {
                var noteTypeId = new NoteTypeId(dbNote.mid);
                var noteType = noteTypes.Single(noteType => noteType.Id == noteTypeId);
                return NoteMapper.FromDb(dbNote, noteType);
            })
            .ToArray();
        foreach (var note in notes)
            await _noteRepository.AddAsync(note).ConfigureAwait(false);

        // Add decks and together with their cards.
        {
            // We need to lookup cards by deck ID to assign them to the correct deck.
            var getCardsByDeckId = dbExtract.cards
                .Select(card =>
                {
                    var noteId = new NoteId(card.nid);
                    var note = notes.Single(n => n.Id == noteId);
                    var noteType = noteTypes.Single(nt => nt.Id == note.NoteTypeId);
                    return CardMapper.FromDb(card, noteType);
                })
                .GroupBy(card => card.DeckId)
                .ToDictionary(
                    grouping => grouping.Key, // Deck ID.
                    grouping => grouping
                        .OrderBy(entry => entry.Card.Id)
                        .Select(entry => entry.Card)); // Cards.

            var getRevLogsByCardId = dbExtract.revLogs
                .Select(RevisionLogMapper.FromDb)
                .GroupBy(revLog => revLog.CardId)
                .ToDictionary(
                    grouping => grouping.Key, // Card ID.
                    grouping => grouping
                        .Select(entry => entry.RevisionLog)
                        .OrderBy(revLog => revLog.Id)); // Revision logs.

            foreach (var deck in CollectionMapper.DecksFromDb(dbExtract.col))
            {
                // Add cards to deck.
                {
                    var cards = getCardsByDeckId[deck.Id].ToArray();
                    foreach (var card in cards)
                    {
                        // Add revision logs to card.
                        {
                            var revLogs = getRevLogsByCardId[card.Id].ToArray();
                            foreach (var revLog in revLogs)
                                card.AddRevisionLog(revLog);
                        }

                        deck.AddCard(card);
                    }
                }

                collection.AddDeck(deck);

                await _deckRepository.AddAsync(deck).ConfigureAwait(false);
            }
        }

        // Add graves.
        foreach (var grave in dbExtract.graves.Select(GraveMapper.FromDb))
            collection.AddGrave(grave);

        await _collectionRepository.AddAsync(collection).ConfigureAwait(false);
    }

    private async Task<DatabaseExtract> LoadDatabaseExtractAsync()
    {
        var collections = await _collectionRepository.GetAllAsync().ToArrayAsync().ConfigureAwait(false);
        var decks = await _deckRepository.GetAllAsync().ToArrayAsync().ConfigureAwait(false);
        var notes = await _noteRepository.GetAllAsync().ToArrayAsync().ConfigureAwait(false);
        var noteTypes = await _noteTypeRepository.GetAllAsync().ToArrayAsync().ConfigureAwait(false);

        var collectionDb = collections
            .Select(collection => CollectionMapper.ToDb(collection, noteTypes, decks))
            .Single();

        var cardsDb = decks
            .SelectMany(deck => deck.Cards
                .Select(card =>
                {
                    var note = notes.Single(note => note.Id == card.NoteId);
                    var noteType = noteTypes.Single(noteType => noteType.Id == note.NoteTypeId);
                    return CardMapper.ToDb(card, deck.Id, noteType);
                }))
            .ToImmutableArray();

        var notesDb = notes
            .Select(NoteMapper.ToDb)
            .ToImmutableArray();

        var gravesDb = collections
            .SelectMany(collection => collection.Graves
                .Select(GraveMapper.ToDb))
            .ToImmutableArray();

        var revLogsDb = decks
            .SelectMany(deck => deck.Cards
                .SelectMany(card => card.RevisionLogs
                    .Select(revLog => (RevisionLog: revLog, CardId: card.Id))))
            .Select(entry => RevisionLogMapper.ToDb(entry.RevisionLog, entry.CardId))
            .ToImmutableArray();

        return new(
            col: collectionDb,
            cards: cardsDb,
            notes: notesDb,
            graves: gravesDb,
            revLogs: revLogsDb);
    }

    private static string ExtractDbFileFromAnkiFileStream(Stream stream)
    {
        using var zipArchive = new ZipArchive(stream);

        var entryNames = zipArchive.Entries.Select(e => e.Name);
        if (entryNames.Contains("collection.anki21b"))
        {
            throw new NotImplementedException("Anki.NET cannot yet open 2.1b files. Please export from Anki App using the 'Support older Anki versions' option.");
            var entry = zipArchive.GetEntry("collection.anki21b")!;

            var tempFileCompressed = Path.GetTempFileName();
            entry.ExtractToFile(tempFileCompressed, true);

            var tempFileDecompressed = "db_files/new.dbExtract"; // TODO Revert - Path.GetTempFileName();

            using var input = File.OpenRead(tempFileCompressed);
            using var output = File.OpenWrite(tempFileDecompressed);
            using var decompressionStream = new DecompressionStream(input);
            decompressionStream.CopyTo(output);

            File.Delete(tempFileCompressed);
            return tempFileDecompressed;
        }
        var databaseEntry = zipArchive.GetEntry("collection.anki21");
        if (databaseEntry == null)
        {
            databaseEntry = zipArchive.GetEntry("collection.anki2");
        }
        if (databaseEntry == null)
        {
            throw new InvalidOperationException("No collection SQLite file found in this Anki archive");
        }

        // Open DB file, as we cannot go from Stream to SqliteConnection
        var tempFile = Path.GetTempFileName();
        databaseEntry.ExtractToFile(tempFile, true);
        return tempFile;
    }
}
