using AnkiNet.DomainModel;

namespace AnkiNet.Infrastructure.Persistence.Repositories;

internal sealed class InMemoryCollectionRepository
    : ICollectionRepository
{
    private readonly Dictionary<CollectionId, Collection> _collections = [];

    public ValueTask AddAsync(Collection collection)
    {
        collection = collection.Clone();
        _collections.Add(collection.Id, collection);
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> ExistsAsync(CollectionId collectionId)
        => ValueTask.FromResult(_collections.ContainsKey(collectionId));

    public ValueTask<Collection?> GetByIdAsync(CollectionId collectionId)
        => ValueTask.FromResult(_collections.TryGetValue(collectionId, out var collection)
            ? collection.Clone()
            : null);

    public IAsyncEnumerable<Collection> GetAllAsync()
        => _collections.Values
            .Select(collection => collection.Clone())
            .ToAsyncEnumerable();

    public ValueTask UpdateAsync(Collection collection)
    {
        if (!_collections.ContainsKey(collection.Id))
            throw new InvalidOperationException($"Collection with ID {collection.Id} does not exist.");

        if (collection.IsDirty is false)
            return ValueTask.CompletedTask;

        collection.IsDirty = false;
        _collections[collection.Id] = collection.Clone();
        return ValueTask.CompletedTask;
    }
}
