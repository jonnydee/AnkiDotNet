using AnkiNet.CollectionFile.Model;
using AnkiNet.CollectionFile.Model.Json;
using System.Collections.Immutable;

namespace AnkiNet.CollectionFile;

internal sealed class InternalConverter
{
    public Collection ConvertAnkiCollectionToCollection(AnkiCollection collection)
    {
        var result = new Collection(
            Id: 1, // Arbitrary
            CreationDateTime: 0,
            LastModifiedDateTime: 0,
            SchemaModificationDateTime: 0,
            Version: 11, // See https://github.com/ankitects/anki/blob/main/rslib/src/storage/upgrades/mod.rs (should it be 18?)
            Dirty: 0,
            UpdateSequenceNumber: 0,
            LastSyncDateTime: 0,
            Configuration: new JsonConfiguration
            {
                SortBackwards = false,
                CurrentDeck = 1,
                DueCounts = true,
                SortType = "noteFld",
                CurrentModel = collection.NoteTypes.First().Id,
                TimeLimit = 0,
                NewSpread = 0,
                CollapseTime = 1200,
                EstimateTimes = true,
                AddToCurrent = true,
                NextPosition = 1,
                DayLearnFirst = false,
                SchedulerVersion = 2,
                CreationOffset = -480,
                ActiveDecks = [1],
                NewBury = false,
                LastUnburied = 0,
            },
            Models: collection.NoteTypes.ToDictionary(
                nt => nt.Id,
                nt => new JsonModel
                {
                    Id = nt.Id,
                    Name = nt.Name,
                    ModificationTime = 0,
                    Css = nt.Css ?? string.Empty,
                    DefaultDeckId = null,
                    ModelType = 0,
                    UpdateSequenceNumber = 0,
                    LegacyVersionNumber = [],
                    LatexPost = "\\end{ document }",
                    LatexPre = "\\documentclass[12pt]{article}\n\\special{papersize=3in,5in}\n\\usepackage[utf8]{inputenc}\n\\usepackage{amssymb,amsmath}\n\\pagestyle{empty}\n\\setlength{\\parindent}{0in}\n\\begin{document}\n",
                    LatexSvg = false,
                    BrowserSortField = 0,
                    LastAddedNoteTags = [],
                    CardTemplates = nt.CardTypes.Select(ct => new JsonCardTemplate
                    {
                        TemplateName = ct.Name,
                        TemplateOrdinal = ct.Ordinal,
                        DeckOverrideId = null,
                        AnswerFormat = ct.AnswerFormat,
                        BrowserAnswerFormat = string.Empty,
                        QuestionFormat = ct.QuestionFormat,
                        BrowserQuestionFormat = string.Empty,
                        BFont = string.Empty,
                        BSize = 0
                    }).ToArray(),
                    Fields = nt.FieldNames.Select((field, index) => new JsonField
                    {
                        FieldName = field,
                        FieldNumber = index,
                        IsRightToLeft = false,
                        IsSticky = false,
                        Font = "Arial", // TODO Make this customizable?
                        FontSize = 20, // TODO Make this customizable?
                        Description = string.Empty,
                        Media = null
                    }).ToArray(),
                    RequiredFields =
                    [
                        0,
                        "any",
                        new object[] {0}
                    ]
                }
            ),
            Decks: collection.Decks.ToDictionary(
                d => d.Id,
                d => new JsonDeck
                {
                    Id = d.Id,
                    LastModificationTime = 0,
                    Name = d.Name,
                    UpdateSequenceNumber = 0,
                    NewToday = [0, 0],
                    ReviewedToday = [0, 0],
                    LearnedToday = [0, 0],
                    TimeToday = [0, 0],
                    IsCollapsed = false,
                    IsCollapsedInBrowser = false,
                    Description = string.Empty, // TODO Handle deck description?
                    IsDynamic = 0,
                    ConfigurationGroupId = 1,
                    ExtendedNewCardLimit = 0,
                    ExtendedReviewCardLimit = 0,
                }
            ),
            DecksConfiguration: new Dictionary<long, JsonDeckConfguration>
            {
                [AnkiCollection.DefaultDeckId] = new JsonDeckConfguration
                {
                    Id = 1,
                    LastModificationTime = 0,
                    Name = AnkiCollection.DefaultDeckName,
                    UpdateSequenceNumber = 0,
                    AutoplayQuestionAudio = true,
                    ReplayQuestionAudio = true,
                    ShowTimer = 0,
                    IsDynamic = false,
                    StopTimerAfterSeconds = 0,
                    LapseCardsConfiguration = new JsonLapseCardsConfiguration
                    {
                        Delays = [10f],
                        LapsedIntervalMultiplierPercent = 0,
                        LeechAction = 1,
                        LeechFailsAllowedCount = 8,
                        MinimumInterfalAfterLeech = 1
                    },
                    NewCardsConfiguration = new JsonNewCardsConfiguration
                    {
                        Bury = false,
                        Delays = [1f, 10f],
                        InitialEaseFactor = 2500,
                        IntDelays = [1, 4, 0],
                        NewCardsPerDay = 20,
                        NewCardsShowOrder = 1,
                        Separate = 0
                    },
                    ReviewCardsConfiguration = new JsonReviewCardsConfiguration
                    {
                        Bury = false,
                        CardsToReviewPerDay = 200,
                        Ease4 = 1.3f,
                        Fuzz = 0,
                        HardFactor = 1.2f,
                        IntervalMultiplicationFactor = 1,
                        MaximumReviewInterval = 36500,
                        MinSpace = 0
                    }
                }
            },
            Tags: "{}"
        );

        var allCards = collection.Decks.SelectMany(d => d.Cards).ToArray();
        var allNotes = allCards.Select(d => d.Note).Distinct().ToArray();
        var deckIdByCardId = collection.Decks
            .SelectMany(deck => deck.Cards.Select(card => (DeckId: deck.Id, CardId: card.Id)))
            .ToDictionary(entry => entry.CardId, entry => entry.DeckId);

        return result with
        {
            Notes = allNotes.Select(n => new Note(
                Id: n.Id,
                Guid: Guid.NewGuid().ToString().Substring(0, 10),
                ModelId: n.NoteTypeId,
                ModificationDateTime: 0,
                UpdateSequenceNumber: 0,
                Tags: string.Empty,
                Fields: n.FieldValues.ToArray(),
                SortField: n.FieldValues[0], // TODO Check this is correct
                FieldChecksum: 0, // TODO Check this is correct
                Flags: 0,
                Data: string.Empty
            )).ToImmutableArray(),

            Cards = allCards.Select(c => new Card(
                Id: c.Id,
                NoteId: c.Note.Id,
                DeckId: deckIdByCardId[c.Id],
                Ordinal: c.NoteCardTypeOrdinal,
                ModificationTime: 0,
                UpdateSequenceNumber: 0,
                LearningType: CardLearningType.New,
                Queue: 0,
                Due: 0,
                Interval: 0,
                EaseFactor: 0,
                ReviewsCount: 0,
                LapsesCount: 0,
                Left: 0,
                OriginalDue: 0,
                OriginalDid: 0,
                Flags: 0,
                Data: string.Empty
            )).ToImmutableArray(),

            RevLogs = ImmutableArray<RevisionLog>.Empty,

            Graves = ImmutableArray<Grave>.Empty,
        };
    }

    public AnkiCollection ConvertCollectionToAnkiCollection(Collection collection)
    {
        var resultCollection = new AnkiCollection();

        // Add the note types.
        {
            var noteTypes = collection.Models.Values
                .Select(model => new AnkiNoteType(
                    id: model.Id,
                    name: model.Name,
                    cardTypes: model.CardTemplates.Select(cardTemplate => new AnkiCardType(
                        Name: cardTemplate.TemplateName,
                        Ordinal: cardTemplate.TemplateOrdinal,
                        QuestionFormat: cardTemplate.QuestionFormat,
                        AnswerFormat: cardTemplate.AnswerFormat
                    )),
                    fieldNames: model.Fields.Select(field => field.FieldName),
                    css: model.Css
                ));

            foreach (var noteType in noteTypes)
            {
                resultCollection.AddNoteType(noteType);
            }
        }

        // Add the decks (except the default deck because AnkiCollection already has it).
        {
            var decks = collection.Decks.Values
                .Where(deck => deck.Id != AnkiCollection.DefaultDeckId)
                .Select(deck => new AnkiDeck(
                    id: deck.Id,
                    name: deck.Name));

            foreach (var deck in decks)
            {
                resultCollection.AddDeck(deck);
            }
        }

        // Add the notes and their associated cards, keeping the existing ids.
        {
            var cardsByNoteId = collection.Cards.ToLookup(c => c.NoteId);

            foreach (var note in collection.Notes)
            {
                var cardsForThisNote = cardsByNoteId[note.Id]; // TODO Error handling
                var deckId = cardsForThisNote.Select(c => c.DeckId).Distinct().Single(); // TODO Error handling

                var ids = cardsForThisNote.Select(c => (c.Ordinal, c.Id)).ToArray();

                resultCollection.AddNoteWithCards(note.Id, deckId, note.ModelId, note.Fields, ids);
            }
        }

        // Ignore RevLogs and Graves

        return resultCollection;
    }
}