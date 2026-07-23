using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Yaevh.EventSourcing.Core;
using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.EFCore.Postgres.Tests;

public class EventStoreTests
{
    [Fact(DisplayName = "00. Basic aggregate sanity check")]
    public void AggregateSanityCheck()
    {
        BuildAndCheckBasicAggregate();
    }

    [Fact(DisplayName = "01. Events are stored properly")]
    public async Task CanStoreEvents()
    {
        // Arrange
        var token = CancellationToken.None;

        // TODO use Testcontainers to start a PostgreSQL container for testing
        await using var postgresContainer = new PostgreSqlBuilder().Build();
        await postgresContainer.StartAsync(token);
        var (dbContext, eventStore) = await BuildDbContextAndEventStore(postgresContainer);


        // Act
        var aggregateId = Guid.NewGuid();
        var aggregate = new CalculationAggregate(aggregateId);
        aggregate.Add(5);
        aggregate.Subtract(2);
        aggregate.Multiply(4);
        aggregate.Divide(3);

        await eventStore.StoreAsync(aggregate.UncommittedEvents, token);
        await dbContext.SaveChangesAsync(token);


        // Assert
        var events = await dbContext.Events.OrderBy(x => x.EventIndex).ToListAsync();

        events.Should().HaveCount(4);
        events.Should().SatisfyRespectively(
            first => {
                first.EventIndex.Should().Be(1);
                first.EventName.Should().Be(typeof(CalculationAggregate.AdditionEvent).AssemblyQualifiedName);
            },
            second => {
                second.EventIndex.Should().Be(2);
                second.EventName.Should().Be(typeof(CalculationAggregate.SubtractionEvent).AssemblyQualifiedName);
            },
            third => {
                third.EventIndex.Should().Be(3);
                third.EventName.Should().Be(typeof(CalculationAggregate.MultiplicationEvent).AssemblyQualifiedName);
            },
            fourth => {
                fourth.EventIndex.Should().Be(4);
                fourth.EventName.Should().Be(typeof(CalculationAggregate.DivisionEvent).AssemblyQualifiedName);
            });
    }

    [Fact(DisplayName = "02. Stored events can be retrieved")]
    public async Task CanLoadEvents()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        var token = CancellationToken.None;

        // TODO use Testcontainers to start a PostgreSQL container for testing
        await using var postgresContainer = new PostgreSqlBuilder().Build();
        await postgresContainer.StartAsync(token);
        var (dbContext, eventStore) = await BuildDbContextAndEventStore(postgresContainer);
        
        var aggregateId = Guid.NewGuid();
        var aggregate = new CalculationAggregate(aggregateId);
        aggregate.Add(5);
        aggregate.Subtract(2);
        aggregate.Multiply(4);
        aggregate.Divide(3);

        dbContext.Events.Add(eventStore.ToEventData(aggregate.UncommittedEvents[0]));
        dbContext.Events.Add(eventStore.ToEventData(aggregate.UncommittedEvents[1]));
        dbContext.Events.Add(eventStore.ToEventData(aggregate.UncommittedEvents[2]));
        dbContext.Events.Add(eventStore.ToEventData(aggregate.UncommittedEvents[3]));

        await dbContext.SaveChangesAsync(token);

        // Act
        var aggregateEvents = await eventStore.LoadAsync(aggregateId, token);

        
        // Assert
        aggregateEvents.Should().NotBeNull();
        aggregateEvents.Should().SatisfyRespectively(
            add => {
                add.Payload.Should().BeOfType<CalculationAggregate.AdditionEvent>()
                    .Which.Value.Should().Be(5);
                add.DateTime.Should().BeCloseTo(now, TimeSpan.FromMinutes(1));
                add.EventId.Should().Be(1);
                add.EventName.Should().Be(typeof(CalculationAggregate.AdditionEvent).AssemblyQualifiedName);
                add.AggregateId.Should().Be(aggregateId);
                add.AggregateName.Should().Be(typeof(CalculationAggregate).AssemblyQualifiedName);
                add.EventIndex.Should().Be(1);
            },
            subtract => {
                subtract.Payload.Should().BeOfType<CalculationAggregate.SubtractionEvent>()
                    .Which.Value.Should().Be(2);
                subtract.DateTime.Should().BeCloseTo(now, TimeSpan.FromMinutes(1));
                subtract.EventId.Should().Be(2);
                subtract.EventName.Should().Be(typeof(CalculationAggregate.SubtractionEvent).AssemblyQualifiedName);
                subtract.AggregateId.Should().Be(aggregateId);
                subtract.AggregateName.Should().Be(typeof(CalculationAggregate).AssemblyQualifiedName);
                subtract.EventIndex.Should().Be(2);
            },
            multiply => {
                multiply.Payload.Should().BeOfType<CalculationAggregate.MultiplicationEvent>()
                    .Which.Value.Should().Be(4);
                multiply.DateTime.Should().BeCloseTo(now, TimeSpan.FromMinutes(1));
                multiply.EventId.Should().Be(3);
                multiply.EventName.Should().Be(typeof(CalculationAggregate.MultiplicationEvent).AssemblyQualifiedName);
                multiply.AggregateId.Should().Be(aggregateId);
                multiply.AggregateName.Should().Be(typeof(CalculationAggregate).AssemblyQualifiedName);
                multiply.EventIndex.Should().Be(3);
            },
            divide => {
                divide.Payload.Should().BeOfType<CalculationAggregate.DivisionEvent>()
                    .Which.Value.Should().Be(3);
                divide.DateTime.Should().BeCloseTo(now, TimeSpan.FromMinutes(1));
                divide.EventId.Should().Be(4);
                divide.EventName.Should().Be(typeof(CalculationAggregate.DivisionEvent).AssemblyQualifiedName);
                divide.AggregateId.Should().Be(aggregateId);
                divide.AggregateName.Should().Be(typeof(CalculationAggregate).AssemblyQualifiedName);
                divide.EventIndex.Should().Be(4);
            });
    }


    private static CalculationAggregate BuildAndCheckBasicAggregate()
    {
        var aggregate = new CalculationAggregate(Guid.NewGuid());
        aggregate.Value.Should().Be(0);

        aggregate.Add(5);
        aggregate.Value.Should().Be(5);

        aggregate.Subtract(2);
        aggregate.Value.Should().Be(3);

        aggregate.Multiply(4);
        aggregate.Value.Should().Be(12);

        aggregate.Divide(3);
        aggregate.Value.Should().Be(4);

        return aggregate;
    }

    private async Task<(TestDbContext, DbContextEventStore<TestDbContext, Guid>)>
        BuildDbContextAndEventStore(PostgreSqlContainer postgresContainer)
    {
        var token = CancellationToken.None;

        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
        dbContextOptionsBuilder.UseNpgsql(postgresContainer.GetConnectionString());
        var eventSerializer = new SystemTextJsonEventSerializer();
        var dbContext = new TestDbContext(dbContextOptionsBuilder.Options);
        await dbContext.Database.MigrateAsync(token);

        return (dbContext, new DbContextEventStore<TestDbContext, Guid>(dbContext, eventSerializer));
    }

}
