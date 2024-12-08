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
        FieldValues = fieldValues.ToImmutableArray();
        SortField = FieldValues.FirstOrDefault().Value ?? string.Empty;
        Tags = tags.ToImmutableArray();
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

    public NoteTypeId NoteTypeId { get => field; internal set => SetPropertyValue(ref field, value); }

    public ImmutableArray<KeyValuePair<string, string>> FieldValues { get => field; internal set => SetPropertyValue(ref field, value); }

    public ImmutableArray<Tag> Tags
    {
        get => field;
        set => SetPropertyValue(ref field, value,
            validate: tags => tags.Distinct().Count() == tags.Length
            ? true
            : throw new ArgumentException("Duplicate tags are not allowed."));
    }

    public string Guid { get => field; set => SetPropertyValue(ref field, value); } = string.Empty;

    public long ModificationDateTime { get => field; set => SetPropertyValue(ref field, value); }

    public long UpdateSequenceNumber { get => field; set => SetPropertyValue(ref field, value); }

    /// <summary>
    /// Integer representation of first 8 digits of sha1 hash of the first field.
    /// </summary>
    public long FieldChecksum { get => field; set => SetPropertyValue(ref field, value); }

    public string SortField { get => field; set => SetPropertyValue(ref field, value); } = string.Empty;
}
