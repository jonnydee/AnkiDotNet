namespace AnkiNet.DomainModel;

public readonly record struct NoteTypeId(long Value)
    : IComparable<NoteTypeId>
{
    public static readonly NoteTypeId Empty = new(0);

    public int CompareTo(NoteTypeId other)
        => Value.CompareTo(other.Value);
}
