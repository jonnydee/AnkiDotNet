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
    
    IAsyncEnumerable<Deck> GetDecksAsync();
    
    IAsyncEnumerable<Note> GetNotesAsync();
    
    IAsyncEnumerable<NoteType> GetNoteTypesAsync();
    
    Task LoadAnkiFileAsync(string ankiFile);
    
    Task SaveAnkiFileAsync(string ankiFile);
}
