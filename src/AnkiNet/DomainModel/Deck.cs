using System.Collections.Immutable;
using AnkiNet.DomainModel.Base;

namespace AnkiNet.DomainModel;

/// <summary>
/// Organizes a collection of notes for study purposes.
/// </summary>
public sealed class Deck
    : AggregateRoot<DeckId>
{
    public static Deck Create(
        DeckId id,
        string name,
        DeckConfiguration? deckConfiguration,
        IEnumerable<Card> cards)
        => new(id, name, deckConfiguration ?? new(), cards)
        {
            IsDirty = true,
            ConfigurationGroupId = 1, // TODO What is this?
        };

    /// <summary>
    /// Creates a new deck.
    /// </summary>
    /// <param name="id">Unique identifier for the deck.</param>
    /// <param name="name">Name of the deck (e.g., "Biology 101", "German Vocabulary").</param>
    /// <param name="deckConfiguration">The configuration for this deck.</param>
    public Deck(DeckId id, string name, DeckConfiguration deckConfiguration, IEnumerable<Card> cards)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(deckConfiguration);
        ArgumentNullException.ThrowIfNull(cards);

        Name = name;
        DeckConfiguration = deckConfiguration;

        Cards = [];
        AddCards(cards);
    }

    internal Deck Clone()
        => new(Id, Name, DeckConfiguration, Cards)
        {
            LastModificationTime = LastModificationTime,
            UpdateSequenceNumber = UpdateSequenceNumber,
            NewToday = NewToday,
            ReviewedToday = ReviewedToday,
            LearnedToday = LearnedToday,
            TimeToday = TimeToday,
            IsCollapsed = IsCollapsed,
            IsCollapsedInBrowser = IsCollapsedInBrowser,
            Description = Description,
            IsDynamic = IsDynamic,
            ConfigurationGroupId = ConfigurationGroupId,
            ExtendedNewCardLimit = ExtendedNewCardLimit,
            ExtendedReviewCardLimit = ExtendedReviewCardLimit,
            IsDirty = IsDirty,
        };

    public ImmutableArray<Card> Cards { get; private set; }

    /// <summary>
    /// Name of the deck (e.g., "Biology 101", "German Vocabulary").
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The configuration for this deck.
    /// </summary>
    public DeckConfiguration DeckConfiguration { get => field; set => SetPropertyValue(ref field, value); }

    public Deck AddCards(IEnumerable<Card> cards)
    {
        ArgumentNullException.ThrowIfNull(cards);

        foreach (var card in cards)
            AddCard(card);

        return this;
    }

    public Card AddCard(Card card)
    {
        ArgumentNullException.ThrowIfNull(card);

        ThrowIfCardWithIdAlreadyExists(card);

        Cards = Cards.Add(card);
        IsDirty = true;
        return card;
    }

    public Card? GetCard(long cardId)
        => Cards.FirstOrDefault(card => card.Id == cardId);

    public bool RemoveCard(long cardId)
    {
        if (GetCard(cardId) is not { } card)
            return false;

        Cards = Cards.Remove(card);
        IsDirty = true;
        return true;
    }

    public void AddRevisionLogToCard(Card card, RevisionLog revisionLog)
    {
        ArgumentNullException.ThrowIfNull(card);
        ArgumentNullException.ThrowIfNull(revisionLog);

        ThrowIfCardIsNotFromThisDeck(card);
        ThrowIfRevisionLogWithIdAlreadyExists(revisionLog);

        card.AddRevisionLog(revisionLog);
        IsDirty = true;
    }

    public void AddCardsForNote(Note note, NoteType noteType)
    {
        ArgumentNullException.ThrowIfNull(note);
        ArgumentNullException.ThrowIfNull(noteType);

        if (note.NoteTypeId != noteType.Id)
            throw new ArgumentException(message: "'Note' and 'NoteType' not related");

        foreach (var template in noteType.CardTemplates)
        {
            var id = IdFactory.Create(idExists: id => Cards.Any(card => card.Id == id));
            var card = new Card(
                id: id,
                noteId: note.Id,
                cardTemplateId: template.Id,
                revisionLogs: []);

            Cards = Cards.Add(card);
        }

        IsDirty = true;
    }

    private void ThrowIfCardWithIdAlreadyExists(Card card)
    {
        // Card id must be unique.
        if (Cards.Any(c => c.Id == card.Id))
            throw new InvalidOperationException("Card id must be unique.");
    }

    private void ThrowIfCardIsNotFromThisDeck(Card card)
    {
        // Card must be from this deck.
        if (!Cards.Contains(card))
            throw new InvalidOperationException("Card must be from this deck.");
    }

    private void ThrowIfRevisionLogWithIdAlreadyExists(RevisionLog revisionLog)
    {
        // Revision log id must be unique in the deck.
        if (Cards.Any(card => card.RevisionLogs.Any(rl => rl.Id == revisionLog.Id)))
            throw new InvalidOperationException("Revision log id must be unique.");
    }

    public IEnumerable<Card> FindCardsForNote(Note note)
    {
        ArgumentNullException.ThrowIfNull(note);

        return Cards.Where(c => c.NoteId == note.Id);
    }

    /// <summary>
    /// Last modification time.
    /// </summary>
    public long LastModificationTime { get => field; set => SetPropertyValue(ref field, value); } // TODO Use DateTime?

    /// <summary>
    /// Update sequence number.
    /// </summary>
    public long UpdateSequenceNumber { get => field; set => SetPropertyValue(ref field, value); }

    /// <summary>
    /// First one is the number of days that have passed between the collection was created and the deck was last updated.
    /// The second one is equal to the number of cards seen today in this deck minus the number of new cards in custom study today.
    /// </summary>
    public (int DaysSinceCreation, int CardsSeenToday) NewToday { get => field; set => SetPropertyValue(ref field, value); }

    /// <summary>
    /// First one is the number of days that have passed between the collection was created and the deck was last updated.
    /// The second one is equal to the number of cards seen today in this deck minus the number of new cards in custom study today.
    /// </summary>
    public (int DaysSinceCreation, int CardsSeenToday) ReviewedToday { get => field; set => SetPropertyValue(ref field, value); }

    /// <summary>
    /// Two number array.
    /// First one is the number of days that have passed between the collection was created and the deck was last updated.
    /// The second one is equal to the number of cards seen today in this deck minus the number of new cards in custom study today.
    /// </summary>
    public (int DaysSinceCreation, int CardsSeenToday) LearnedToday { get => field; set => SetPropertyValue(ref field, value); }

    /// <summary>
    /// Two numbers used somehow for custom study. Currently unused in the code.
    /// </summary>
    public (int Value1, int Value2) TimeToday { get => field; set => SetPropertyValue(ref field, value); }

    /// <summary>
    /// True when deck is collapsed.
    /// </summary>
    public bool IsCollapsed { get => field; set => SetPropertyValue(ref field, value); }

    /// <summary>
    /// True when deck collapsed in browser.
    /// </summary>
    public bool IsCollapsedInBrowser { get => field; set => SetPropertyValue(ref field, value); }

    /// <summary>
    /// Deck's description.
    /// </summary>
    public string Description { get => field; set => SetPropertyValue(ref field, value); } = string.Empty;

    /// <summary>
    /// Indicates if the deck is dynamic (aka. filtered).
    /// </summary>
    public bool IsDynamic { get => field; set => SetPropertyValue(ref field, value); }

    /// <summary>
    /// Id of option group from 'dconf' column in [col]table.
    /// Or absent if the deck is dynamic (aka. filtered).
    /// </summary>
    public long? ConfigurationGroupId { get => field; set => SetPropertyValue(ref field, value); }

    /// <summary>
    /// Extended new card limit (for custom study).
    /// Potentially absent, in this case it's considered to be 10, by aqt.customstudy.
    /// </summary>
    public int ExtendedNewCardLimit { get => field; set => SetPropertyValue(ref field, value); } //= 10;

    /// <summary>
    /// Extended review card limit (for custom study).
    /// Potentially absent, in this case it's considered to be 10, by aqt.customstudy.
    /// </summary>
    public int ExtendedReviewCardLimit { get => field; set => SetPropertyValue(ref field, value); } //= 10;
}
