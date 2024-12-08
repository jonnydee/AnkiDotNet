using AnkiNet.DomainModel;
using AnkiNet.Infrastructure;
using FluentAssertions;

namespace AnkiNet.Tests.Integration;

public class WriteAndReadTests
{
    private static readonly ICollectionServiceFactory CollectionServiceFactory = new InMemoryCollectionServiceFactory();

    [Fact]
    public async Task WhenWriteThenRead_ThenCollectionsAreIdentical()
    {
        await using var collectionService1 = await CollectionServiceFactory.CreateCollectionServiceAsync();
        await using var collectionService2 = await CollectionServiceFactory.CreateCollectionServiceAsync();
        {
            await using var stream = new MemoryStream();

            await InitCollectionServiceAsync(collectionService1);
            await collectionService1.CommitAsync();
            await collectionService1.SaveAnkiFileAsync(stream);

            stream.Position = 0;

            await collectionService2.LoadAnkiFileAsync(stream);
        }

        var expectedCollections = await collectionService1.GetCollectionsAsync().ToArrayAsync();
        var actualCollections = await collectionService2.GetCollectionsAsync().ToArrayAsync();
        actualCollections.Should().BeEquivalentTo(expectedCollections);

        var expectedDecks = await collectionService1.GetDecksAsync().ToArrayAsync();
        var actualDecks = await collectionService2.GetDecksAsync().ToArrayAsync();
        actualDecks.Should().BeEquivalentTo(expectedDecks);

        var expectedNotes = await collectionService1.GetNotesAsync().ToArrayAsync();
        var actualNotes = await collectionService2.GetNotesAsync().ToArrayAsync();
        actualNotes.Should().BeEquivalentTo(expectedNotes);

        var expectedNoteTypes = await collectionService1.GetNoteTypesAsync().ToArrayAsync();
        var actualNoteTypes = await collectionService2.GetNoteTypesAsync().ToArrayAsync();
        actualNoteTypes.Should().BeEquivalentTo(expectedNoteTypes);
    }

    private static async Task InitCollectionServiceAsync(ICollectionService collectionService)
    {
        var collection = await collectionService.CreateCollectionAsync();

        var deck = await collectionService.CreateDeckAsync(collection.Id, "C# Test");

        var noteType = await collectionService.CreateNoteTypeAsync(
            collectionId: collection.Id,
            name: "Basic (With hints)",
            fields:
            [
                Field.Create("Front"),
                Field.Create("Back"),
                Field.Create("Help"),
            ],
            cardTemplates:
            [
                CardTemplate.Create(
                    name: "Forward",
                    questionFormat: "{{Front}}<br/>{{hint:Help}}",
                    answerFormat: "{{Front}}<hr id=\"answer\">{{Back}}"
                ),
                CardTemplate.Create(
                    name: "Backward",
                    questionFormat: "{{Back}}<br/>{{hint:Help}}",
                    answerFormat: "{{Back}}<hr id=\"answer\">{{Front}}"
                )
            ]);
        noteType.Styling =
            """
            .card {font-family: arial;
                font-size: 20px;
                text-align: center;
                color: red;
                background-color: blue;
            }
            """;

        {
            var note = await collectionService.CreateNoteAsync(
                collectionId: collection.Id,
                noteTypeId: noteType.Id,
                fieldValues: new Dictionary<string, string>()
                {
                    ["Front"] = "Bonjour",
                    ["Back"] = "Hello",
                    ["Help"] = "B... H...",
                },
                tags: []);

            deck.AddCardsForNote(note, noteType);
        }

        {
            var note = await collectionService.CreateNoteAsync(
                collectionId: collection.Id,
                noteTypeId: noteType.Id,
                fieldValues: new Dictionary<string, string>()
                {
                    ["Front"] = "Salut",
                    ["Back"] = "Hi",
                    ["Help"] = "S... Hi...",
                },
                tags: []);
            deck.AddCardsForNote(note, noteType);
        }
    }
}
