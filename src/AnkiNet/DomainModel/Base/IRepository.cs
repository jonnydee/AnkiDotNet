namespace AnkiNet.DomainModel.Base;

public interface IRepository<TAggregateRoot, TId>
    where TAggregateRoot : AggregateRoot<TId>
    where TId : notnull
{
    ValueTask AddAsync(TAggregateRoot aggregateRoot);

    ValueTask<bool> ExistsAsync(TId id);

    ValueTask<TAggregateRoot?> GetByIdAsync(TId id);

    IAsyncEnumerable<TAggregateRoot> GetAllAsync();

    ValueTask UpdateAsync(TAggregateRoot aggregateRoot);
}
