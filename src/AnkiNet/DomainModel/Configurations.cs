using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnkiNet.DomainModel;

public static class Configurations
{
    public static readonly DeckId DefaultDeckId = new(1);

    public static readonly Configuration Default
        = new()
        {
            SortBackwards = false,
            CurrentDeck = DefaultDeckId,
            DueCounts = true,
            SortType = "noteFld",
            //CurrentNoteType = new(1), // TODO
            TimeLimit = 0,
            NewSpread = 0,
            CollapseTime = 1200,
            EstimateTimes = true,
            AddToCurrent = true,
            NextPosition = 1,
            DayLearnFirst = false,
            SchedulerVersion = 2,
            CreationOffset = -480,
            NewBury = false,
            LastUnburied = 0,
            ActiveDecks = [DefaultDeckId],
        };
}
