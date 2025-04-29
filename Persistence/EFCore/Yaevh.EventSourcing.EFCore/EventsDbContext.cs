using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.EFCore;

/// <summary>
/// A <see cref="=DbContext"/> that stores Aggregate Events
/// </summary>
/// <typeparam name="TAggregateId"></typeparam>
public class EventsDbContext<TAggregateId> : DbContext
    where TAggregateId : notnull
{
    public EventsDbContext(DbContextOptions options, IEventSerializer eventSerializer)
        : base(options)
    {
        Events = Set<EventData>();
    }

    internal DbSet<EventData> Events { get; }


    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventData>(b => {
            b.ToTable("Events");
            b.HasKey(b => b.EventId);

            b.Property(b => b.DateTime);
            b.Property(b => b.EventName);
            b.Property(b => b.AggregateId);
            b.Property(b => b.AggregateName);
            b.Property(b => b.EventIndex);
            b.Property(b => b.Payload);

            b.HasIndex(b => b.AggregateId);
        });

        OnModelCreatingImpl(modelBuilder);
    }

    protected virtual void OnModelCreatingImpl(ModelBuilder modelBuilder)
    {
    }


    internal record EventData(
        Guid EventId,
        DateTimeOffset DateTime,
        string EventName,
        TAggregateId AggregateId,
        string AggregateName,
        long EventIndex,
        string Payload);
}
