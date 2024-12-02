using AnkiNet.DomainModel.Base;

namespace AnkiNet.DomainModel;

/// <summary>
/// Specifies the layout and browserAnsertFormat for the flashcards.
/// </summary>
public sealed class CardTemplate
    : Entity<string>
{
    public static readonly string EmptyId = string.Empty;

    public static CardTemplate Create(
        string name,
        string questionFormat = "",
        string answerFormat = "",
        string browserQuestionFormat = "",
        string browserAnswerFormat = "",
        string bFont = "",
        int bSize = 20,
        long? deckOverrideId = default)
        => new (name)
        {
            QuestionFormat = questionFormat,
            AnswerFormat = answerFormat,
            BrowserQuestionFormat = browserQuestionFormat,
            BrowserAnswerFormat = browserAnswerFormat,
            BFont = bFont,
            BSize = bSize,
            DeckOverrideId = deckOverrideId,
        };

    /// <summary>
    /// Initializes a new instance of the <see cref="CardTemplate"/> record.
    /// </summary>
    /// <param name="name">Name of the card template.</param>
    public CardTemplate(string name)
        : base(id: name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }

    internal CardTemplate Clone()
        => new(name: Id)
        {
            AnswerFormat = AnswerFormat,
            QuestionFormat = QuestionFormat,
            BrowserAnswerFormat = BrowserAnswerFormat,
            BrowserQuestionFormat = BrowserQuestionFormat,
            BFont = BFont,
            BSize = BSize,
            DeckOverrideId = DeckOverrideId,
        };

    /// <summary>
    /// Name of the card template.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Deck override (null by default)
    /// </summary>
    public long? DeckOverrideId { get; set; }

    /// <summary>
    /// HTML/CSS template for the front of the card.
    /// </summary>
    public string AnswerFormat { get; set; } = string.Empty;

    /// <summary>
    /// HTML/CSS template for the back of the card.
    /// </summary>
    public string QuestionFormat { get; set; } = string.Empty;

    /// <summary>
    /// Browser answer format: used for displaying answer in browser.
    /// </summary>
    public string BrowserAnswerFormat { get; set; } = string.Empty;

    /// <summary>
    /// Browser question format: used for displaying question in browser.
    /// </summary>
    public string BrowserQuestionFormat { get; set; } = string.Empty;

    /// <summary>
    /// Undocumented.
    /// </summary>
    public string BFont { get; set; } = string.Empty;

    /// <summary>
    /// Undocumented.
    /// </summary>
    public int BSize { get; set; }
}
