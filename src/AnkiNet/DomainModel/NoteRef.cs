namespace AnkiNet.DomainModel;

public readonly record struct NoteRef(DeckId DeckId, long NoteId);
