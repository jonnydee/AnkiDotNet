using AnkiNet.CollectionFile.Model;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Model;

namespace AnkiNet.CollectionFile.Mapper;

internal static class NoteMapper
{
    private const char FieldSeparator = '\u001f';

    public static Note FromDb(note note)
    {
        return new Note(
            Id: note.id,
            Guid: note.guid,
            ModelId: note.mid,
            ModificationDateTime: note.mod,
            UpdateSequenceNumber: note.usn,
            Tags: note.tags,
            Fields: note.flds.Split(FieldSeparator),
            SortField: note.sfld,
            FieldChecksum: note.csum,
            Flags: note.flags,
            Data: note.data
        );
    }

    public static note ToDb(Note note)
    {
        return new note(
            id: note.Id,
            guid: note.Guid,
            mid: note.ModelId,
            mod: note.ModificationDateTime,
            usn: note.UpdateSequenceNumber,
            tags: note.Tags,
            flds: string.Join(FieldSeparator, note.Fields),
            sfld: note.SortField,
            csum: note.FieldChecksum,
            flags: note.Flags,
            data: note.Data
        );
    }
}