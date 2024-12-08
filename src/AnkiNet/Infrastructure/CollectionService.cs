using AnkiNet.DomainModel;
using AnkiNet.Infrastructure.Persistence;
using AnkiNet.Infrastructure.Persistence.AnkiFile;

namespace AnkiNet.Infrastructure;

internal sealed class CollectionService
    : ICollectionService
{
    private readonly Func<Task>? _disposeAction;

    public CollectionService(
        IDeckRepository deckRepository,
        INoteRepository noteRepository,
        INoteTypeRepository noteTypeRepository,
        ICollectionRepository collectionRepository,
        Func<Task>? disposeAction = null
    )
    {
        ArgumentNullException.ThrowIfNull(deckRepository);
        ArgumentNullException.ThrowIfNull(noteRepository);
        ArgumentNullException.ThrowIfNull(noteTypeRepository);
        ArgumentNullException.ThrowIfNull(collectionRepository);

        DeckRepository = deckRepository;
        NoteRepository = noteRepository;
        NoteTypeRepository = noteTypeRepository;
        CollectionRepository = collectionRepository;

        _disposeAction = disposeAction;
        UnitOfWork = new UnitOfWork(DeckRepository, NoteRepository, NoteTypeRepository, CollectionRepository);
    }

    public async ValueTask DisposeAsync()
    {
        await UnitOfWork.DisposeAsync().ConfigureAwait(false);

        if (_disposeAction is not null)
            await _disposeAction().ConfigureAwait(false);
    }

    private IUnitOfWork UnitOfWork { get; }

    private IDeckRepository DeckRepository { get; }

    private INoteRepository NoteRepository { get; }

    private INoteTypeRepository NoteTypeRepository { get; }

    private ICollectionRepository CollectionRepository { get; }

    public async Task CommitAsync()
    {
        await UnitOfWork.CommitAsync().ConfigureAwait(false);
    }

    public async Task LoadAnkiFileAsync(Stream ankiFileStream)
    {
        var ankiFileAccess = new AnkiFileAccess(
            DeckRepository,
            NoteRepository,
            NoteTypeRepository,
            CollectionRepository);

        await ankiFileAccess.LoadAsync(ankiFileStream).ConfigureAwait(false);
    }

    public async Task LoadAnkiFileAsync(string ankiFile)
    {
        var ankiFileAccess = new AnkiFileAccess(
            DeckRepository,
            NoteRepository,
            NoteTypeRepository,
            CollectionRepository);

        await ankiFileAccess.LoadAsync(ankiFile).ConfigureAwait(false);
    }

    public async Task SaveAnkiFileAsync(Stream ankiFileStream)
    {
        var ankiFileAccess = new AnkiFileAccess(
            DeckRepository,
            NoteRepository,
            NoteTypeRepository,
            CollectionRepository);

        await ankiFileAccess.SaveAsync(ankiFileStream).ConfigureAwait(false);
    }

    public async Task SaveAnkiFileAsync(string ankiFile)
    {
        var ankiFileAccess = new AnkiFileAccess(
            DeckRepository,
            NoteRepository,
            NoteTypeRepository,
            CollectionRepository);

        await ankiFileAccess.SaveAsync(ankiFile).ConfigureAwait(false);
    }

    private async Task<CollectionId> NewCollectionIdAsync()
    {
        var idValue = await IdFactory
            .CreateAsync(id => CollectionRepository.ExistsAsync(new CollectionId(id)).AsTask())
            .ConfigureAwait(false);

        return new(idValue);
    }

    public async Task<Collection> CreateCollectionAsync()
    {
        var id = await NewCollectionIdAsync().ConfigureAwait(false);
        var collection = Collection.Create(
            id,
            Configurations.Default,
            noteTypes: [],
            decks: [],
            notes: [],
            graves: []);

        await CollectionRepository.AddAsync(collection).ConfigureAwait(false);
        UnitOfWork.Register(collection);

        var defaultDeck = Deck.Create(
            Configurations.DefaultDeckId, DeckConfigurations.DefaultDeckName, DeckConfigurations.Default, cards: []);

        collection.AddDeck(defaultDeck);

        await DeckRepository.AddAsync(defaultDeck).ConfigureAwait(false);
        UnitOfWork.Register(defaultDeck);

        return collection;
    }

    public async Task<Collection?> GetCollectionByIdAsync(CollectionId collectionId)
    {
        if (UnitOfWork.TryGetEntity(collectionId, out Collection? collection))
            return collection;

        collection = await CollectionRepository.GetByIdAsync(collectionId).ConfigureAwait(false);
        if (collection is not null)
            UnitOfWork.Register(collection);

        return collection;
    }

    public async Task<Deck> CreateDeckAsync(CollectionId collectionId, string name, DeckConfiguration? deckConfiguration)
    {
        ArgumentNullException.ThrowIfNull(name);

        var collection = await GetCollectionByIdAsync(collectionId).ConfigureAwait(false)
            ?? throw new ArgumentException("The collection does not exist.", nameof(collectionId));

        var id = await NewDeckIdAsync().ConfigureAwait(false);
        var deck = Deck.Create(id, name, deckConfiguration, cards: []);

        collection.AddDeck(deck);

        await DeckRepository.AddAsync(deck).ConfigureAwait(false);
        UnitOfWork.Register(deck);

        return deck;
    }

    private async Task<DeckId> NewDeckIdAsync()
    {
        var idValue = await IdFactory
            .CreateAsync(id => DeckRepository.ExistsAsync(new DeckId(id)).AsTask())
            .ConfigureAwait(false);

        return new(idValue);
    }

    public async Task<Deck?> GetDeckByIdAsync(DeckId deckId)
    {
        if (UnitOfWork.TryGetEntity(deckId, out Deck? deck))
            return deck;

        deck = await DeckRepository.GetByIdAsync(deckId).ConfigureAwait(false);
        if (deck is not null)
            UnitOfWork.Register(deck);

        return deck;
    }

    public async Task<Note> CreateNoteAsync(
        CollectionId collectionId,
        NoteTypeId noteTypeId,
        IEnumerable<KeyValuePair<string, string>> fieldValues,
        IEnumerable<Tag> tags)
    {
        ArgumentNullException.ThrowIfNull(fieldValues);
        ArgumentNullException.ThrowIfNull(tags);

        var collection = await GetCollectionByIdAsync(collectionId).ConfigureAwait(false)
            ?? throw new ArgumentException("The collection does not exist", nameof(collectionId));

        var noteType = await GetNoteTypeByIdAsync(noteTypeId).ConfigureAwait(false)
            ?? throw new ArgumentException("The deck type does not exist", nameof(noteTypeId));

        var id = await NewNoteIdAsync().ConfigureAwait(false);
        var note = Note.Create(id, noteType, fieldValues, tags);

        collection.AddNote(note);

        await NoteRepository.AddAsync(note).ConfigureAwait(false);
        UnitOfWork.Register(note);

        return note;
    }

    private async Task<NoteId> NewNoteIdAsync()
    {
        var idValue = await IdFactory
            .CreateAsync(id => NoteRepository.ExistsAsync(new NoteId(id)).AsTask())
            .ConfigureAwait(false);

        return new(idValue);
    }

    public async Task<Note?> GetNoteByIdAsync(NoteId noteId)
    {
        if (UnitOfWork.TryGetEntity(noteId, out Note? note))
            return note;

        note = await NoteRepository.GetByIdAsync(noteId).ConfigureAwait(false);
        if (note is not null)
            UnitOfWork.Register(note);

        return note;
    }

    public async Task<NoteType> CreateNoteTypeAsync(
        CollectionId collectionId,
        string name,
        IEnumerable<Field> fields,
        IEnumerable<CardTemplate> cardTemplates)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(cardTemplates);

        var collection = await GetCollectionByIdAsync(collectionId).ConfigureAwait(false)
            ?? throw new ArgumentException("The collection does not exist.", nameof(collectionId));

        var id = await NewNoteTypeIdAsync().ConfigureAwait(false);
        var noteType = NoteType.Create(id, name, fields, cardTemplates);

        collection.AddNoteType(noteType);

        await NoteTypeRepository.AddAsync(noteType).ConfigureAwait(false);
        UnitOfWork.Register(noteType);

        return noteType;
    }

    private async Task<NoteTypeId> NewNoteTypeIdAsync()
    {
        var idValue = await IdFactory
            .CreateAsync(id => NoteTypeRepository.ExistsAsync(new NoteTypeId(id)).AsTask())
            .ConfigureAwait(false);

        return new(idValue);
    }

    public async Task<NoteType?> GetNoteTypeByIdAsync(NoteTypeId noteTypeId)
    {
        if (UnitOfWork.TryGetEntity(noteTypeId, out NoteType? noteType))
            return noteType;

        noteType = await NoteTypeRepository.GetByIdAsync(noteTypeId).ConfigureAwait(false);
        if (noteType is not null)
            UnitOfWork.Register(noteType);
        
        return noteType;
    }

    public async IAsyncEnumerable<Collection> GetCollectionsAsync()
    {
        await foreach (var collection in CollectionRepository.GetAllAsync().ConfigureAwait(false))
        {
            if (UnitOfWork.TryGetEntity(collection.Id, out Collection? registeredCollection))
            {
                yield return registeredCollection;
            }
            else
            {
                UnitOfWork.Register(collection);
                yield return collection;
            }
        }
    }

    public async IAsyncEnumerable<Deck> GetDecksAsync()
    {
        await foreach (var deck in DeckRepository.GetAllAsync().ConfigureAwait(false))
        {
            if (UnitOfWork.TryGetEntity(deck.Id, out Deck? registeredDeck))
            {
                yield return registeredDeck;
            }
            else
            {
                UnitOfWork.Register(deck);
                yield return deck;
            }
        }
    }

    public async IAsyncEnumerable<Note> GetNotesAsync()
    {
        await foreach (var note in NoteRepository.GetAllAsync().ConfigureAwait(false))
        {
            if (UnitOfWork.TryGetEntity(note.Id, out Note? registeredNote))
            {
                yield return registeredNote;
            }
            else
            {
                UnitOfWork.Register(note);
                yield return note;
            }
        }
    }

    public async IAsyncEnumerable<NoteType> GetNoteTypesAsync()
    {
        await foreach (var noteType in NoteTypeRepository.GetAllAsync().ConfigureAwait(false))
        {
            if (UnitOfWork.TryGetEntity(noteType.Id, out NoteType? registeredNoteType))
            {
                yield return registeredNoteType;
            }
            else
            {
                UnitOfWork.Register(noteType);
                yield return noteType;
            }
        }
    }
}
