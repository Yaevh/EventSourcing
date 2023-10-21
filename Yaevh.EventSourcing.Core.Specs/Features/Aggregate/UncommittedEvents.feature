Feature: UncommittedEvents

Aggregate.UncommittedEvents collection

Scenario: Newly created Aggregate should have no UncommittedEvents
	Given a newly created aggregate
	Then it should have no uncommitted events

Scenario: After raising a single event from an Aggregate, it should have single uncommitted events
	Given a newly created aggregate
	When an event is raised
	Then it should have 1 uncommitted event

Scenario: After raising three events from an Aggregate, it should have three uncommitted events
	Given a newly created aggregate
	When an event is raised
	Then it should have 1 uncommitted event

Scenario: All uncommitted events should point to the Aggregate
	Given a newly created aggregate
	When an event is raised
	Then uncommitted event should have proper AggregateId