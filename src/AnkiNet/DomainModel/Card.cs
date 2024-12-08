using AnkiNet.DomainModel.Base;
using System.Collections.Immutable;

namespace AnkiNet.DomainModel;

public sealed class Card
    : Entity<long>
{
    public enum CardLearningType
    {
        New = 0,
        Learning = 1,
        Review = 2,
        Relearning = 3
    }

    internal Card(long id, NoteId noteId, string cardTemplateId, IEnumerable<RevisionLog> revisionLogs)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrEmpty(cardTemplateId);
        ArgumentNullException.ThrowIfNull(nameof(revisionLogs));

        NoteId = noteId;
        CardTemplateId = cardTemplateId;
        RevisionLogs = revisionLogs.ToImmutableList();
    }

    public NoteId NoteId { get; }

    public string CardTemplateId { get; }

    public IImmutableList<RevisionLog> RevisionLogs { get; private set; }

    internal Card Clone()
        => new(Id, NoteId, CardTemplateId, RevisionLogs)
        {
            ModificationTime = ModificationTime,
            UpdateSequenceNumber = UpdateSequenceNumber,
            LearningType = LearningType,
            Queue = Queue,
            Due = Due,
            Interval = Interval,
            EaseFactor = EaseFactor,
            ReviewsCount = ReviewsCount,
            LapsesCount = LapsesCount,
            Left = Left,
            OriginalDue = OriginalDue,
            OriginalDeckId = OriginalDeckId,
            Flags = Flags,
            Data = Data
        };

    internal void AddRevisionLog(RevisionLog revisionLog)
    {
        ArgumentNullException.ThrowIfNull(revisionLog);

        ThrowIfRevisionLogWithIdAlreadyExists(revisionLog);

        RevisionLogs = RevisionLogs.Add(revisionLog);
    }

    private void ThrowIfRevisionLogWithIdAlreadyExists(RevisionLog revisionLog)
    {
        if (RevisionLogs.Any(rl => rl.Id == revisionLog.Id))
            throw new InvalidOperationException("A revision log with the same ID already exists.");
    }

    public long ModificationTime { get; init; } // TODO Convert to DateTime?

    public long UpdateSequenceNumber { get; init; }

    public CardLearningType LearningType { get; init; }

    public long Queue { get; init; }

    public long Due { get; init; }

    public long Interval { get; init; } // Negative = seconds, Positive = days // TODO Convert to TimeSpan?

    public long EaseFactor { get; init; }

    public long ReviewsCount { get; init; }

    public long LapsesCount { get; init; }

    public long Left { get; init; } // '2004' means 2 reps left today and 4 reps till graduation // TODO Split?

    public long OriginalDue { get; init; }

    public DeckId OriginalDeckId { get; init; }

    public long Flags { get; init; } // Used for colors?

    public string Data { get; init; } = string.Empty; // Currently unused?
}
