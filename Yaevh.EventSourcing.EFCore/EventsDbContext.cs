using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yaevh.EventSourcing.Core;

namespace Yaevh.EventSourcing.EFCore
{
    internal class EventsDbContext : DbContext
    {
        public EventsDbContext(DbContextOptions options)
            : base(options)
        {
            Events = Set<IEvent>();
        }

        public DbSet<IEvent> Events { get; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IEvent>(b => {
                b.ToTable("Events");
                b.HasKey("Id");
            });
            throw new NotImplementedException();
        }
    }
}
