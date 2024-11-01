﻿using System.Collections.Immutable;

namespace AnkiNet;

/// <summary>
/// Defines an Anki note (or model) used as a template to create one or several cards.
/// </summary>
/// <remarks>This is called "model" in the Anki database.</remarks>
public readonly record struct AnkiNoteType
{
    /// <summary>
    /// Id of the note type.
    /// </summary>
    public long Id { get; }

    /// <summary>
    /// Name of the note type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Card types (templates) of the note type.
    /// </summary>
    public ImmutableArray<AnkiCardType> CardTypes { get; }

    /// <summary>
    /// Field names of the note type, used in the <see cref="AnkiCardType"/> templates.
    /// </summary>
    public ImmutableArray<string> FieldNames { get; }

    /// <summary>
    /// CSS to apply on the <see cref="AnkiCardType"/> templates.
    /// </summary>
    public string? Css { get; }

    /// <summary>
    /// Create a new note type, with undefined Id, to pass to <see cref="AnkiCollection.CreateNoteType(AnkiNoteType)"/>.
    /// </summary>
    /// <param name="id">The note type id.</param>
    /// <param name="name">Name of the note type.</param>
    /// <param name="cardTypes">Card types (templates) of the note type.</param>
    /// <param name="fieldNames">Field names of the note type, used in the <see cref="AnkiCardType"/> templates.</param>
    /// <param name="css">CSS to apply on the <see cref="AnkiCardType"/> templates</param>
    internal AnkiNoteType(long id, string name, IEnumerable<AnkiCardType> cardTypes, IEnumerable<string> fieldNames, string? css)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(cardTypes);
        ArgumentNullException.ThrowIfNull(fieldNames);

        Id = id;
        Name = name;
        CardTypes = cardTypes.ToImmutableArray();
        FieldNames = fieldNames.ToImmutableArray();
        Css = css;

        if (CardTypes.Length < 1)
        {
            throw new ArgumentException("AnkiNoteType needs at least one AnkiCardType");
        }

        if (cardTypes.DistinctBy(ct => ct.Name).Count() != CardTypes.Length)
        {
            throw new ArgumentException("AnkiNoteType cannot have duplicate AnkiCardType names");
        }

        if (FieldNames.Distinct().Count() != FieldNames.Length)
        {
            throw new ArgumentException("AnkiNoteType cannot have duplicate field names");
        }
    }
}
