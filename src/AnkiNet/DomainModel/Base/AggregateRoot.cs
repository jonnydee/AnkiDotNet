namespace AnkiNet.DomainModel.Base;

public abstract class AggregateRoot<T>
    : Entity<T>
    where T : notnull
{
    protected AggregateRoot(T id)
        : base(id)
    {
    }

    public bool IsDirty { get; set; }

    protected bool SetPropertyValue<TValue>(
        ref TValue backingField, TValue value, Func<TValue, bool>? validate = null, bool force = true)
    {
        if (validate is not null && validate(value) is false)
            return false;

        if (!force && Equals(backingField, value))
            return false;

        backingField = value;
        IsDirty = true;
        return true;
    }
}
