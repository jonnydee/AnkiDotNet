using AnkiNet.DomainModel.Base;
using System.Diagnostics.CodeAnalysis;

namespace AnkiNet.Infrastructure.Persistence;

public interface IUnitOfWork : IAsyncDisposable
{

    bool TryGetEntity<T>(object entityId, [NotNullWhen(returnValue: true)] out T? entity);

    void Register(IEntity entity);

    Task CommitAsync();
}
