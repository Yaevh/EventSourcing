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

namespace Yaevh.EventSourcing.EFCore.Tests;

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
                add.Metadata.DateTime.Should().BeCloseTo(now, TimeSpan.FromMinutes(1));
                add.Metadata.EventId.Should().NotBeEmpty();
                add.Metadata.EventName.Should().Be(typeof(CalculationAggregate.AdditionEvent).AssemblyQualifiedName);
                add.Metadata.AggregateId.Should().Be(aggregateId);
                add.Metadata.AggregateName.Should().Be(typeof(CalculationAggregate).AssemblyQualifiedName);
                add.Metadata.EventIndex.Should().Be(1);
            },
            subtract => {
                subtract.Payload.Should().BeOfType<CalculationAggregate.SubtractionEvent>()
                    .Which.Value.Should().Be(2);
                subtract.Metadata.DateTime.Should().BeCloseTo(now, TimeSpan.FromMinutes(1));
                subtract.Metadata.EventId.Should().NotBeEmpty();
                subtract.Metadata.EventName.Should().Be(typeof(CalculationAggregate.SubtractionEvent).AssemblyQualifiedName);
                subtract.Metadata.AggregateId.Should().Be(aggregateId);
                subtract.Metadata.AggregateName.Should().Be(typeof(CalculationAggregate).AssemblyQualifiedName);
                subtract.Metadata.EventIndex.Should().Be(2);
            },
            multiply => {
                multiply.Payload.Should().BeOfType<CalculationAggregate.MultiplicationEvent>()
                    .Which.Value.Should().Be(4);
                multiply.Metadata.DateTime.Should().BeCloseTo(now, TimeSpan.FromMinutes(1));
                multiply.Metadata.EventId.Should().NotBeEmpty();
                multiply.Metadata.EventName.Should().Be(typeof(CalculationAggregate.MultiplicationEvent).AssemblyQualifiedName);
                multiply.Metadata.AggregateId.Should().Be(aggregateId);
                multiply.Metadata.AggregateName.Should().Be(typeof(CalculationAggregate).AssemblyQualifiedName);
                multiply.Metadata.EventIndex.Should().Be(3);
            },
            divide => {
                divide.Payload.Should().BeOfType<CalculationAggregate.DivisionEvent>()
                    .Which.Value.Should().Be(3);
                divide.Metadata.DateTime.Should().BeCloseTo(now, TimeSpan.FromMinutes(1));
                divide.Metadata.EventId.Should().NotBeEmpty();
                divide.Metadata.EventName.Should().Be(typeof(CalculationAggregate.DivisionEvent).AssemblyQualifiedName);
                divide.Metadata.AggregateId.Should().Be(aggregateId);
                divide.Metadata.AggregateName.Should().Be(typeof(CalculationAggregate).AssemblyQualifiedName);
                divide.Metadata.EventIndex.Should().Be(4);
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
        var dbContext = new TestDbContext(dbContextOptionsBuilder.Options, eventSerializer);
        await dbContext.Database.MigrateAsync(token);

        return (dbContext, new DbContextEventStore<TestDbContext, Guid>(dbContext, eventSerializer));
    }

}
