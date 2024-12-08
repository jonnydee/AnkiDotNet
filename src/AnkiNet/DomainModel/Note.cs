using System.Collections.Immutable;
using AnkiNet.DomainModel.Base;

namespace AnkiNet.DomainModel;

/// <summary>
/// Stores actual values for fields defined by the <see cref="NoteType"/>.
/// </summary>
public sealed class Note
    : AggregateRoot<NoteId>
{
    public static Note Create(
        NoteId id,
        NoteType noteType,
        IEnumerable<KeyValuePair<string, string>> fieldValues,
        IEnumerable<Tag> tags)
    {
        ArgumentNullException.ThrowIfNull(noteType);
        ArgumentNullException.ThrowIfNull(fieldValues);
        ArgumentNullException.ThrowIfNull(tags);

        // Check that the field values match the note type.
        if (fieldValues.Count() != noteType.Fields.Length)
            throw new ArgumentException("The number of field values does not match the number of fields.");

        var fieldValuesArray = fieldValues.ToArray();
        for (var i = 0; i < fieldValuesArray.Length; ++i)
        {
            if (fieldValuesArray[i].Key != noteType.Fields[i].Name)
                throw new ArgumentException("The field names do not match the note type.");
        }

        return new(id, noteType.Id, fieldValues, tags)
        {
            IsDirty = true,
        };
    }

    public Note(
        NoteId id, NoteTypeId noteTypeId, IEnumerable<KeyValuePair<string, string>> fieldValues, IEnumerable<Tag> tags)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(fieldValues);
        ArgumentNullException.ThrowIfNull(tags);

        NoteTypeId = noteTypeId;
        FieldValues = fieldValues.ToImmutableArray(); // TODO Use array for preserving order?
        SortField = FieldValues.FirstOrDefault().Value ?? string.Empty;
        Tags = tags.ToImmutableArray();

        if (Tags.Distinct().Count() != Tags.Length)
            throw new ArgumentException("Duplicate tags are not allowed.");

        Guid = System.Guid.NewGuid().ToString().Substring(0, 10);
    }

    internal Note Clone()
        => new(Id, NoteTypeId, FieldValues, Tags)
        {
            Guid = Guid,
            ModificationDateTime = ModificationDateTime,
            UpdateSequenceNumber = UpdateSequenceNumber,
            FieldChecksum = FieldChecksum,
            SortField = SortField,
            IsDirty = IsDirty,
        };

    public bool IsDirty { get; set; }

    public NoteTypeId NoteTypeId { get; internal set; }

    public ImmutableArray<KeyValuePair<string, string>> FieldValues { get; internal set; }

    public ImmutableArray<Tag> Tags { get; internal set; }

    public string Guid { get; internal set; } = string.Empty;

    public long ModificationDateTime { get; internal set; }

    public long UpdateSequenceNumber { get; internal set; }

    /// <summary>
    /// Integer representation of first 8 digits of sha1 hash of the first field.
    /// </summary>
    public long FieldChecksum { get; internal set; }

    public string SortField { get; internal set; } = string.Empty;
}
