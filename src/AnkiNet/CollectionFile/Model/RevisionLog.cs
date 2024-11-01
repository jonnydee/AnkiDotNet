namespace AnkiNet.CollectionFile.Model;

internal readonly record struct RevisionLog(
    long Id, // Timestamp
    long CardId,
    long UpdateSequenceNumber,
    long Ease,
    long Interval, // See cards table
    long LastInterval,
    long Factor,
    long TimeTookMs,
    RevisionType RevisionType
)
{
    public RevisionEaseType GetEaseType()
    {
        switch (RevisionType)
        {
            case RevisionType.Review:
                return Ease switch
                {
                    1 => RevisionEaseType.Wrong,
                    2 => RevisionEaseType.Hard,
                    3 => RevisionEaseType.Ok,
                    4 => RevisionEaseType.Easy,
                    _ => throw new InvalidOperationException(),
                };

            case RevisionType.Learn:
            case RevisionType.Relearn:
                return Ease switch
                {
                    1 => RevisionEaseType.Wrong,
                    2 => RevisionEaseType.Ok,
                    3 => RevisionEaseType.Easy,
                    _ => throw new InvalidOperationException(),
                };
        }

        throw new InvalidOperationException();
        // TODO Test
    }
}