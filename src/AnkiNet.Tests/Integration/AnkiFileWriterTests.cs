using AnkiNet.DomainModel;
using AnkiNet.Infrastructure;

namespace AnkiNet.Tests.Integration;

public class AnkiFileWriterTests
{
    private static readonly ICollectionServiceFactory CollectionServiceFactory = new InMemoryCollectionServiceFactory();

    private const string OutputFolder = "db_files";
    private const string AnkiFileName = "Output.apkg";

    [Fact]
    public async Task WhenWrite_ThenNoExceptionIsThrown()
    {
        await using var collectionService = await CollectionServiceFactory.CreateCollectionServiceAsync();

        //
        // 1. Create everything through the service.
        //

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

        //
        // 2. Write to file
        //

        var outputFilePath = Path.Combine(OutputFolder, AnkiFileName);
        await collectionService.SaveAnkiFileAsync(outputFilePath);
    }
}
