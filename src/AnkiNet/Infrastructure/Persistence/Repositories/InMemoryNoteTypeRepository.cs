using AnkiNet.DomainModel;

namespace AnkiNet.Infrastructure.Persistence.Repositories;

internal sealed class InMemoryNoteTypeRepository
    : INoteTypeRepository
{
    private readonly Dictionary<NoteTypeId, NoteType> _noteTypes = [];

    public ValueTask AddAsync(NoteType noteType)
    {
        noteType = noteType.Clone();
        _noteTypes.Add(noteType.Id, noteType);
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> ExistsAsync(NoteTypeId noteTypeId)
        => ValueTask.FromResult(_noteTypes.ContainsKey(noteTypeId));

    public ValueTask<NoteType?> GetByIdAsync(NoteTypeId noteTypeId)
        => ValueTask.FromResult(_noteTypes.TryGetValue(noteTypeId, out var noteType)
            ? noteType.Clone()
            : null);

    public IAsyncEnumerable<NoteType> GetAllAsync()
        => _noteTypes.Values
            .Select(noteType => noteType.Clone())
            .ToAsyncEnumerable();

    public ValueTask UpdateAsync(NoteType noteType)
    {
        if (!_noteTypes.ContainsKey(noteType.Id))
            throw new InvalidOperationException($"Note type with ID {noteType.Id} does not exist.");

        if (noteType.IsDirty is false)
            return ValueTask.CompletedTask;

        noteType.IsDirty = false;
        _noteTypes[noteType.Id] = noteType.Clone();
        return ValueTask.CompletedTask;
    }
}
