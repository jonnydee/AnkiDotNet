using AnkiNet.DomainModel;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Database;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Mappers;

namespace AnkiNet.Infrastructure.Persistence.Repositories;

internal sealed class SqliteNoteTypeRepository
    : INoteTypeRepository
{
    private readonly ColRepository _colRepositoryDb;

    public SqliteNoteTypeRepository(ColRepository colRepositoryDb)
    {
        ArgumentNullException.ThrowIfNull(colRepositoryDb);

        _colRepositoryDb = colRepositoryDb;
    }

    public ValueTask AddAsync(NoteType noteType)
        => throw new NotImplementedException();

    public ValueTask<bool> ExistsAsync(NoteTypeId noteTypeId)
        => GetAllAsync()
            .AnyAsync(noteType => noteType.Id == noteTypeId);

    public ValueTask<NoteType?> GetByIdAsync(NoteTypeId noteTypeId)
        => GetAllAsync()
            .SingleOrDefaultAsync(noteType => noteType.Id == noteTypeId);

    public IAsyncEnumerable<NoteType> GetAllAsync()
        => _colRepositoryDb.ReadAllAsync()
            .Select(CollectionMapper.NoteTypesFromDb)
            .SelectMany(noteTypes => noteTypes.ToAsyncEnumerable());

    public ValueTask UpdateAsync(NoteType noteType)
        => throw new NotImplementedException();
}
