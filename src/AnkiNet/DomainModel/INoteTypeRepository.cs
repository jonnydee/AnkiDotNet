using AnkiNet.DomainModel.Base;

namespace AnkiNet.DomainModel;

public interface INoteTypeRepository
    : IRepository<NoteType, NoteTypeId>
{
}
