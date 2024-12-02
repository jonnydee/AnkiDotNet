
using AnkiNet.DomainModel.Base;

namespace AnkiNet.DomainModel;

public interface IDeckRepository
    : IRepository<Deck, DeckId>
{
}
