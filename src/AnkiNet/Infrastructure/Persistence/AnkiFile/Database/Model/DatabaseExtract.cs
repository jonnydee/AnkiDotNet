using System.Collections.Immutable;

namespace AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Model;

internal record DatabaseExtract(
    col col,
    ImmutableArray<card> cards,
    ImmutableArray<grave> graves,
    ImmutableArray<note> notes,
    ImmutableArray<revLog> revLogs
);
