using AnkiNet.DomainModel.Base;

namespace AnkiNet.DomainModel;

public interface INoteRepository
    : IRepository<Note, NoteId>
{
}
