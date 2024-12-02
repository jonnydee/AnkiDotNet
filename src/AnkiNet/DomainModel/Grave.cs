namespace AnkiNet.DomainModel;

/// <summary>
/// Represents a record of a deleted item (note, card, or deck).
/// </summary>
/// <param name="Type">The type of item that was deleted.</param>
/// <param name="OriginalId">The unique identifier of the item that was deleted.</param>
/// <param name="UpdateSequenceNumber">The update sequence number of the item that was deleted.</param>
public readonly record struct Grave(
    GraveType Type,
    long OriginalId,
    long UpdateSequenceNumber
)
{
    public NoteId? GetNoteId()
        => GraveType.Note == Type
            ? new(OriginalId)
            : null;

    public DeckId? GetDeckId()
        => GraveType.Deck == Type
            ? new(OriginalId)
            : null;

    public long? GetCardId()
        => GraveType.Card == Type
            ? OriginalId
            : null;
}
