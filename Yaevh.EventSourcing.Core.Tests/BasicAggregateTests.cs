using FluentAssertions;
using System;
using System.Collections.Generic;

namespace Yaevh.EventSourcing.Core.Tests
{
    public class BasicAggregateTests
    {
        #region Aggregate.Version

        [Fact(DisplayName = "A01. After creating and before applying any Events, Aggregate should have Version = 0")]
        public void After_creating_and_before_applying_any_Events_Aggregate_should_have_Version_0()
        {
            var aggregate = new BasicAggregate(Guid.NewGuid());

            aggregate.Version.Should().Be(0);
        }

        [Fact(DisplayName = "A02. After applying a single Event, Aggregate should have Version = 1")]
        public void After_applying_a_single_Event_Aggregate_should_have_Version_1()
        {
            var aggregate = new BasicAggregate(Guid.NewGuid());

            aggregate.DoSomething("jeden", DateTimeOffset.Now);

            aggregate.Version.Should().Be(1);
        }

        [Fact(DisplayName = "A03. After applying three Events, Aggregate should have Version = 3")]
        public void After_applying_three_Events_Aggregate_should_have_Version_3()
        {
            var aggregate = new BasicAggregate(Guid.NewGuid());

            aggregate.DoSomething("jeden", DateTimeOffset.Now);
            aggregate.DoSomething("dwa", DateTimeOffset.Now);
            aggregate.DoSomething("trzy", DateTimeOffset.Now);

            aggregate.Version.Should().Be(3);
        }

        #endregion Aggregate.Version


        #region Aggregate.UncommittedEvents

        [Fact(DisplayName = "B01. After creating and before applying any Events, Aggregate should contain no UncommittedEvents")]
        public void After_creating_and_before_applying_any_Events_Aggregate_should_contain_no_UncommittedEvents()
        {
            var aggregate = new BasicAggregate(Guid.NewGuid());

            aggregate.UncommittedEvents.Should().BeEmpty();
        }

        [Fact(DisplayName = "B02. After applying a single Event, Aggregate should contain a single UncommittedEvent")]
        public void After_applying_a_single_Event_Aggregate_should_contain_a_single_UncommittedEvent()
        {
            var aggregate = new BasicAggregate(Guid.NewGuid());

            aggregate.DoSomething("jeden", DateTimeOffset.Now);

            aggregate.UncommittedEvents.Should().ContainSingle();
        }

        [Fact(DisplayName = "B03. After applying three Events, Aggregate should contain three UncommittedEvents")]
        public void After_applying_three_Events_Aggregate_should_contain_three_UncommittedEvents()
        {
            var aggregate = new BasicAggregate(Guid.NewGuid());

            aggregate.DoSomething("jeden", DateTimeOffset.Now);
            aggregate.DoSomething("dwa", DateTimeOffset.Now);
            aggregate.DoSomething("trzy", DateTimeOffset.Now);

            aggregate.UncommittedEvents.Should().HaveCount(3);
        }

        #endregion Aggregate.UncommittedEvents



        [Fact(DisplayName = "D01. All events should have AggregateId equal to the current Aggregate's ID")]
        public void All_events_should_have_AggregateId_equal_to_the_current_Aggregates_ID()
        {
            var aggregate = new BasicAggregate(Guid.NewGuid());

            aggregate.DoSomething("jeden", DateTimeOffset.Now);
            aggregate.DoSomething("dwa", DateTimeOffset.Now);
            aggregate.DoSomething("trzy", DateTimeOffset.Now);

            aggregate.UncommittedEvents
                .Select(x => x.Metadata.AggregateId)
                .Should().AllSatisfy(x => x.Should().Be(aggregate.AggregateId));
        }

        [Fact(DisplayName = "D02. Uncommitted events should match raised events")]
        public void Uncommitted_events_should_match_raised_events()
        {
            var aggregate = new BasicAggregate(Guid.NewGuid());

            var now1 = DateTimeOffset.Now;
            var now2 = now1 + TimeSpan.FromMinutes(1);
            var now3 = now2 + TimeSpan.FromHours(24);
            aggregate.DoSomething("jeden", now1);
            aggregate.DoSomething("dwa", now2);
            aggregate.DoSomething("trzy", now3);

            aggregate.UncommittedEvents
                .Should().SatisfyRespectively(
                    jeden => {
                        jeden.Payload.Should().BeOfType<BasicAggregate.BasicEvent>()
                            .Which.Value.Should().Be("jeden");
                        jeden.Metadata.DateTime.Should().Be(now1);
                        jeden.Metadata.EventId.Should().NotBeEmpty();
                        jeden.Metadata.EventName.Should().Be(typeof(BasicAggregate.BasicEvent).AssemblyQualifiedName);
                        jeden.Metadata.AggregateId.Should().Be(aggregate.AggregateId);
                        jeden.Metadata.AggregateName.Should().Be(typeof(BasicAggregate).AssemblyQualifiedName);
                        jeden.Metadata.EventIndex.Should().Be(1);
                    },
                    dwa => {
                        dwa.Payload.Should().BeOfType<BasicAggregate.BasicEvent>()
                            .Which.Value.Should().Be("dwa");
                        dwa.Metadata.DateTime.Should().Be(now2);
                        dwa.Metadata.EventId.Should().NotBeEmpty();
                        dwa.Metadata.EventName.Should().Be(typeof(BasicAggregate.BasicEvent).AssemblyQualifiedName);
                        dwa.Metadata.AggregateId.Should().Be(aggregate.AggregateId);
                        dwa.Metadata.AggregateName.Should().Be(typeof(BasicAggregate).AssemblyQualifiedName);
                        dwa.Metadata.EventIndex.Should().Be(2);
                    },
                    trzy => {
                        trzy.Payload.Should().BeOfType<BasicAggregate.BasicEvent>()
                            .Which.Value.Should().Be("trzy");
                        trzy.Metadata.DateTime.Should().Be(now3);
                        trzy.Metadata.EventId.Should().NotBeEmpty();
                        trzy.Metadata.EventName.Should().Be(typeof(BasicAggregate.BasicEvent).AssemblyQualifiedName);
                        trzy.Metadata.AggregateId.Should().Be(aggregate.AggregateId);
                        trzy.Metadata.AggregateName.Should().Be(typeof(BasicAggregate).AssemblyQualifiedName);
                        trzy.Metadata.EventIndex.Should().Be(3);
                    }
                );
        }
    }
}