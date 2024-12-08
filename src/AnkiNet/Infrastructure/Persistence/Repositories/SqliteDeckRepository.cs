using AnkiNet.DomainModel;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Database;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Mappers;

namespace AnkiNet.Infrastructure.Persistence.Repositories;

internal sealed class SqliteDeckRepository
    : IDeckRepository
{
    private readonly ColRepository _colRepositoryDb;
    private readonly CardRepository _cardRepositoryDb;
    private readonly NoteRepository _noteRepositoryDb;

    public SqliteDeckRepository(ColRepository colRepositoryDb, CardRepository cardRepositoryDb, NoteRepository noteRepositoryDb)
    {
        ArgumentNullException.ThrowIfNull(colRepositoryDb);

        _colRepositoryDb = colRepositoryDb;
        _cardRepositoryDb = cardRepositoryDb;
        _noteRepositoryDb = noteRepositoryDb;
    }

    public ValueTask AddAsync(Deck deck)
        => throw new NotImplementedException();

    public ValueTask<bool> ExistsAsync(DeckId deckId)
        => _colRepositoryDb.ReadAllAsync()
            .Select(CollectionMapper.DecksFromDb)
            .SelectMany(decks => decks.ToAsyncEnumerable())
            .AnyAsync(deck => deck.Id == deckId);

    public ValueTask<Deck?> GetByIdAsync(DeckId deckId)
        => GetAllAsync()
            .SingleOrDefaultAsync(deck => deck.Id == deckId);

    public async IAsyncEnumerable<Deck> GetAllAsync()
    {
        var lookupNoteTypeById = await _colRepositoryDb.ReadAllAsync()
            .Select(CollectionMapper.NoteTypesFromDb)
            .SelectMany(noteTypes => noteTypes.ToAsyncEnumerable())
            .ToLookupAsync(noteType => noteType.Id)
            .ConfigureAwait(false);

        var lookupNoteById = await _noteRepositoryDb.ReadAllAsync()
            .Select(dbNote =>
            {
                var noteTypeId = new NoteTypeId(dbNote.mid);
                var noteType = lookupNoteTypeById[noteTypeId].Single();
                return NoteMapper.FromDb(dbNote, noteType);
            })
            .ToLookupAsync(note => note.Id)
            .ConfigureAwait(false);

        // Read all cards, group by deck ID, and order the cards in the deck by their ordinal.
        var cards = await _cardRepositoryDb.ReadAllAsync()
            .Select(dbCard =>
            {
                var noteId = new NoteId(dbCard.nid);
                var note = lookupNoteById[noteId].Single();
                var noteType = lookupNoteTypeById[note.NoteTypeId].Single();
                return CardMapper.FromDb(dbCard, noteType);
            })
            .ToArrayAsync()
            .ConfigureAwait(false);

        var cardsGroupedByDeckId = cards
            .GroupBy(card => card.DeckId)
            .ToDictionary(
                grouping => grouping.Key, // Deck ID.
                grouping => grouping
                    .OrderBy(entry => entry.Card.Id)
                    .Select(entry => entry.Card)); // Cards.

        var decks = _colRepositoryDb.ReadAllAsync()
            .Select(CollectionMapper.DecksFromDb)
            .SelectMany(decks => decks.ToAsyncEnumerable())
            .Select(deck =>
            {
                deck.AddCards(cardsGroupedByDeckId[deck.Id]);
                return deck;
            });

        await foreach (var deck in decks.ConfigureAwait(false))
            yield return deck;
    }

    public ValueTask UpdateAsync(Deck deck)
        => throw new NotImplementedException();
}
