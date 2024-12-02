namespace AnkiNet.DomainModel;

public static class Tags
{
    private const char TagSeparator = ' ';

    public static IEnumerable<Tag> FromString(string str)
    {
        var tags = str.Split(TagSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return FromStrings(tags);
    }

    public static IEnumerable<Tag> FromStrings(params string[] tags)
    {
        var seen = new HashSet<string>(capacity: tags.Length);

        foreach (var tag in tags)
        {
            var newTagFound = seen.Add(tag);
            if (newTagFound)
                yield return Tag.Create(tag);
        }
    }

    public static string ToString(IEnumerable<Tag> tags)
        => string.Join(TagSeparator, tags.Select(tag => tag.Value));
}
