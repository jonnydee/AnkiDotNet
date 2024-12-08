using AnkiNet.DomainModel;
using AnkiNet.Infrastructure;
using FluentAssertions;

namespace AnkiNet.Tests.Unit;

public class AnkiFileReaderTests
{
    private static readonly ICollectionServiceFactory CollectionServiceFactory = new InMemoryCollectionServiceFactory();

    [Fact]
    public async Task WhenRead_ThenNoExceptionIsThrown()
    {
        await using var collectionService = await CollectionServiceFactory.CreateCollectionServiceAsync();

        var action = () => collectionService.LoadAnkiFileAsync("unknown");
        await action.Should().ThrowExactlyAsync<FileNotFoundException>();
    }
}
