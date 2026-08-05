using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
                b.Property(b => b.EventId).ValueGeneratedOnAdd();

                b.Property(b => b.DateTime);
                b.Property(b => b.EventType);
                b.Property(b => b.AggregateId);
                b.Property(b => b.AggregateType);
                b.Property(b => b.EventIndex);
                b.Property(b => b.Payload);
                b.Property(b => b.MetadataType).IsRequired(false);
                b.Property(b => b.Metadata).IsRequired(false);

                b.HasIndex(b => b.AggregateId);
                b.HasIndex(e => new { e.AggregateId, e.EventIndex }).IsUnique();

                callback?.Invoke(b);
            });
        }
    }
}
