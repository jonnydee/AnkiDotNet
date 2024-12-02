using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using AnkiNet.DomainModel;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Json;
using AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Model;

namespace AnkiNet.Infrastructure.Persistence.AnkiFile.Mappers;

internal static class CollectionMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    #region Database -> Domain Model

    public static Collection CollectionFromDb(col col)
    {
        var jsonConfiguration = JsonSerializer.Deserialize<JsonConfiguration>(col.conf, SerializerOptions)!;

        return new(
            id: new CollectionId(col.id),
            configuration: FromJson(jsonConfiguration),
            noteTypes: [],
            decks: [],
            notes: [],
            graves: []
        )
        {
            CreationDateTime = col.crt,
            IsDirty = col.dty == 1,
            LastModifiedDateTime = col.mod,
            LastSyncDateTime = col.ls,
            SchemaModificationDateTime = col.scm,
            UpdateSequenceNumber = col.usn,
            Version = col.ver,
        };
    }

    public static IEnumerable<NoteType> NoteTypesFromDb(col col)
    {
        var jsonModels = JsonSerializer.Deserialize<Dictionary<long, JsonModel>>(col.models, SerializerOptions)!;

        return jsonModels.Values.Select(FromJson);
    }

    public static IEnumerable<Deck> DecksFromDb(col col)
    {
        var jsonDecks = JsonSerializer.Deserialize<Dictionary<long, JsonDeck>>(col.decks, SerializerOptions)!;
        var jsonDeckConfigurations = JsonSerializer.Deserialize<Dictionary<long, JsonDeckConfguration>>(col.dconf, SerializerOptions)!;

        return jsonDecks.Values
            .Join(
                inner: jsonDeckConfigurations.Values,
                outerKeySelector: jsonDeck => jsonDeck.Id,
                innerKeySelector: jsonDeckConfiguration => jsonDeckConfiguration.Id,
                resultSelector: FromJson)
            .ToImmutableArray();
    }

    private static Configuration FromJson(JsonConfiguration jsonConfiguration)
        => new()
        {
            ActiveColumns = jsonConfiguration.ActiveColumns?
                .ToImmutableArray() ?? [],
            ActiveDecks = jsonConfiguration.ActiveDecks?
                .Select(deckId => new DeckId(deckId))
                .ToImmutableArray() ?? [],
            AddToCurrent = jsonConfiguration.AddToCurrent,
            CollapseTime = jsonConfiguration.CollapseTime,
            CreationOffset = jsonConfiguration.CreationOffset,
            CurrentDeck = new DeckId(jsonConfiguration.CurrentDeck),
            DueCounts = jsonConfiguration.DueCounts,
            LastUnburied = jsonConfiguration.LastUnburied,
            CurrentNoteType = new NoteTypeId(jsonConfiguration.CurrentModel),
            DayLearnFirst = jsonConfiguration.DayLearnFirst,
            EstimateTimes = jsonConfiguration.EstimateTimes,
            NewBury = jsonConfiguration.NewBury,
            NewSpread = (Configuration.NewCardOrder)jsonConfiguration.NewSpread,
            NextPosition = jsonConfiguration.NextPosition,
            SchedulerVersion = jsonConfiguration.SchedulerVersion,
            SortBackwards = jsonConfiguration.SortBackwards,
            SortType = jsonConfiguration.SortType,
            TimeLimit = jsonConfiguration.TimeLimit,
        };

    private static Deck FromJson(JsonDeck jsonDeck, JsonDeckConfguration jsonDeckConfiguration)
    {
        return new(
            id: new DeckId(jsonDeck.Id),
            name: jsonDeck.Name,
            deckConfiguration: FromJson(jsonDeckConfiguration),
            cards: [])
        {
            LastModificationTime = jsonDeck.LastModificationTime,
            UpdateSequenceNumber = jsonDeck.UpdateSequenceNumber,
            NewToday = (jsonDeck.NewToday[0], jsonDeck.NewToday[1]),
            ReviewedToday = (jsonDeck.ReviewedToday[0], jsonDeck.ReviewedToday[1]),
            LearnedToday = (jsonDeck.LearnedToday[0], jsonDeck.LearnedToday[1]),
            TimeToday = (jsonDeck.TimeToday[0], jsonDeck.TimeToday[1]),
            IsCollapsed = jsonDeck.IsCollapsed,
            IsCollapsedInBrowser = jsonDeck.IsCollapsedInBrowser,
            Description = jsonDeck.Description,
            IsDynamic = jsonDeck.IsDynamic == 1,
            ConfigurationGroupId = jsonDeck.ConfigurationGroupId,
            ExtendedNewCardLimit = jsonDeck.ExtendedNewCardLimit,
            ExtendedReviewCardLimit = jsonDeck.ExtendedReviewCardLimit,
        };

        #region Helper function(s)

        static DeckConfiguration FromJson(JsonDeckConfguration jsonDeckConfguration)
            => new()
            {
                AutoplayQuestionAudio = jsonDeckConfguration.AutoplayQuestionAudio,
                IsDynamic = jsonDeckConfguration.IsDynamic,
                LapseCards = LeapsCardsConfigurationFromJson(jsonDeckConfguration.LapseCardsConfiguration),
                LastModificationTime = jsonDeckConfguration.LastModificationTime,
                Name = jsonDeckConfguration.Name,
                NewCards = NewCardsConfigurationFromJson(jsonDeckConfguration.NewCardsConfiguration),
                ReplayQuestionAudio = jsonDeckConfguration.ReplayQuestionAudio,
                ReviewCards = ReviewCardsConfigurationFromJson(jsonDeckConfguration.ReviewCardsConfiguration),
                ShowTimer = jsonDeckConfguration.ShowTimer,
                StopTimerAfterSeconds = jsonDeckConfguration.StopTimerAfterSeconds,
                UpdateSequenceNumber = jsonDeckConfguration.UpdateSequenceNumber,
            };

        static DeckConfiguration.LapseCardsConfiguration LeapsCardsConfigurationFromJson(
            JsonLapseCardsConfiguration jsonLapseCardsConfiguration)
            => new()
            {
                Delays = jsonLapseCardsConfiguration.Delays?.ToImmutableArray() ?? [],
                LapsedIntervalMultiplierPercent = jsonLapseCardsConfiguration.LapsedIntervalMultiplierPercent,
                LeechAction = (DeckConfiguration.LapseCardsConfiguration.LeechActionType)jsonLapseCardsConfiguration.LeechAction,
                LeechFailsAllowedCount = jsonLapseCardsConfiguration.LeechFailsAllowedCount,
                MinimumInterfalAfterLeech = jsonLapseCardsConfiguration.MinimumInterfalAfterLeech,
            };

        static DeckConfiguration.NewCardsConfiguration NewCardsConfigurationFromJson(
            JsonNewCardsConfiguration jsonNewCardsConfiguration)
            => new()
            {
                Bury = jsonNewCardsConfiguration.Bury,
                Delays = jsonNewCardsConfiguration.Delays?.ToImmutableArray() ?? [],
                InitialEaseFactor = jsonNewCardsConfiguration.InitialEaseFactor,
                IntDelays = jsonNewCardsConfiguration.IntDelays?.ToImmutableArray() ?? [],
                NewCardsPerDay = jsonNewCardsConfiguration.NewCardsPerDay,
                NewCardsShowOrder = (DeckConfiguration.NewCardsConfiguration.NewCardsOrder)jsonNewCardsConfiguration.NewCardsShowOrder,
                Separate = jsonNewCardsConfiguration.Separate,
            };

        static DeckConfiguration.ReviewCardsConfiguration ReviewCardsConfigurationFromJson(
            JsonReviewCardsConfiguration jsonReviewCardsConfiguration)
            => new()
            {
                Bury = jsonReviewCardsConfiguration.Bury,
                CardsToReviewPerDay = jsonReviewCardsConfiguration.CardsToReviewPerDay,
                Ease4 = jsonReviewCardsConfiguration.Ease4,
                Fuzz = jsonReviewCardsConfiguration.Fuzz,
                HardFactor = jsonReviewCardsConfiguration.HardFactor,
                IntervalMultiplicationFactor = jsonReviewCardsConfiguration.IntervalMultiplicationFactor,
                MaximumReviewInterval = jsonReviewCardsConfiguration.MaximumReviewInterval,
                MinSpace = jsonReviewCardsConfiguration.MinSpace,
            };

        #endregion
    }

    private static NoteType FromJson(JsonModel model)
        => new(
            id: new NoteTypeId(model.Id),
            name: model.Name,
            fields: model.Fields
                .Select(FromJson)
                .OrderBy(entry => entry.Ordinal)
                .Select(entry => entry.Field),
            cardTemplates: model.CardTemplates
                .Select(FromJson)
                .OrderBy(entry => entry.Ordinal)
                .Select(entry => entry.CardTemplate))
        {
            ModificationTime = model.ModificationTime,
            LatexPre = model.LatexPre,
            LatexPost = model.LatexPost,
            LatexSvg = model.LatexSvg,
            BrowserSortField = model.BrowserSortField,
            DefaultDeckId = model.DefaultDeckId is { } defaultDeckIdValue
                ? new DeckId(defaultDeckIdValue)
                : DeckId.Empty,
            ModelType = (ModelType)model.ModelType,
            UpdateSequenceNumber = model.UpdateSequenceNumber,
            LastAddedNoteTags = Tags.FromStrings(model.LastAddedNoteTags).ToImmutableArray(),
            Styling = model.Css,
        };

    private static (int Ordinal, Field Field) FromJson(JsonField jsonField)
    {
        var field = new Field(name: jsonField.FieldName)
        {
            Description = jsonField.Description,
            Font = jsonField.Font,
            FontSize = jsonField.FontSize,
            IsRightToLeft = jsonField.IsRightToLeft,
            IsSticky = jsonField.IsSticky,
            Media = jsonField.Media?.ToImmutableArray() ?? [],
        };

        return (Ordinal: jsonField.FieldNumber, Field: field);
    }

    private static (long Ordinal, CardTemplate CardTemplate) FromJson(JsonCardTemplate jsonCardTemplate)
    {
        var cardTemplate = new CardTemplate(name: jsonCardTemplate.TemplateName)
        {
            AnswerFormat = jsonCardTemplate.AnswerFormat,
            BrowserAnswerFormat = jsonCardTemplate.BrowserAnswerFormat,
            BrowserQuestionFormat = jsonCardTemplate.BrowserQuestionFormat,
            DeckOverrideId = jsonCardTemplate.DeckOverrideId,
            BFont = jsonCardTemplate.BFont,
            BSize = jsonCardTemplate.BSize,
            QuestionFormat = jsonCardTemplate.QuestionFormat,
        };

        return (Ordinal: jsonCardTemplate.TemplateOrdinal, CardTemplate: cardTemplate);
    }

    #endregion

    #region Domain Model -> Database

    public static col ToDb(Collection collection, IEnumerable<NoteType> noteTypes, IEnumerable<Deck> decks)
    {
        var jsonConfiguration = ToJson(collection.Configuration);

        var jsonModels = noteTypes.ToDictionary(
            noteType => noteType.Id.Value,
            ToJson);

        var jsonDeckAndConfiguration = decks.Select(ToJson).ToArray();

        var jsonDecks = decks.ToDictionary(
            deck => deck.Id.Value,
            ToJson);

        var jsonDeckConfigurations = decks.ToDictionary(
            deck => deck.Id.Value,
            deck => ToJson(deck.Id, deck.DeckConfiguration));

        var jsonConfigurationStr = JsonSerializer.Serialize(jsonConfiguration, SerializerOptions);
        var jsonModelsStr = JsonSerializer.Serialize(jsonModels, SerializerOptions);
        var jsonDecksStr = JsonSerializer.Serialize(jsonDecks, SerializerOptions);
        var jsonDeckConfigurationsStr = JsonSerializer.Serialize(jsonDeckConfigurations, SerializerOptions);

        return new(
            id: collection.Id.Value,
            crt: collection.CreationDateTime,
            mod: collection.LastModifiedDateTime,
            scm: collection.SchemaModificationDateTime,
            ver: collection.Version,
            dty: collection.IsDirty ? 1 : 0,
            usn: collection.UpdateSequenceNumber,
            ls: collection.LastSyncDateTime,
            conf: jsonConfigurationStr,
            models: jsonModelsStr,
            decks: jsonDecksStr,
            dconf: jsonDeckConfigurationsStr,
            tags: "{}");
    }

    private static JsonModel ToJson(NoteType noteType)
        => new()
        {
            Id = noteType.Id.Value,
            Name = noteType.Name,
            Fields = noteType.Fields
                .Select((field, ordinal) => (Ordinal: ordinal, Field: field))
                .Select(entry => ToJson(entry.Ordinal, entry.Field))
                .ToArray(),
            CardTemplates = noteType.CardTemplates
                .Select((cardTemplate, ordinal) => (Ordinal: ordinal, CardTemplate: cardTemplate))
                .Select(entry => ToJson(entry.Ordinal, entry.CardTemplate))
                .ToArray(),
            ModificationTime = noteType.ModificationTime,
            LatexPre = noteType.LatexPre,
            LatexPost = noteType.LatexPost,
            LatexSvg = noteType.LatexSvg,
            BrowserSortField = noteType.BrowserSortField,
            DefaultDeckId = noteType.DefaultDeckId != DeckId.Empty
                ? noteType.DefaultDeckId.Value
                : null,
            ModelType = (int)noteType.ModelType,
            UpdateSequenceNumber = noteType.UpdateSequenceNumber,
            LastAddedNoteTags = noteType.LastAddedNoteTags.Select(tag => tag.Value).ToArray(),
            Css = noteType.Styling,
            RequiredFields = [0, "any", new int[] { 0 }]
        };

    private static JsonConfiguration ToJson(Configuration configuration)
        => new()
        {
            ActiveColumns = configuration.ActiveColumns.ToArray(),
            ActiveDecks = configuration.ActiveDecks.Select(deck => (int)deck.Value).ToArray(),
            AddToCurrent = configuration.AddToCurrent,
            CollapseTime = configuration.CollapseTime,
            CreationOffset = configuration.CreationOffset,
            CurrentDeck = (int)configuration.CurrentDeck.Value,
            CurrentModel = configuration.CurrentNoteType.Value,
            DayLearnFirst = configuration.DayLearnFirst,
            DueCounts = configuration.DueCounts,
            EstimateTimes = configuration.EstimateTimes,
            LastUnburied = configuration.LastUnburied,
            NewBury = configuration.NewBury,
            NewSpread = (int)configuration.NewSpread,
            NextPosition = configuration.NextPosition,
            SchedulerVersion = configuration.SchedulerVersion,
            SortBackwards = configuration.SortBackwards,
            SortType = configuration.SortType,
            TimeLimit = configuration.TimeLimit,
        };

    private static JsonDeck ToJson(Deck deck)
        => new()
        {
            Id = deck.Id.Value,
            Name = deck.Name,
            LastModificationTime = deck.LastModificationTime,
            UpdateSequenceNumber = deck.UpdateSequenceNumber,
            NewToday = [deck.NewToday.DaysSinceCreation, deck.NewToday.CardsSeenToday],
            ReviewedToday = [deck.ReviewedToday.DaysSinceCreation, deck.ReviewedToday.CardsSeenToday],
            LearnedToday = [deck.LearnedToday.DaysSinceCreation, deck.LearnedToday.CardsSeenToday],
            TimeToday = [deck.TimeToday.Value1, deck.TimeToday.Value2],
            IsCollapsed = deck.IsCollapsed,
            IsCollapsedInBrowser = deck.IsCollapsedInBrowser,
            Description = deck.Description,
            IsDynamic = deck.IsDynamic ? 1 : 0,
            ConfigurationGroupId = deck.ConfigurationGroupId,
            ExtendedNewCardLimit = deck.ExtendedNewCardLimit,
            ExtendedReviewCardLimit = deck.ExtendedReviewCardLimit,
        };

    private static JsonDeckConfguration ToJson(DeckId deckId, DeckConfiguration deckConfiguration)
    {
        return new()
        {
            AutoplayQuestionAudio = deckConfiguration.AutoplayQuestionAudio,
            IsDynamic = deckConfiguration.IsDynamic,
            LapseCardsConfiguration = LeapsCardsConfigurationToJson(deckConfiguration.LapseCards),
            LastModificationTime = deckConfiguration.LastModificationTime,
            Id = deckId.Value,
            Name = deckConfiguration.Name,
            NewCardsConfiguration = NewCardsConfigurationToJson(deckConfiguration.NewCards),
            ReplayQuestionAudio = deckConfiguration.ReplayQuestionAudio,
            ReviewCardsConfiguration = ReviewCardsConfigurationToJson(deckConfiguration.ReviewCards),
            ShowTimer = deckConfiguration.ShowTimer,
            StopTimerAfterSeconds = deckConfiguration.StopTimerAfterSeconds,
            UpdateSequenceNumber = deckConfiguration.UpdateSequenceNumber,
        };

        #region Helper function(s)

        static JsonLapseCardsConfiguration LeapsCardsConfigurationToJson(
            DeckConfiguration.LapseCardsConfiguration leapsCardsConfiguration)
            => new()
            {
                Delays = leapsCardsConfiguration.Delays.ToArray(),
                LapsedIntervalMultiplierPercent = leapsCardsConfiguration.LapsedIntervalMultiplierPercent,
                LeechAction = (int)leapsCardsConfiguration.LeechAction,
                LeechFailsAllowedCount = leapsCardsConfiguration.LeechFailsAllowedCount,
                MinimumInterfalAfterLeech = leapsCardsConfiguration.MinimumInterfalAfterLeech,
            };

        static JsonNewCardsConfiguration NewCardsConfigurationToJson(
            DeckConfiguration.NewCardsConfiguration newCardsConfiguration)
            => new()
            {
                Bury = newCardsConfiguration.Bury,
                Delays = newCardsConfiguration.Delays.ToArray(),
                InitialEaseFactor = newCardsConfiguration.InitialEaseFactor,
                IntDelays = newCardsConfiguration.IntDelays.ToArray(),
                NewCardsPerDay = newCardsConfiguration.NewCardsPerDay,
                NewCardsShowOrder = (int)newCardsConfiguration.NewCardsShowOrder,
                Separate = newCardsConfiguration.Separate,
            };

        static JsonReviewCardsConfiguration ReviewCardsConfigurationToJson(
            DeckConfiguration.ReviewCardsConfiguration reviewCardsConfiguration)
            => new()
            {
                Bury = reviewCardsConfiguration.Bury,
                CardsToReviewPerDay = reviewCardsConfiguration.CardsToReviewPerDay,
                Ease4 = reviewCardsConfiguration.Ease4,
                Fuzz = reviewCardsConfiguration.Fuzz,
                HardFactor = reviewCardsConfiguration.HardFactor,
                IntervalMultiplicationFactor = reviewCardsConfiguration.IntervalMultiplicationFactor,
                MaximumReviewInterval = reviewCardsConfiguration.MaximumReviewInterval,
                MinSpace = reviewCardsConfiguration.MinSpace,
            };

        #endregion
    }

    private static JsonCardTemplate ToJson(int ordinal, CardTemplate cardTemplate)
        => new()
        {
            TemplateName = cardTemplate.Name,
            TemplateOrdinal = ordinal,
            AnswerFormat = cardTemplate.AnswerFormat,
            QuestionFormat = cardTemplate.QuestionFormat,
            BrowserAnswerFormat = cardTemplate.BrowserAnswerFormat,
            BrowserQuestionFormat = cardTemplate.BrowserQuestionFormat,
            DeckOverrideId = cardTemplate.DeckOverrideId,
            BFont = cardTemplate.BFont,
            BSize = cardTemplate.BSize,
        };

    private static JsonField ToJson(int ordinal, Field field)
        => new()
        {
            FieldName = field.Name,
            FieldNumber = ordinal,
            Description = field.Description,
            Font = field.Font,
            FontSize = field.FontSize,
            IsRightToLeft = field.IsRightToLeft,
            IsSticky = field.IsSticky,
            Media = field.Media.Length > 0
                ? field.Media.ToArray()
                : null,
        };

    #endregion
}
