using AnkiNet.DomainModel;

namespace AnkiNet.Infrastructure.Persistence.Repositories;

internal sealed class InMemoryNoteRepository
    : INoteRepository
{
    private readonly Dictionary<NoteId, Note> _notes = [];

    public ValueTask AddAsync(Note note)
    {
        note = note.Clone();
        _notes.Add(note.Id, note);
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> ExistsAsync(NoteId noteId)
        => ValueTask.FromResult(_notes.ContainsKey(noteId));

    public ValueTask<Note?> GetByIdAsync(NoteId noteId)
        => ValueTask.FromResult(_notes.TryGetValue(noteId, out var note)
            ? note.Clone()
            : null);

    public IAsyncEnumerable<Note> GetAllAsync()
        => _notes.Values
            .Select(note => note.Clone())
            .ToAsyncEnumerable();

    public ValueTask UpdateAsync(Note note)
    {
        if (!_notes.ContainsKey(note.Id))
            throw new InvalidOperationException($"Note with ID {note.Id} does not exist.");

        if (note.IsDirty is false)
            return ValueTask.CompletedTask;

        note.IsDirty = false;
        _notes[note.Id] = note.Clone();
        return ValueTask.CompletedTask;
    }
}
