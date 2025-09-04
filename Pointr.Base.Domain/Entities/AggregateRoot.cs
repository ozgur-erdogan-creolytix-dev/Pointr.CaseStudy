using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pointr.Base.Domain.DomainEvents.Interfaces;

namespace Pointr.Base.Domain.Entities;

public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot() { }

    protected AggregateRoot(TId id)
        : base(id) { }

    public IReadOnlyCollection<IDomainEvent> DomainEvents
    {
        get { return _domainEvents.AsReadOnly(); }
    }

    protected void AddDomainEvent(IDomainEvent @event)
    {
        _domainEvents.Add(@event);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
