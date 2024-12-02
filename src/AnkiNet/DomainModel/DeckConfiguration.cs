using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace AnkiNet.DomainModel;

public readonly record struct DeckConfiguration
{
    public enum NewCardOrder
    {
        NewCardsDistribute = 0,
        NewCardsLast = 1,
        NewCardsFirst = 2
    }

    public DeckConfiguration()
    {
    }

    /// <summary>
    /// Last modification time.
    /// </summary>
    public long LastModificationTime { get; init; } // TODO Use DateTime?

    /// <summary>
    /// The name of the configuration.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Update Sequence Number.
    /// </summary>
    public long UpdateSequenceNumber { get; init; }

    /// <summary>
    /// Whether the audio associated to a question should be played when the question is shown.
    /// </summary>
    public bool AutoplayQuestionAudio { get; init; }

    /// <summary>
    /// Whether the audio associated to a question should be played when the answer is shown.
    /// </summary>
    public bool ReplayQuestionAudio { get; init; }

    /// <summary>
    /// Whether timer should be shown (1) or not (0).
    /// </summary>
    public int ShowTimer { get; init; }

    /// <summary>
    /// Whether this deck is dynamic.
    /// </summary>
    public bool IsDynamic { get; init; }

    /// <summary>
    /// The number of seconds after which to stop the timer.
    /// </summary>
    public long StopTimerAfterSeconds { get; init; }

    /// <summary>
    /// The configuration for lapse cards.
    /// </summary>
    public LapseCardsConfiguration LapseCards { get; init; } = new();

    /// <summary>
    /// The configuration for new cards.
    /// </summary>
    public NewCardsConfiguration NewCards { get; init; } = new();

    /// <summary>
    /// The configuration for review cards.
    /// </summary>
    public ReviewCardsConfiguration ReviewCards { get; init; } = new();

    public readonly record struct LapseCardsConfiguration
    {
        public enum LeechActionType
        {
            Suspend = 0,
            Mark = 1
        }

        public LapseCardsConfiguration()
        {
        }

        /// <summary>
        /// TODO Is this description correct?
        /// The list of successive delay between the learning steps of the new cards, as explained in the manual.
        /// </summary>
        public ImmutableArray<float> Delays { get; init; } = [];

        /// <summary>
        /// What to do to leech cards. 0 for suspend, 1 for mark.
        /// Numbers according to the order in which the choices appear in aqt/dconf.ui
        /// </summary>
        public LeechActionType LeechAction { get; init; }

        /// <summary>
        /// The number of lapses authorized before doing leechAction.
        /// </summary>
        public int LeechFailsAllowedCount { get; init; }

        /// <summary>
        /// Lower limit to the new interval after a leech.
        /// </summary>
        public long MinimumInterfalAfterLeech { get; init; }

        /// <summary>
        /// Percent by which to multiply the current interval when a card has lapsed.
        /// </summary>
        public float LapsedIntervalMultiplierPercent { get; init; }
    }

    public readonly record struct NewCardsConfiguration
    {
        public enum NewCardsOrder
        {
            NewCardsRandom,
            NewCardsDue,
        }

        public NewCardsConfiguration()
        {
        }

        /// <summary>
        /// Whether to bury cards related to new cards answered.
        /// </summary>
        public bool Bury { get; init; }

        /// <summary>
        /// The list of successive delay between the learning steps of the new cards, as explained in the manual.
        /// </summary>
        public ImmutableArray<float> Delays { get; init; } = [];

        /// <summary>
        /// The initial ease factor.
        /// </summary>
        public int InitialEaseFactor { get; init; } // TODO What's the type? Int or float?

        /// <summary>
        /// The list of delays according to the button pressed while leaving the learning mode.
        /// Good, easy and unused.
        /// In the GUI, the first two elements corresponds to Graduating Interval and Easy interval.
        /// </summary>
        public ImmutableArray<int> IntDelays { get; init; } = [];

        /// <summary>
        /// In which order new cards must be shown.
        /// </summary>
        public NewCardsOrder NewCardsShowOrder { get; init; }

        /// <summary>
        /// Maximal number of new cards shown per day.
        /// </summary>
        public int NewCardsPerDay { get; init; }

        /// <summary>
        /// Seems to be unused in the code.
        /// </summary>
        public int Separate { get; init; } // TODO What's the type?
    }

    public readonly record struct ReviewCardsConfiguration
    {
        public ReviewCardsConfiguration()
        {
        }

        /// <summary>
        /// Whether to bury cards related to new cards answered.
        /// </summary>
        public bool Bury { get; init; }

        /// <summary>
        /// The number to add to the easyness when the easy button is pressed.
        /// </summary>
        public float Ease4 { get; init; }

        /// <summary>
        /// The new interval is multiplied by a random number between -fuzz and fuzz
        /// </summary>
        public int Fuzz { get; init; } // TODO What type is it???? Float or Int?

        /// <summary>
        /// Multiplication factor applied to the intervals Anki generates.
        /// </summary>
        public float IntervalMultiplicationFactor { get; init; }

        /// <summary>
        /// Maximal interval for reviews. // TODO What's the unit?
        /// </summary>
        public int MaximumReviewInterval { get; init; }

        /// <summary>
        /// Not currently used. // TODO What is the type?
        /// </summary>
        public int MinSpace { get; init; }

        /// <summary>
        /// Numbers of cards to review per day
        /// </summary>
        public int CardsToReviewPerDay { get; init; }

        /// <summary>
        /// ?? TODO Not in the doc
        /// </summary>
        public float HardFactor { get; init; }
    }
}
