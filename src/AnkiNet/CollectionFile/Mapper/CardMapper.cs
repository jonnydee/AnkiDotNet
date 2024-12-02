using AnkiNet.CollectionFile.Model;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Model;

namespace AnkiNet.CollectionFile.Mapper;

internal static class CardMapper
{
    public static Card FromDb(card card)
    {
        return new Card(
            Id: card.id,
            NoteId: card.nid,
            DeckId: card.did,
            Ordinal: card.ord,
            ModificationTime: card.mod,
            UpdateSequenceNumber: card.usn,
            LearningType: (CardLearningType)card.type,
            Queue: card.queue,
            Due: card.due,
            Interval: card.ivl,
            EaseFactor: card.factor,
            ReviewsCount: card.reps,
            LapsesCount: card.lapses,
            Left: card.left,
            OriginalDue: card.odue,
            OriginalDid: card.odid,
            Flags: card.flags,
            Data: card.data
        );
    }

    public static card ToDb(Card card)
    {
        return new card(
            id: card.Id,
            nid: card.NoteId,
            did: card.DeckId,
            ord: card.Ordinal,
            mod: card.ModificationTime,
            usn: card.UpdateSequenceNumber,
            type: (long)card.LearningType,
            queue: card.Queue,
            due: card.Due,
            ivl: card.Interval,
            factor: card.EaseFactor,
            reps: card.ReviewsCount,
            lapses: card.LapsesCount,
            left: card.Left,
            odue: card.OriginalDue,
            odid: card.OriginalDid,
            flags: card.Flags,
            data: card.Data
        );
    }
}