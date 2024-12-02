namespace AnkiNet.DomainModel;

public readonly record struct Tag
{
    public static Tag Create(string name)
        => new(name);

    public Tag(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value;
    }

    public string Value { get; }
}
