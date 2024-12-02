using AnkiNet.CollectionFile.Database.Model;
using AnkiNet.DomainModel;

namespace AnkiNet.Infrastructure.Persistence.AnkiFile.Mappers;

internal static class NoteMapper
{
    private const char FieldSeparator = '\u001f'; // Unicode Information Separator One / Unit Separator.

    public static Note FromDb(note note, NoteType noteType)
    {
        var fieldValues = note.flds
            .Split(FieldSeparator)
            .Zip(
                second: noteType.Fields,
                resultSelector: (fieldValue, field) => KeyValuePair.Create(field.Name, fieldValue));

        var tags = Tags.FromString(note.tags);

        return new(
            id: new NoteId(note.id),
            noteTypeId: new NoteTypeId(note.mid),
            fieldValues: fieldValues,
            tags: tags
        )
        {
            FieldChecksum = note.csum,
            Guid = note.guid,
            ModificationDateTime = note.mod,
            SortField = note.sfld,
            UpdateSequenceNumber = note.usn,
        };
    }

    public static note ToDb(Note note)
    {
        var fieldValues = string.Join(
            FieldSeparator,
            note.FieldValues.Select(pair => pair.Value));

        var tags = Tags.ToString(note.Tags);

        return new(
            id: note.Id.Value,
            guid: note.Guid,
            mid: note.NoteTypeId.Value,
            mod: note.ModificationDateTime,
            usn: note.UpdateSequenceNumber,
            tags: tags,
            flds: fieldValues,
            sfld: note.SortField,
            csum: note.FieldChecksum,
            flags: 0,
            data: string.Empty);
    }
}
