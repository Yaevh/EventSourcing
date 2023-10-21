Feature: Aggregate version
Aggregate.Version specification

Link to a feature: [Aggregate version](Yaevh.EventSourcing.Core.Specs/Features/Aggregate/AggregateVersion.feature)
***Further read***: **[Learn more about how to generate Living Documentation](https://docs.specflow.org/projects/specflow-livingdoc/en/latest/LivingDocGenerator/Generating-Documentation.html)**

Scenario: Newly created aggregate's version should be 0
	Given a newly created aggregate
	Then the version should be 0

Scenario: Aggregate that raised a single event should have version = 1
	Given a newly created aggregate
	When an event is raised with value 'jeden'
	Then the version should be 1

Scenario: Aggregate that raised three events should have version = 3
	Given a newly created aggregate
	When an event is raised with value 'jeden'
	And another event is raised with value 'dwa'
	And another event is raised with value 'trzy'
	Then the version should be 3

Scenario: Aggregate with a single event applied should have version = 1
	Given a newly created aggregate
	When an event is applied with value 'jeden'
	Then the version should be 1

Scenario: Aggregate with three events applied should have version = 3
	Given a newly created aggregate
	When an event is applied with value 'jeden'
	And another event is applied with value 'dwa'
	And another event is applied with value 'trzy'
	Then the version should be 3