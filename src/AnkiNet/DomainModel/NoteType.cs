using System.Collections.Immutable;
using AnkiNet.DomainModel.Base;

namespace AnkiNet.DomainModel;

/// <summary>
/// Contains definitions of the fields and associated card types.
/// </summary>
/// <remarks>
/// The note type is also known as a "model" in the Anki desktop application.
/// </remarks>
public sealed class NoteType
    : AggregateRoot<NoteTypeId>
{
    public static NoteType Create(
        NoteTypeId id,
        string name,
        IEnumerable<Field> fields,
        IEnumerable<CardTemplate> cardTemplates)
        => new(id, name, fields, cardTemplates)
        {
            IsDirty = true,
        };

    /// <summary>
    /// Creates a new instance of the <see cref="NoteType"/> class.
    /// </summary>
    /// <param name="id">Unique identifier for the note type.</param>
    /// <param name="name">Name of the note type (e.g., "Basic", "Cloze").</param>
    /// <param name="fields">List of <see cref="Field"/>s included in this note type.</param>
    /// <param name="cardTemplates">List of associated card templates.</param>
    public NoteType(NoteTypeId id, string name, IEnumerable<Field> fields, IEnumerable<CardTemplate> cardTemplates)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(cardTemplates);

        Name = name;
        LatexPost = "\\end{ document }";
        LatexPre = "\\documentclass[12pt]{article}\n\\special{papersize=3in,5in}\n\\usepackage[utf8]{inputenc}\n\\usepackage{amssymb,amsmath}\n\\pagestyle{empty}\n\\setlength{\\parindent}{0in}\n\\begin{document}\n";

        Fields = [];
        AddFields(fields);

        CardTemplates = [];
        AddCardTemplates(cardTemplates);
    }

    internal NoteType Clone()
    {
        var fields = Fields.Select(field => field.Clone());
        var cardTemplates = CardTemplates.Select(cardTemplate => cardTemplate.Clone());

        return new(Id, Name, fields, cardTemplates)
        {
            Styling = Styling,
            ModificationTime = ModificationTime,
            DefaultDeckId = DefaultDeckId,
            ModelType = ModelType,
            UpdateSequenceNumber = UpdateSequenceNumber,
            LatexPost = LatexPost,
            LatexPre = LatexPre,
            LatexSvg = LatexSvg,
            BrowserSortField = BrowserSortField,
            LastAddedNoteTags = LastAddedNoteTags,
            IsDirty = IsDirty,
        };
    }

    /// <summary>
    /// Name of the note type (e.g., "Basic", "Cloze").
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// List of <see cref="Field"/>s included in this note type.
    /// </summary>
    public ImmutableArray<Field> Fields { get; private set; }

    /// <summary>
    /// List of associated card templates.
    /// </summary>
    public ImmutableArray<CardTemplate> CardTemplates { get; private set; }

    public void AddCardTemplates(IEnumerable<CardTemplate> cardTemplates)
    {
        ArgumentNullException.ThrowIfNull(cardTemplates);
        foreach (var cardTemplate in cardTemplates)
            AddCardTemplate(cardTemplate);
    }

    public CardTemplate AddCardTemplate(CardTemplate cardTemplate)
    {
        ArgumentNullException.ThrowIfNull(cardTemplate);

        ThrowIfCardTemplateIsInvalid(cardTemplate);

        CardTemplates = CardTemplates.Add(cardTemplate);
        IsDirty = true;
        return cardTemplate;
    }

    public void AddFields(IEnumerable<Field> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        foreach (var field in fields)
            AddField(field);
    }

    public Field AddField(Field field)
    {
        ThrowIfFieldIsInvalid(field);

        Fields = Fields.Add(field);
        IsDirty = true;
        return field;
    }

    private void ThrowIfFieldIsInvalid(Field field)
    {
        if (Fields.Any(f => f.Name == field.Name))
            throw new InvalidOperationException("A field with the same name already exists.");
    }

    private void ThrowIfCardTemplateIsInvalid(CardTemplate cardTemplate)
    {
        if (CardTemplates.Any(ct => ct.Id == cardTemplate.Id))
            throw new InvalidOperationException("A card template with the same ID already exists.");

        if (CardTemplates.Any(ct => ct.Name == cardTemplate.Name))
            throw new InvalidOperationException("A card template with the same name already exists.");
    }

    /// <summary>
    /// CSS, shared for all templates.
    /// </summary>
    public string Styling { get => field; set => SetPropertyValue(ref field, value); } = string.Empty;

    /// <summary>
    /// Modification time in seconds.
    /// </summary>
    public long ModificationTime { get => field; set => SetPropertyValue(ref field, value); } // TODO Use DateTime?

    /// <summary>
    /// The id of the deck that cards are added to by default.
    /// </summary>
    public DeckId DefaultDeckId { get => field; set => SetPropertyValue(ref field, value); }

    /// <summary>
    /// The type of model.
    /// </summary>
    public ModelType ModelType { get => field; set => SetPropertyValue(ref field, value); }

    /// <summary>
    /// Update sequence number: used in same way as other usn vales in db.
    /// </summary>
    public long UpdateSequenceNumber { get => field; set => SetPropertyValue(ref field, value); }

    /// <summary>
    /// String added to end of LaTeX expressions (usually \\end{document}).
    /// </summary>
    public string LatexPost { get => field; set => SetPropertyValue(ref field, value); } = string.Empty;

    /// <summary>
    /// Preamble string for LaTeX expressions.
    /// </summary>
    public string LatexPre { get => field; set => SetPropertyValue(ref field, value); } = string.Empty;

    /// <summary>
    /// Undocumented.
    /// </summary>
    public bool LatexSvg { get => field; set => SetPropertyValue(ref field, value); }

    /// <summary>
    /// Integer specifying which field is used for sorting in the browser.
    /// </summary>
    public int BrowserSortField { get => field; set => SetPropertyValue(ref field, value); }

    /// <summary>
    /// Anki saves the tags of the last added note to the current model, use an empty array [].
    /// </summary>
    public ImmutableArray<Tag> LastAddedNoteTags { get => field; set => SetPropertyValue(ref field, value); } = [];
}
