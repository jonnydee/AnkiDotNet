using AnkiNet.CollectionFile.Database.Model;
using AnkiNet.DomainModel;

namespace AnkiNet.Infrastructure.Persistence.AnkiFile.Mappers;

internal static class GraveMapper
{
    public static Grave FromDb(grave grave)
        => new(
            Type: (GraveType)grave.type,
            OriginalId: grave.usn,
            UpdateSequenceNumber: grave.oid
        );

    public static grave ToDb(Grave grave)
        => new(
            usn: grave.UpdateSequenceNumber,
            oid: grave.OriginalId,
            type: (long)grave.Type
        );
}
