using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnkiNet.DomainModel.Base;

public abstract class Entity<T> : IEntity
    where T : notnull
{
    protected Entity(T id)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
    }

    public T Id { get; }

    object IEntity.Id => Id;
}
