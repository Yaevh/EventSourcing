using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.EFCore.Tests
{
    public class TestDbContext : EventsDbContext<Guid>
    {
        public TestDbContext(DbContextOptions options, IEventSerializer eventSerializer)
            : base(options, eventSerializer)
        {
        }
    }
}
