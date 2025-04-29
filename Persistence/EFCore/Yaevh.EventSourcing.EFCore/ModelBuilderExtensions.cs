using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.EFCore
{
    public static class ModelBuilderExtensions
    {
        public static void AddEventData<TAggregateId>(
            this ModelBuilder modelBuilder,
            Action<EntityTypeBuilder<EventData<TAggregateId>>>? callback = null)
            where TAggregateId : notnull
        {
            modelBuilder.Entity<EventData<TAggregateId>>(b => {
                b.ToTable("Events");
                b.HasKey(b => b.EventId);

                b.Property(b => b.DateTime);
                b.Property(b => b.EventName);
                b.Property(b => b.AggregateId);
                b.Property(b => b.AggregateName);
                b.Property(b => b.EventIndex);
                b.Property(b => b.Payload);

                b.HasIndex(b => b.AggregateId);
                b.HasIndex(e => new { e.AggregateId, e.EventIndex }).IsUnique();

                callback?.Invoke(b);
            });
        }
    }
}
