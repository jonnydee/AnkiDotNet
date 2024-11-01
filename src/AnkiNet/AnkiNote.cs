using System.Collections.Immutable;

namespace AnkiNet;

/// <summary>
/// Represents an Anki note, which can be mapped to one or more <see cref="AnkiCard"/>s.
/// </summary>
public readonly record struct AnkiNote
{
    /// <summary>
    /// Create a new Anki note.
    /// </summary>
    /// <param name="id">Id of the note.</param>
    /// <param name="noteTypeId">Id of the <see cref="AnkiNoteType"/> used as a template for this note.</param>
    /// <param name="fieldValues">Values of the fields of this note, matching (or having less fields than) <see cref="AnkiNoteType.FieldNames"/>.</param>
    internal AnkiNote(long id, long noteTypeId, IEnumerable<string> fieldValues)
    {
        Id = id;
        NoteTypeId = noteTypeId;
        FieldValues = fieldValues.ToImmutableArray();
    }

    /// <summary>
    /// Id of the note.
    /// </summary>
    public long Id { get; }

    /// <summary>
    /// Id of the <see cref="AnkiNoteType"/> used as a template for this note.
    /// </summary>
    public long NoteTypeId { get; }

    /// <summary>
    /// Field values of this note, matching (or having less fields than) <see cref="AnkiNoteType.FieldNames"/>.
    public ImmutableArray<string> FieldValues { get; }
}
