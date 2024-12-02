using AnkiNet.DomainModel.Base;

namespace AnkiNet.DomainModel;

/// <summary>
/// Represents a revision log entry.
/// 
/// The RevisionLog class in the Anki domain model serves as a detailed record of
/// a revision (study session) for a card. It captures various attributes related
/// to the revision session, providing a historical log that can be used for tracking,
/// analysis, and reporting purposes.
/// </summary>
public sealed class RevisionLog
    : Entity<long>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RevisionLog"/> class.
    /// </summary>
    /// <param name="timestamp">The unique identifier for the RevisionLog, representing the time at which the revision session occurred.</param>
    /// <param name="updateSequenceNumber">A sequence number used for tracking changes and synchronization purposes.</param>
    /// <param name="ease">The ease rating given during the revision session, indicating how well the user remembered the card.</param>
    /// <param name="interval">The interval until the next review, which can be used to schedule future study sessions.</param>
    /// <param name="lastInterval">The interval before the current review, providing context for the current session.</param>
    /// <param name="factor">A factor used in scheduling the next review, influencing the interval calculation.</param>
    /// <param name="timeTookMs">The time taken for the review session, measured in milliseconds.</param>
    /// <param name="revisionType">The type of revision session (e.g., Learn, Review, Relearn, Cram).</param>
    public RevisionLog(
        long timestamp,
        long updateSequenceNumber,
        long ease,
        long interval,
        long lastInterval,
        long factor,
        long timeTookMs,
        RevisionType revisionType)
        : base(timestamp)
    {
        UpdateSequenceNumber = updateSequenceNumber;
        Ease = ease;
        Interval = interval;
        LastInterval = lastInterval;
        Factor = factor;
        TimeTookMs = timeTookMs;
        RevisionType = revisionType;
    }

    /// <summary>
    /// Gets the unique identifier for the RevisionLog, representing the time at which the revision session occurred.
    /// </summary>
    public long Timestamp => Id;

    /// <summary>
    /// Gets the sequence number used for tracking changes and synchronization purposes.
    /// </summary>
    public long UpdateSequenceNumber { get; private set; }

    /// <summary>
    /// Gets the ease rating given during the revision session, indicating how well the user remembered the card.
    /// </summary>
    public long Ease { get; private set; }

    /// <summary>
    /// Gets the interval until the next review, which can be used to schedule future study sessions.
    /// </summary>
    public long Interval { get; private set; } // See cards table

    /// <summary>
    /// Gets the interval before the current review, providing context for the current session.
    /// </summary>
    public long LastInterval { get; private set; }

    /// <summary>
    /// Gets the factor used in scheduling the next review, influencing the interval calculation.
    /// </summary>
    public long Factor { get; private set; }

    /// <summary>
    /// Gets the time taken for the review session, measured in milliseconds.
    /// </summary>
    public long TimeTookMs { get; private set; }

    /// <summary>
    /// Gets the type of revision session (e.g., Learn, Review, Relearn, Cram).
    /// </summary>
    public RevisionType RevisionType { get; private set; }

    /// <summary>
    /// Gets the ease type based on the revision type and ease rating.
    /// </summary>
    /// <returns>The ease type based on the revision type and ease rating.</returns>
    public RevisionEaseType GetEaseType()
        => RevisionType switch
        {
            RevisionType.Review
                => Ease switch
                {
                    1 => RevisionEaseType.Wrong,
                    2 => RevisionEaseType.Hard,
                    3 => RevisionEaseType.Ok,
                    4 => RevisionEaseType.Easy,
                    _ => throw new InvalidOperationException(),
                },

            RevisionType.Learn or RevisionType.Relearn
                => Ease switch
                {
                    1 => RevisionEaseType.Wrong,
                    2 => RevisionEaseType.Ok,
                    3 => RevisionEaseType.Easy,
                    _ => throw new InvalidOperationException(),
                },

            _ => throw new InvalidOperationException(),
        };
}
