using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.EFCore;

/// <summary>
/// A <see cref="DbContext"/> that stores Aggregate Events
/// </summary>
/// <typeparam name="TAggregateId"></typeparam>
public abstract class EventsDbContext<TAggregateId> : DbContext
    where TAggregateId : notnull
{
    public EventsDbContext(DbContextOptions options, IEventSerializer eventSerializer)
        : base(options)
    {
        Events = Set<EventData<TAggregateId>>();
    }

    internal DbSet<EventData<TAggregateId>> Events { get; }


    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddEventData<TAggregateId>();

        OnModelCreatingImpl(modelBuilder);
    }

    protected virtual void OnModelCreatingImpl(ModelBuilder modelBuilder)
    {
    }
}
