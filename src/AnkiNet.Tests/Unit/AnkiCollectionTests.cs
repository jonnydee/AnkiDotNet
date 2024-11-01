using FluentAssertions;

namespace AnkiNet.Tests.Unit;

public class AnkiCollectionTests
{
    [Fact]
    public void New_AnkiCollection_Has_Default_Deck()
    {
        var collection = new AnkiCollection();

        collection.NoteTypes.Should().BeEmpty();
        collection.Decks.Should().HaveCount(1);

        var defaultDeck = collection.Decks.Single();
        defaultDeck.Name.Should().Be("Default");
        defaultDeck.Id.Should().Be(1);
    }

    [Fact]
    public void New_AnkiNoteType_Without_CardType_Added_To_Collection_Throws()
    {
        var createNoteType = () => _ = new AnkiNoteType(
            id: 1,
            name: "NT",
            cardTypes: [],
            fieldNames: ["A", "B"],
            css: "Css");

        createNoteType.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void New_AnkiNoteType_With_CardType_Added_To_Collection_Is_OK()
    {
        var collection = new AnkiCollection();

        var createAnkiCollection = () => collection.CreateNoteType(
            name: "NT",
            cardTypes: [
                new AnkiCardType(Name: "Name", Ordinal: 0, QuestionFormat: "Q", AnswerFormat: "A")
            ],
            fieldNames: ["A", "B"],
            css: "Css");

        createAnkiCollection.Should().NotThrow();
    }

    [Fact]
    public void AnkiCollection_Cannot_Add_Deck_With_Default_Name()
    {
        var collection = new AnkiCollection();
        var addDeck = () => _ = collection.CreateDeck("Default");
        addDeck.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void AnkiCollection_Cannot_Add_Deck_With_Default_Id_1()
    {
        var collection = new AnkiCollection();
        var addDeck = () => collection.AddDeck(1, "Some deck");
        addDeck.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void AnkiCollection_With_Deck_Cannot_Add_Deck_With_Same_Name()
    {
        var collection = new AnkiCollection();
        _ = collection.CreateDeck("New");
        var addDeck = () => _ = collection.CreateDeck("New");
        addDeck.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void AnkiCollection_With_Deck_Cannot_Add_Deck_With_Same_Id()
    {
        var collection = new AnkiCollection();
        collection.AddDeck(15, "New deck 1");
        var addDeck = () => collection.AddDeck(15, "New deck 2");
        addDeck.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void AnkiCollection_AddNote_With_Unknown_Deck_Id_Throws()
    {
        var collection = new AnkiCollection();
        var addNote = () => collection.CreateNote(
            50,
            1,
            "A", "B"
        );
        addNote.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void AnkiCollection_AddNote_With_Unknown_NoteTypeId_Throws()
    {
        var collection = new AnkiCollection();
        const int unknownNoteTypeId = 15;
        var addNote = () => collection.CreateNote(
            1,
            unknownNoteTypeId,
            "A", "B"
        );
        addNote.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void AnkiCollection_AddNote_To_Deck_With_Known_NoteTypeId_Creates_Cards()
    {
        const long cardTypeOrdinal1 = 23;
        const long cardTypeOrdinal2 = 55;
    
        var collection = new AnkiCollection();

        var noteTypeId = collection.CreateNoteType(
            name: "NT",
            cardTypes: [
                new AnkiCardType(
                    Name: "CT1",
                    Ordinal: cardTypeOrdinal1,
                    QuestionFormat: "Q1",
                    AnswerFormat: "A1"),
                new AnkiCardType(
                    Name: "CT2",
                    Ordinal: cardTypeOrdinal2,
                    QuestionFormat: "Q2",
                    AnswerFormat: "A2"),
            ],
            fieldNames: ["A", "B", "C"],
            css: "css");

        var defaultDeck = collection.DefaultDeck;
        defaultDeck.Cards.Should().BeEmpty();

        collection.CreateNote(defaultDeck.Id, noteTypeId, "A", "B");

        defaultDeck.Cards.Should().HaveCount(2);
        var card1 = defaultDeck.Cards[0];
        var card2 = defaultDeck.Cards[1];

        card1.Note.NoteTypeId.Should().Be(noteTypeId);
        card1.Note.Fields.Should().Equal("A", "B");
        card1.NoteCardTypeOrdinal.Should().Be(cardTypeOrdinal1);

        card2.Note.NoteTypeId.Should().Be(noteTypeId);
        card2.Note.Fields.Should().Equal("A", "B");
        card2.NoteCardTypeOrdinal.Should().Be(cardTypeOrdinal2);
    }

    [Fact]
    public void CheckAllFeatures()
    {
        // Create a collection
        var collection = new AnkiCollection();

        // Create and add a note type with 2 models. This will create 2 cards for each new note
        var noteTypeId = collection.CreateNoteType(
            name: "Back and forth",
            cardTypes: [
                new AnkiCardType(
                    Name: "ID to EN",
                    Ordinal: 0,
                    QuestionFormat: "{{ID}} ",
                    AnswerFormat: """{{ID}}<hr id="answer">{{EN}}"""),
                new AnkiCardType(
                    Name: "EN to ID",
                    Ordinal: 1,
                    QuestionFormat: "{{EN}}",
                    AnswerFormat: """{{EN}}<hr id="answer">{{ID}}"""),
            ],
            fieldNames: ["ID", "EN"],
            css: "css");

        // Create a deck
        var deckId = collection.CreateDeck("Indonesian vocabulary");

        // Create notes, using the note type idx
        collection.CreateNote(deckId, noteTypeId, "Bunga", "Flower");
        collection.CreateNote(deckId, noteTypeId, "Kucing", "Cat");

        // Check the resulting cards
        var allDecks = collection.Decks;
        _ = collection.TryGetDeckByName("Indonesian vocabulary", out var deck1);
        _ = collection.TryGetDeckById(deckId, out var deck2);

        foreach (var c in deck1!.Cards)
        {
            // Read the fields
            var fields = c.Note.Fields;
        }
    }

    [Fact]
    public void AddSeveralNoteTypes_NoIdClash()
    {
        var cardTypes = new[] { new AnkiCardType("CT", 0, "", "") };
        var fields = new[] { "F1", "F2" };
        var css = "";

        var col = new AnkiCollection();
        col.CreateNoteType(name: "A", cardTypes, fields, css);
        col.CreateNoteType(name: "B", cardTypes, fields, css);
        col.CreateNoteType(name: "C", cardTypes, fields, css);
        col.CreateNoteType(name: "D", cardTypes, fields, css);
    }
}