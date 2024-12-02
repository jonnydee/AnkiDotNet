namespace AnkiNet.DomainModel;

public readonly record struct DeckId(long Value)
{
    public static readonly DeckId Empty = new(0);
}
