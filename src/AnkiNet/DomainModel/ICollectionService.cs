namespace AnkiNet.DomainModel;

public interface ICollectionService
    : IAsyncDisposable
{
    Task CommitAsync();

    Task<Collection> CreateCollectionAsync();

    Task<Deck> CreateDeckAsync(
        CollectionId collectionId, string name, DeckConfiguration? deckConfiguration = default);
    
    Task<Note> CreateNoteAsync(
        CollectionId collectionId,
        NoteTypeId noteTypeId,
        IEnumerable<KeyValuePair<string, string>> fieldValues,
        IEnumerable<Tag> tags);
    
    Task<NoteType> CreateNoteTypeAsync(
        CollectionId collectionId, string name, IEnumerable<Field> fields, IEnumerable<CardTemplate> cardTemplates);
    
    Task<Collection?> GetCollectionByIdAsync(CollectionId collectionId);

    IAsyncEnumerable<Collection> GetCollectionsAsync();

    Task<Deck?> GetDeckByIdAsync(DeckId deckId);

    IAsyncEnumerable<Deck> GetDecksAsync();

    Task<Note?> GetNoteByIdAsync(NoteId noteId);

    IAsyncEnumerable<Note> GetNotesAsync();

    Task<NoteType?> GetNoteTypeByIdAsync(NoteTypeId noteTypeId);

    IAsyncEnumerable<NoteType> GetNoteTypesAsync();

    Task LoadAnkiFileAsync(Stream ankiFileStream);

    Task LoadAnkiFileAsync(string ankiFile);
    
    Task SaveAnkiFileAsync(Stream ankiFileStream);

    Task SaveAnkiFileAsync(string ankiFile);
}
