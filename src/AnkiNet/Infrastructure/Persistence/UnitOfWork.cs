using AnkiNet.DomainModel;
using AnkiNet.DomainModel.Base;
using System.Diagnostics.CodeAnalysis;

namespace AnkiNet.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly Dictionary<object, object> _registeredEntities = [];
    private readonly IDeckRepository _deckRepository;
    private readonly INoteRepository _noteRepository;
    private readonly INoteTypeRepository _noteTypeRepository;
    private readonly ICollectionRepository _collectionRepository;

    public UnitOfWork(
        IDeckRepository deckRepository,
        INoteRepository noteRepository,
        INoteTypeRepository noteTypeRepository,
        ICollectionRepository collectionRepository)
    {
        _deckRepository = deckRepository;
        _noteRepository = noteRepository;
        _noteTypeRepository = noteTypeRepository;
        _collectionRepository = collectionRepository;
    }

    public bool TryGetEntity<T>(object entityId, [NotNullWhen(returnValue: true)] out T? entity)
    {
        if (_registeredEntities.TryGetValue(entityId, out var registeredEntity))
        {
            entity = (T)registeredEntity;
            return true;
        }

        entity = default;
        return false;
    }

    public void Register(IEntity entity)
    {
        _registeredEntities[entity.Id] = entity;
    }

    public async Task CommitAsync()
    {
        foreach (var entity in _registeredEntities.Values)
        {
            switch (entity)
            {
                case Collection collection when collection.IsDirty:
                    await _collectionRepository.UpdateAsync(collection);
                    break;

                case Deck deck when deck.IsDirty:
                    await _deckRepository.UpdateAsync(deck);
                    break;

                case Note note when note.IsDirty:
                    await _noteRepository.UpdateAsync(note);
                    break;

                case NoteType noteType when noteType.IsDirty:
                    await _noteTypeRepository.UpdateAsync(noteType);
                    break;
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        _registeredEntities.Clear();
        return ValueTask.CompletedTask;
    }
}
