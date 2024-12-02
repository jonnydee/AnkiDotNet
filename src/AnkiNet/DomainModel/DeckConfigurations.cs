namespace AnkiNet.DomainModel;

public static class DeckConfigurations
{
    public const string DefaultDeckName = "Default";

    public static DeckConfiguration Default => new()
    {
        LastModificationTime = 0,
        Name = DefaultDeckName,
        UpdateSequenceNumber = 0,
        AutoplayQuestionAudio = true,
        ReplayQuestionAudio = true,
        ShowTimer = 0,
        IsDynamic = false,
        StopTimerAfterSeconds = 0,
        LapseCards = new()
        {
            Delays = [10f],
            LapsedIntervalMultiplierPercent = 0,
            LeechAction = DeckConfiguration.LapseCardsConfiguration.LeechActionType.Mark,
            LeechFailsAllowedCount = 8,
            MinimumInterfalAfterLeech = 1,
        },
        NewCards = new()
        {
            Bury = false,
            Delays = [1f, 10f],
            InitialEaseFactor = 2500,
            IntDelays = [1, 4, 0],
            NewCardsPerDay = 20,
            NewCardsShowOrder = DeckConfiguration.NewCardsConfiguration.NewCardsOrder.NewCardsDue,
            Separate = 0,
        },
        ReviewCards = new()
        {
            Bury = false,
            CardsToReviewPerDay = 200,
            Ease4 = 1.3f,
            Fuzz = 0,
            HardFactor = 1.2f,
            IntervalMultiplicationFactor = 1,
            MaximumReviewInterval = 36500,
            MinSpace = 0,
        },
    };
}
