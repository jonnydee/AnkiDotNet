namespace AnkiNet.DomainModel;

public readonly record struct NoteTypeId(long Value)
{
    public static readonly NoteTypeId Empty = new(0);
}
