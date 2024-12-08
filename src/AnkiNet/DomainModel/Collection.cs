using AnkiNet.DomainModel.Base;
using System.Collections.Immutable;

namespace AnkiNet.DomainModel;

public sealed class Collection
    : AggregateRoot<CollectionId>
{
    public static Collection Create(
        CollectionId id,
        Configuration configuration,
        IEnumerable<NoteType> noteTypes,
        IEnumerable<Deck> decks,
        IEnumerable<Note> notes,
        IEnumerable<Grave> graves)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(noteTypes);
        ArgumentNullException.ThrowIfNull(decks);
        ArgumentNullException.ThrowIfNull(notes);
        ArgumentNullException.ThrowIfNull(graves);

        return new Collection(
            id: id,
            configuration: configuration,
            noteTypes: noteTypes.Select(nt => nt.Id),
            decks: decks.Select(d => d.Id),
            notes: notes.Select(n => n.Id),
            graves: graves)
        {
            IsDirty = true,
            Version = 11, // See https://github.com/ankitects/anki/blob/main/rslib/src/storage/upgrades/mod.rs (should it be 18?)
        };
    }

    public Collection(
        CollectionId id,
        Configuration configuration,
        IEnumerable<NoteTypeId> noteTypes,
        IEnumerable<DeckId> decks,
        IEnumerable<NoteId> notes,
        IEnumerable<Grave> graves)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(noteTypes);
        ArgumentNullException.ThrowIfNull(decks);
        ArgumentNullException.ThrowIfNull(notes);
        ArgumentNullException.ThrowIfNull(graves);

        Configuration = configuration;
        NoteTypes = noteTypes.ToImmutableSortedSet();
        Decks = decks.ToImmutableSortedSet();
        Notes = notes.ToImmutableSortedSet();
        Graves = graves.ToImmutableList();
    }

    internal Collection Clone()
        => new(Id, Configuration, NoteTypes, Decks, Notes, Graves)
        {
            CreationDateTime = CreationDateTime,
            LastModifiedDateTime = LastModifiedDateTime,
            SchemaModificationDateTime = SchemaModificationDateTime,
            Version = Version,
            UpdateSequenceNumber = UpdateSequenceNumber,
            LastSyncDateTime = LastSyncDateTime,
            Configuration = Configuration,
            IsDirty = IsDirty,
        };

    public void AddDeck(Deck deck)
    {
        if (Decks.Contains(deck.Id))
            return;

        Decks = Decks.Add(deck.Id);
        IsDirty = true;
    }

    public void RemoveDeck(DeckId id)
    {
        Decks = Decks.Remove(id);
        IsDirty = true;
    }

    public void AddNoteType(NoteType noteType)
    {
        if (NoteTypes.Contains(noteType.Id))
            return;

        NoteTypes = NoteTypes.Add(noteType.Id);
        Configuration = Configuration with { CurrentNoteType = noteType.Id };
        IsDirty = true;
    }

    public void RemoveNoteType(NoteTypeId id)
    {
        if (!NoteTypes.Contains(id))
            return;

        NoteTypes = NoteTypes.Remove(id);
        
        if (Configuration.CurrentNoteType == id)
        {
            var lastNoteType = NoteTypes.LastOrDefault(NoteTypeId.Empty);
            Configuration = Configuration with { CurrentNoteType = lastNoteType };
        }

        IsDirty = true;
    }

    public void AddNote(Note note)
    {
        if (Notes.Contains(note.Id))
            return;

        Notes = Notes.Add(note.Id);
        IsDirty = true;
    }

    public void RemoveNote(NoteId id)
    {
        if (!Notes.Contains(id))
            return;

        Notes = Notes.Remove(id);
        IsDirty = true;
    }

    public void AddGrave(Grave grave)
    {
        ArgumentNullException.ThrowIfNull(grave);

        Graves = Graves.Add(grave);
        IsDirty = true;
    }

    public IImmutableSet<NoteId> Notes { get; private set; }

    public IImmutableSet<DeckId> Decks { get; private set; }

    public IImmutableSet<NoteTypeId> NoteTypes { get; private set; }

    public IImmutableList<Grave> Graves { get; private set; }

    public long CreationDateTime { get => field; set => SetPropertyValue(ref field, value); }

    public long LastModifiedDateTime { get => field; set => SetPropertyValue(ref field, value); }

    public long SchemaModificationDateTime { get => field; set => SetPropertyValue(ref field, value); }

    public long Version { get => field; set => SetPropertyValue(ref field, value); }

    public long UpdateSequenceNumber { get => field; set => SetPropertyValue(ref field, value); }

    public long LastSyncDateTime { get => field; set => SetPropertyValue(ref field, value); }

    public Configuration Configuration { get => field; set => SetPropertyValue(ref field, value); } = new();
}
