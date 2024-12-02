using AnkiNet.DomainModel.Base;

namespace AnkiNet.DomainModel;

public interface ICollectionRepository
    : IRepository<Collection, CollectionId>
{
}
