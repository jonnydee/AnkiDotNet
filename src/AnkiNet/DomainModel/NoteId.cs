namespace AnkiNet.DomainModel;

public readonly record struct NoteId(long Value)
    : IComparable<NoteId>
{
    public static readonly NoteId Empty = new(0);

    public int CompareTo(NoteId other)
        => Value.CompareTo(other.Value);
}
