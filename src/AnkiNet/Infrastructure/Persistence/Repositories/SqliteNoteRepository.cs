using AnkiNet.DomainModel;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Database;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Mappers;

namespace AnkiNet.Infrastructure.Persistence.Repositories;

internal sealed class SqliteNoteRepository
    : INoteRepository
{
    private readonly NoteRepository _noteRepositoryDb;
    private readonly ColRepository _colRepositoryDb;

    public SqliteNoteRepository(NoteRepository noteRepositoryDb, ColRepository colRepositoryDb)
    {
        ArgumentNullException.ThrowIfNull(noteRepositoryDb);
        ArgumentNullException.ThrowIfNull(colRepositoryDb);

        _noteRepositoryDb = noteRepositoryDb;
        _colRepositoryDb = colRepositoryDb;
    }

    public ValueTask AddAsync(Note note)
        => throw new NotImplementedException();

    public ValueTask<bool> ExistsAsync(NoteId noteId)
        => _noteRepositoryDb.ReadAllAsync()
            .AnyAsync(dbNote => dbNote.id == noteId.Value);

    public ValueTask<Note?> GetByIdAsync(NoteId noteId)
        => GetAllAsync()
            .SingleOrDefaultAsync(note => note.Id == noteId);

    public async IAsyncEnumerable<Note> GetAllAsync()
    {
        var noteTypes = await _colRepositoryDb.ReadAllAsync()
            .Select(CollectionMapper.NoteTypesFromDb)
            .SelectMany(noteTypes => noteTypes.ToAsyncEnumerable())
            .ToArrayAsync()
            .ConfigureAwait(false);

        var notes = _noteRepositoryDb.ReadAllAsync()
            .Select(dbNote =>
            {
                var noteTypeId = new NoteTypeId(dbNote.mid);
                var noteType = noteTypes.Single(noteType => noteType.Id == noteTypeId);
                return NoteMapper.FromDb(dbNote, noteType);
            });

        await foreach (var note in notes.ConfigureAwait(false))
            yield return note;
    }

    public ValueTask UpdateAsync(Note note)
        => throw new NotImplementedException();
}
