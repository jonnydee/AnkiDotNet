using AnkiNet.DomainModel;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Database;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Mappers;

namespace AnkiNet.Infrastructure.Persistence.Repositories;

internal sealed class SqliteCollectionRepository
    : ICollectionRepository
{
    private readonly ColRepository _colRepositoryDb;

    public SqliteCollectionRepository(ColRepository colRepositoryDb)
    {
        ArgumentNullException.ThrowIfNull(colRepositoryDb);

        _colRepositoryDb = colRepositoryDb;
    }

    public ValueTask AddAsync(Collection collection)
        => throw new NotImplementedException();

    public ValueTask<bool> ExistsAsync(CollectionId collectionId)
        => _colRepositoryDb.ReadAllAsync()
            .AnyAsync(dbCollection => dbCollection.id == collectionId.Value);

    public ValueTask<Collection?> GetByIdAsync(CollectionId collectionId)
        => GetAllAsync()
            .SingleOrDefaultAsync(collection => collection.Id == collectionId);

    public IAsyncEnumerable<Collection> GetAllAsync()
        => _colRepositoryDb.ReadAllAsync()
            .Select(CollectionMapper.CollectionFromDb);

    public ValueTask UpdateAsync(Collection collection)
        => throw new NotImplementedException();
}
