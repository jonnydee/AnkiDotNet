using AnkiNet.DomainModel;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Model;

namespace AnkiNet.Infrastructure.Persistence.AnkiFile.Mappers;

internal static class CardMapper
{
    public static (Card Card, DeckId DeckId) FromDb(card cardDb, NoteType noteType)
    {
        var cardTemplateId = noteType.CardTemplates
            .Select((cardTemplate, ordinal) => (Ordinal: ordinal, CardTemplate: cardTemplate))
            .Where(entry => entry.Ordinal == cardDb.ord)
            .Select(entry => entry.CardTemplate.Id)
            .First();

        var card = new Card(
            id: cardDb.id,
            noteId: new NoteId(cardDb.nid),
            cardTemplateId: cardTemplateId,
            revisionLogs: []
        )
        {
            Data = cardDb.data,
            Due = cardDb.due,
            EaseFactor = cardDb.factor,
            Flags = cardDb.flags,
            Interval = cardDb.ivl,
            LapsesCount = cardDb.lapses,
            LearningType = (Card.CardLearningType)cardDb.type,
            Left = cardDb.left,
            ModificationTime = cardDb.mod,
            OriginalDeckId = new DeckId(cardDb.odid),
            OriginalDue = cardDb.odue,
            Queue = cardDb.queue,
            ReviewsCount = cardDb.reps,
            UpdateSequenceNumber = cardDb.usn,
        };

        var deckId = new DeckId(cardDb.did);

        return (card, deckId);
    }

    public static card ToDb(Card card, DeckId deckId, NoteType noteType)
    {
        var cardTemplateOrdinal = noteType.CardTemplates
            .Select((cardTemplate, index) => (CardTemplate: cardTemplate, CardTemplateOrdinal: index))
            .Where(entry => entry.CardTemplate.Id == card.CardTemplateId)
            .Select(entry => entry.CardTemplateOrdinal)
            .Single();

        return new(
            id: card.Id,
            nid: card.NoteId.Value,
            did: deckId.Value,
            ord: cardTemplateOrdinal,
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
            odid: card.OriginalDeckId.Value,
            flags: card.Flags,
            data: card.Data
        );
    }
}
