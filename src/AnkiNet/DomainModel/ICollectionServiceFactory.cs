namespace AnkiNet.DomainModel;

public interface ICollectionServiceFactory
{
    Task<ICollectionService> CreateCollectionServiceAsync();
}