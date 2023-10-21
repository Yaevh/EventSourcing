using System;
using TechTalk.SpecFlow;

namespace Yaevh.EventSourcing.Core.Specs.StepDefinitions
{
    [Binding]
    public class AggregateStepDefinitions
    {
        protected BasicAggregate _aggregate = new(Guid.NewGuid());


        #region Given
        [Given(@"a newly created aggregate")]
        public void GivenANewlyCreatedAggregate()
        {
            _aggregate = new BasicAggregate(Guid.NewGuid());
        }
        #endregion


        #region When

        [When(@"an event is raised")]
        public void WhenAnEventIsRaised()
        {
            _aggregate.DoSomething(string.Empty, DateTimeOffset.Now);
        }

        [When(@"an event is raised with value '(.*)'")]
        public void WhenAnEventIsRaisedWithValue(string value)
        {
            _aggregate.DoSomething(value, DateTimeOffset.Now);
        }

        [When(@"another event is raised")]
        public void WhenAnotherEventIsRaised()
        {
            _aggregate.DoSomething(string.Empty, DateTimeOffset.Now);
        }

        [When(@"another event is raised with value '(.*)'")]
        public void WhenAnotherEventIsRaisedWithValue(string value)
        {
            _aggregate.DoSomething(value, DateTimeOffset.Now);
        }

        [When(@"an event is applied with value '(.*)'")]
        public void WhenAnEventIsAppliedWithValue(string value)
        {
            _aggregate.DoSomething(value, DateTimeOffset.Now);
        }

        [When(@"another event is applied with value '(.*)'")]
        public void WhenAnotherEventIsAppliedWithValue(string value)
        {
            _aggregate.DoSomething(value, DateTimeOffset.Now);
        }

        #endregion


        #region Then

        [Then(@"the version should be (.*)")]
        public void ThenTheVersionShouldBe(int version)
        {
            _aggregate.Version.Should().Be(version);
        }

        [Then(@"it should have no uncommitted events")]
        public void ThenItShouldHaveNoUncommittedEvents()
        {
            _aggregate.UncommittedEvents.Should().BeEmpty();
        }

        [Then(@"it should have (.*) uncommitted event")]
        public void ThenItShouldHaveUncommittedEvent(int eventCount)
        {
            _aggregate.UncommittedEvents.Should().HaveCount(eventCount);
        }

        [Then(@"uncommitted event should have proper AggregateId")]
        public void ThenUncommittedEventShouldHaveProperAggregateId()
        {
            _aggregate.UncommittedEvents
                .Select(x => x.Metadata.AggregateId)
                .Should().AllSatisfy(x =>
                    x.Should().Be(_aggregate.AggregateId));
        }

        #endregion
    }
}
