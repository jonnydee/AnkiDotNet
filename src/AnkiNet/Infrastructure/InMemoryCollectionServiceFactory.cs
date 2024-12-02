using AnkiNet.DomainModel;
using AnkiNet.Infrastructure.Persistence.Repositories;

namespace AnkiNet.Infrastructure;

public sealed class InMemoryCollectionServiceFactory
    : ICollectionServiceFactory
{
    public Task<ICollectionService> CreateCollectionServiceAsync()
    {
        ICollectionService instance = new CollectionService(
            new InMemoryDeckRepository(),
            new InMemoryNoteRepository(),
            new InMemoryNoteTypeRepository(),
            new InMemoryCollectionRepository());

        return Task.FromResult(instance);
    }
}
