using AnkiNet.DomainModel;
using AnkiNet.Infrastructure.Persistence.AnkiFile;
using AnkiNet.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace AnkiNet.Tests.Integration;

public class AnkiFileReaderTests
{
    private readonly IDeckRepository _deckRepository = new InMemoryDeckRepository();
    private readonly INoteRepository _noteRepository = new InMemoryNoteRepository();
    private readonly INoteTypeRepository _noteTypeRepository = new InMemoryNoteTypeRepository();
    private readonly ICollectionRepository _collectionRepository = new InMemoryCollectionRepository();
    private AnkiFileAccess _ankiFileAccess;


    public AnkiFileReaderTests()
    {
        _ankiFileAccess = new AnkiFileAccess(
            _deckRepository,
            _noteRepository,
            _noteTypeRepository,
            _collectionRepository);
    }

    [Fact]
    public async Task WhenRead_ThenNoExceptionIsThrown()
    {
        await _ankiFileAccess.LoadAsync("db_files/Japanese.apkg");

        var collection = await _collectionRepository.GetAllAsync().FirstAsync();

        var decks = collection.Decks
            .Select(deckId => _deckRepository.GetByIdAsync(deckId).Result)
            .ToArray();

        var expectedDecks = new[]
        {
            (Id: Configurations.DefaultDeckId, Name: DeckConfigurations.DefaultDeckName),
            (Id: new DeckId(1661780077840), Name: "Japanese::YouTube::1. Premiers mots en kanji"),
            (Id: new DeckId(1663496509878), Name: "Japanese"),
            (Id: new DeckId(1663496546962), Name: "Japanese::YouTube"),
            (Id: new DeckId(1661656229326), Name: "Japanese::YouTube::3. Le matériel domestique"),
            (Id: new DeckId(1663496558232), Name: "Japanese::Class")
        };

        decks.Select(deck => (Id: deck!.Id, Name: deck.Name)).Should().BeEquivalentTo(expectedDecks);

        var noteTypes = collection.NoteTypes
            .Select(noteTypeId => _noteTypeRepository.GetByIdAsync(noteTypeId).Result)
            .Where(noteType => noteType is not null)
            .ToArray();

        {
            var noteType = await _noteTypeRepository.GetByIdAsync(new NoteTypeId(1663496639418L))!;
            noteType!.Should().NotBeNull();
            
            noteType!.Name.Should().Be("Basic");
         
            noteType.Styling.Should().Be((string?)
                ".card {\n    font-family: arial;\n    font-size: 20px;\n    text-align: center;\n    color: black;\n    background-color: white;\n}\n");

            noteType.Fields.Select(field => field.Name).Should().Equal("Front", "Back");
            
            noteType.CardTemplates.Should().HaveCount(1);
            var cardTemplate = noteType.CardTemplates[0];
            cardTemplate.Name.Should().Be("Card 1");
            cardTemplate.QuestionFormat.Should().Be("{{Front}}");
            cardTemplate.AnswerFormat.Should().Be("{{FrontSide}}\n\n<hr id=answer>\n\n{{Back}}");
        }

        {
            var noteType = await _noteTypeRepository.GetByIdAsync(new NoteTypeId(1661780059778L))!;
            noteType!.Should().NotBeNull();

            noteType!.Name.Should().Be("Basic-6d0e1");

            noteType.Styling.Should().Be((string?)
                ".card {\nfont-family: arial;\nfont-size: 20px;\ntext-align: center;\ncolor: black;\nbackground-color: white;\n}");

            noteType.Fields.Select(field => field.Name).Should().Equal("Front", "Back");

            noteType.CardTemplates.Should().HaveCount(1);
            var cardTemplate = noteType.CardTemplates[0];
            cardTemplate.Name.Should().Be("Forward");
            cardTemplate.QuestionFormat.Should().Be("{{Front}}\n");
            cardTemplate.AnswerFormat.Should().Be("{{FrontSide}}\n<hr id=answer />\n{{Back}}");
        }

        {
            var noteType = await _noteTypeRepository.GetByIdAsync(new NoteTypeId(1661656212286L))!;
            noteType!.Should().NotBeNull();

            noteType!.Name.Should().Be("Basic-64627");

            noteType.Styling.Should().Be((string?)
                ".card {\nfont-family: arial;\nfont-size: 20px;\ntext-align: center;\ncolor: black;\nbackground-color: blue;\n}");

            noteType.Fields.Select(field => field.Name).Should().Equal("Front", "Back", "Help");

            noteType.CardTemplates.Should().HaveCount(1);
            var cardTemplate = noteType.CardTemplates[0];
            cardTemplate.Name.Should().Be("Forward");
            cardTemplate.QuestionFormat.Should().Be("{{Front}}\n<div style='font-family: \"Ayuthaya\"; font-size: 15px;'>{{Help}}</div>\n");
            cardTemplate.AnswerFormat.Should().Be("{{FrontSide}}\n<hr id=answer />\n{{Back}}");
        }

        {
            var noteType = await _noteTypeRepository.GetByIdAsync(new NoteTypeId(1661780059778L))!;

            var fieldNames = noteType!.Fields.Select(field => field.Name).ToArray();

            (NoteId NoteId, string[] FieldValues)[] expectedNotes =
            [
                (NoteId: new NoteId(1661780059797L), FieldValues: ["人【ひと】", "person; someone; somebody"]),
                (NoteId: new NoteId(1661780059804L), FieldValues: ["男【おとこ】", "man; male"]),
                (NoteId: new NoteId(1661780059807L), FieldValues: ["女【おんな】", "female; woman; female sex"]),
                (NoteId: new NoteId(1661780059809L), FieldValues: ["子【こ】", "child; kid; teenager; youngster; young (non-adult) person"]),
                (NoteId: new NoteId(1661780059811L), FieldValues: ["日【ひ】", "day; days"]),
                (NoteId: new NoteId(1661780059814L), FieldValues: ["月【つき】", "Moon"]),
                (NoteId: new NoteId(1661780059816L), FieldValues: ["時【とき】", "time; hour; moment"]),
                (NoteId: new NoteId(1661780059819L), FieldValues: ["水【みず】", "water (esp. cool, fresh water, e.g. drinking water)"]),
                (NoteId: new NoteId(1661780059821L), FieldValues: ["火【ひ】", "fire; flame; blaze"]),
                (NoteId: new NoteId(1661780059823L), FieldValues: ["土【つち】", "earth; soil; dirt; clay; mud"]),
                (NoteId: new NoteId(1661780059825L), FieldValues: ["風【かぜ】", "wind; breeze; draught; draft"]),
                (NoteId: new NoteId(1661780059826L), FieldValues: ["空【そら】", "sky; the air; the heavens"]),
                (NoteId: new NoteId(1661780059828L), FieldValues: ["山【やま】", "mountain; hill"]),
                (NoteId: new NoteId(1661780059830L), FieldValues: ["川【かわ】", "river; stream"]),
                (NoteId: new NoteId(1661780059832L), FieldValues: ["木【き】", "tree; shrub; bush"]),
                (NoteId: new NoteId(1661780059835L), FieldValues: ["花【はな】", "flower; blossom; bloom; petal"]),
                (NoteId: new NoteId(1661780059837L), FieldValues: ["雨【あめ】", "rain"]),
                (NoteId: new NoteId(1661780059839L), FieldValues: ["雪【ゆき】", "snow; snowfall"]),
                (NoteId: new NoteId(1661780059840L), FieldValues: ["金【かね】", "money"]),
                (NoteId: new NoteId(1661780059843L), FieldValues: ["刀【かたな】", "sword (esp. Japanese single-edged); katana"]),
            ];

            var notes = collection.Notes
                .Select(noteId => _noteRepository.GetByIdAsync(noteId).Result!)
                .Where(note => note.NoteTypeId == noteType!.Id)
                .OrderBy(note => note.Id)
                .ToArray();

            var notesRead = notes.Select(note =>
            {
                var fieldValues = fieldNames.Select(name => note.FieldValues.ToDictionary()[name]).ToArray();
                return (NoteId: note.Id, FieldValues: fieldValues);
            }).ToArray();

            notesRead.Select(x => x.NoteId).Should().BeEquivalentTo(expectedNotes.Select(x => x.NoteId));
        }

        {
            var noteType = await _noteTypeRepository.GetByIdAsync(new NoteTypeId(1661780059778L))!;

            var cardTemplateNames = noteType!.CardTemplates.Select(cardTemplate => cardTemplate.Name).ToArray();

            var notes = collection.Notes
                .Select(noteId => _noteRepository.GetByIdAsync(noteId).Result!)
                .Where(note => note.NoteTypeId == noteType!.Id)
                .OrderBy(note => note.Id)
                .ToArray();

            var noteIds = notes.Select(note => note.Id).ToArray();

            var cards = collection.Decks
                .SelectMany(deckId => _deckRepository.GetByIdAsync(deckId).Result!.Cards)
                .ToArray();

            var cardsRead = cards
                .Where(card => noteIds.Contains(card.NoteId))
                .Select(card => (CardId: card.Id, CardTemplateOrdinal: Array.IndexOf(cardTemplateNames, card.CardTemplateId)))
                .ToArray();

            var expectedCards = new[]
            {
                (CardId: 1661780059803L, CardTemplateOrdinal: 0),
                (CardId: 1661780059806L, CardTemplateOrdinal: 0),
                (CardId: 1661780059808L, CardTemplateOrdinal: 0),
                (CardId: 1661780059810L, CardTemplateOrdinal: 0),
                (CardId: 1661780059813L, CardTemplateOrdinal: 0),
                (CardId: 1661780059815L, CardTemplateOrdinal: 0),
                (CardId: 1661780059817L, CardTemplateOrdinal: 0),
                (CardId: 1661780059820L, CardTemplateOrdinal: 0),
                (CardId: 1661780059822L, CardTemplateOrdinal: 0),
                (CardId: 1661780059824L, CardTemplateOrdinal: 0),
                (CardId: 1661780059825L, CardTemplateOrdinal: 0),
                (CardId: 1661780059827L, CardTemplateOrdinal: 0),
                (CardId: 1661780059829L, CardTemplateOrdinal: 0),
                (CardId: 1661780059831L, CardTemplateOrdinal: 0),
                (CardId: 1661780059834L, CardTemplateOrdinal: 0),
                (CardId: 1661780059836L, CardTemplateOrdinal: 0),
                (CardId: 1661780059838L, CardTemplateOrdinal: 0),
                (CardId: 1661780059840L, CardTemplateOrdinal: 0),
                (CardId: 1661780059841L, CardTemplateOrdinal: 0),
                (CardId: 1661780059843L, CardTemplateOrdinal: 0),
            };

            cardsRead.Should().BeEquivalentTo(expectedCards);
        }
    }

    [Fact]
    public async Task ReadCollection21_NoError()
    {
        await _ankiFileAccess.LoadAsync("db_files/collection21.apkg");

        var collection = await _collectionRepository.GetAllAsync().FirstAsync();

        var decks = collection.Decks
            .Select(deckId => _deckRepository.GetByIdAsync(deckId).Result!)
            .ToArray();

        decks.Should().HaveCount(3);

        {
            var deck = decks[0];
            deck.Id.Should().Be(Configurations.DefaultDeckId);
            deck.Name.Should().Be(DeckConfigurations.DefaultDeckName);
            deck.Cards.Should().BeEmpty();
        }

        {
            var deck = decks[2];
            deck.Id.Should().Be(new DeckId(1691848838057L));
            deck.Cards.Should().HaveCount(16);

            var card = deck.Cards.First();
            var note = await _noteRepository.GetByIdAsync(card.NoteId);
            note!.FieldValues.Select(entry => entry.Value).Should().BeEquivalentTo("Bunga", "Flower");
        }
    }

    [Fact]
    public async Task ReadCollection21b_NotImplementedException()
    {
        var action = () => _ankiFileAccess.LoadAsync("db_files/collection21b.apkg");
        await action.Should().ThrowExactlyAsync<NotImplementedException>();

        /*
         * If no exception is thrown, a deck with single card like below will be read in collection21 database file.
         * 
        var collection = await AnkiFileReader.ReadFromFileAsync("db_files/collection21b.apkg");

        collection.Id.Should().Be(1);

        collection.Decks.Should().HaveCount(1);
        collection.Decks.First().Id.Should().Be(1);
        collection.Decks.First().Cards.Should().HaveCount(1);

        var card = collection.Decks.Single().Cards.Single();
        card.Note.Fields.Should().Equal("Please update to the latest Anki version, then import the .colpkg/.apkg file again.", "");
        */
    }
}