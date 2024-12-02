using AnkiNet.CollectionFile.Model;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Model;
using System.Collections.Immutable;

namespace AnkiNet.CollectionFile.Mapper;

internal sealed class DatabaseMapper
{
    public DatabaseMapper()
    {
    }

    public DatabaseExtract ConvertModelsToDb(Collection collection)
    {
        var col = CollectionMapper.ToDb(collection);
        var cards = collection.Cards.Select(CardMapper.ToDb).ToImmutableArray();
        var graves = ImmutableArray<grave>.Empty;
        var notes = collection.Notes.Select(NoteMapper.ToDb).ToImmutableArray();
        var revLogs = ImmutableArray<revLog>.Empty;

        // TODO Add RevLog MetaData
        /*
        if (_revLogMetadatas.Count != 0)
        {
            string insertRevLog = "";
            foreach (var revlogMetadata in _revLogMetadatas)
            {
                insertRevLog = "INSERT INTO revlog VALUES(" + revlogMetadata.id + ", " + revlogMetadata.cid + ", " + revlogMetadata.usn + ", " + revlogMetadata.ease + ", " + revlogMetadata.ivl + ", " + revlogMetadata.lastIvl + ", " + revlogMetadata.factor + ", " + revlogMetadata.time + ", " + revlogMetadata.type + ")";
                SQLiteHelper.ExecuteSQLiteCommand(_conn, insertRevLog);
            }
        }
        */

        return new DatabaseExtract(
            col,
            cards,
            graves,
            notes,
            revLogs
        );
    }

    public Collection ConvertDbToModels(DatabaseExtract db)
    {
        var collection = CollectionMapper.FromDb(db.col);

        return collection with
        {
            Cards = db.cards.Select(CardMapper.FromDb).ToImmutableArray(),
            Graves = db.graves.Select(GraveMapper.FromDb).ToImmutableArray(),
            Notes = db.notes.Select(NoteMapper.FromDb).ToImmutableArray(),
            RevLogs = db.revLogs.Select(RevisionLogMapper.FromDb).ToImmutableArray()
        };
    }
}