namespace AnkiNet.DomainModel;

public readonly record struct DeckId(long Value)
    : IComparable<DeckId>
{
    public static readonly DeckId Empty = new(0);

    public int CompareTo(DeckId other)
        => Value.CompareTo(other.Value);
}
