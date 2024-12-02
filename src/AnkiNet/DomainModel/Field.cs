using AnkiNet.DomainModel.Base;
using System.Collections.Immutable;

namespace AnkiNet.DomainModel;

public sealed class Field
    : Entity<string>
{
    public static Field Create(
        string name,
        string description = "",
        string font = "",
        int fontSize = 20,
        bool isRightToLeft = false,
        bool isSticky = false,
        ImmutableArray<string>? media = null)
        => new (name)
        {
            Description = description,
            Font = font,
            FontSize = fontSize,
            IsRightToLeft = isRightToLeft,
            IsSticky = isSticky,
            Media = media ?? [],
        };

    public Field(string name)
        : base(name)
    {
        FontSize = 20;
        Font = "Arial";
        Media = [];
    }

    internal Field Clone()
        => new(name: Id)
        {
            IsRightToLeft = IsRightToLeft,
            IsSticky = IsSticky,
            Font = Font,
            FontSize = FontSize,
            Description = Description,
            Media = Media,
        };

    /// <summary>
    /// Field name (alias for Id).
    /// </summary>
    public string Name => Id;

    /// <summary>
    /// Whether this field uses right-to-left script.
    /// </summary>
    public bool IsRightToLeft { get; internal set; }

    /// <summary>
    /// Sticky fields retain the value that was last added when adding new notes.
    /// </summary>
    public bool IsSticky { get; internal set; }

    /// <summary>
    /// DisplayFont
    /// </summary>
    public string Font { get; internal set; } = string.Empty;

    /// <summary>
    /// Font size
    /// </summary>
    public int FontSize { get; internal set; }

    /// <summary>
    /// Undocumented
    /// </summary>
    public string Description { get; internal set; } = string.Empty;

    /// <summary>
    /// Array of media. appears to be unused.
    /// </summary>
    public ImmutableArray<string> Media { get; internal set; } = [];
}
