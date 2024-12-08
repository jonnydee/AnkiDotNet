using AnkiNet.DomainModel;

namespace AnkiNet.Infrastructure.Persistence.Repositories;

public sealed class InMemoryDeckRepository
    : IDeckRepository
{
    private readonly Dictionary<DeckId, Deck> _decks = [];

    public ValueTask AddAsync(Deck deck)
    {
        if (_decks.ContainsKey(deck.Id))
            throw new InvalidOperationException($"Deck with ID {deck.Id} already exists.");

        if (deck.IsDirty is false)
            throw new InvalidOperationException("Deck is not dirty.");

        deck.IsDirty = false;
        deck = deck.Clone();
        _decks.Add(deck.Id, deck);
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> ExistsAsync(DeckId deckId)
        => ValueTask.FromResult(_decks.ContainsKey(deckId));

    public ValueTask<Deck?> GetByIdAsync(DeckId deckId)
        => ValueTask.FromResult(_decks.TryGetValue(deckId, out var deck)
            ? deck.Clone()
            : null);

    public IAsyncEnumerable<Deck> GetAllAsync()
        => _decks.Values
            .Select(deck => deck.Clone())
            .ToAsyncEnumerable();

    public ValueTask UpdateAsync(Deck deck)
    {
        if (!_decks.ContainsKey(deck.Id))
            throw new InvalidOperationException($"Deck with ID {deck.Id} does not exist.");

        if (deck.IsDirty is false)
            return ValueTask.CompletedTask;

        deck.IsDirty = false;
        _decks[deck.Id] = deck.Clone();
        return ValueTask.CompletedTask;
    }
}
