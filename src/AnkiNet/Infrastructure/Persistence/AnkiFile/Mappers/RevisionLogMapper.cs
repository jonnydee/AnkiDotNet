using AnkiNet.CollectionFile.Database.Model;
using AnkiNet.DomainModel;

namespace AnkiNet.Infrastructure.Persistence.AnkiFile.Mappers;

internal static class RevisionLogMapper
{
    public static (RevisionLog RevisionLog, long CardId) FromDb(revLog revLog)
        => (
            RevisionLog: new(
                timestamp: revLog.id,
                updateSequenceNumber: revLog.usn,
                ease: revLog.ease,
                interval: revLog.ivl,
                lastInterval: revLog.lastIvl,
                factor: revLog.factor,
                timeTookMs: revLog.time,
                revisionType: (RevisionType)revLog.type
            ),
            CardId: revLog.cid
        );

    public static revLog ToDb(RevisionLog revisionLog, long cardId)
        => new(
            id: revisionLog.Id,
            cid: cardId,
            usn: revisionLog.UpdateSequenceNumber,
            ease: revisionLog.Ease,
            ivl: revisionLog.Interval,
            lastIvl: revisionLog.LastInterval,
            factor: revisionLog.Factor,
            time: revisionLog.TimeTookMs,
            type: (long)revisionLog.RevisionType
        );
}