using Dapper;
using FluentAssertions;
using System.Data;
using System.Data.Common;
using Yaevh.EventSourcing.Persistence;

namespace Yaevh.EventSourcing.SQLite.Tests;

public class EventStoreTests
{
    [Fact(DisplayName = "A01. Database is sane: can be created and queried")]
    public async Task DatabaseCanBeCreatedAndQueried()
    {
        // Arrange
        var connection = new InMemorySqliteConnection();
        var connectionFactory = () => connection;
        var eventSerializer = new SystemTextJsonEventSerializer();

        var eventStore = new EventStore<Guid>(
            connectionFactory,
            eventSerializer,
            new GuidAggregateIdSerializer(),
            new DefaultAggregateTypeNamingStrategy(),
            new DefaultEventTypeNamingStrategy());

        // Act & Assert - should not throw
        var events = await eventStore.LoadAsync(Guid.NewGuid(), CancellationToken.None);

        events.Should().BeEmpty();
    }

    [Fact(DisplayName = "A02. Data can be stored")]
    public async Task StoringTest()
    {
        // Arrange
        var connection = new InMemorySqliteConnection();
        var connectionFactory = () => connection;
        var eventSerializer = new SystemTextJsonEventSerializer();
        var aggregateIdSerializer = new GuidAggregateIdSerializer();

        var aggregate = new BasicAggregate(Guid.NewGuid());
        var now1 = DateTimeOffset.Now;
        var now2 = now1 + TimeSpan.FromMinutes(1);
        var now3 = now2 + TimeSpan.FromHours(24);
        aggregate.DoSomething("jeden", now1);
        aggregate.DoSomething("dwa", now2);
        aggregate.DoSomething("trzy", now3);

        var eventStore = new EventStore<Guid>(
            connectionFactory,
            eventSerializer,
            new GuidAggregateIdSerializer(),
            new DefaultAggregateTypeNamingStrategy(),
            new DefaultEventTypeNamingStrategy());

        // Act
        await eventStore.StoreAsync(aggregate.UncommittedEvents, CancellationToken.None);

        // Assert by querying the DB manually
        const string sql = @"
                SELECT
                    AggregateName, AggregateId, EventName, EventId, EventIndex, DateTime, Payload, MetadataType, Metadata
                FROM Events
                WHERE
                    AggregateId = @AggregateId
                ORDER BY
                    EventIndex ASC";
        var parameters = new { AggregateId = aggregateIdSerializer.Serialize(aggregate.AggregateId) };
        var command = new CommandDefinition(sql, parameters: parameters);
        var results = await connection.QueryAsync<EventStore<Guid>.EventData>(command);

        results.Should().SatisfyRespectively(
            jeden => {
                jeden.Payload.Should().Be(eventSerializer.Serialize(new BasicAggregate.BasicEvent("jeden")));
                DateTimeOffset.Parse(jeden.DateTime, System.Globalization.CultureInfo.InvariantCulture).Should().Be(now1);
                jeden.EventId.Should().Be(1);
                jeden.EventName.Should().Be(typeof(BasicAggregate.BasicEvent).AssemblyQualifiedName);
                jeden.AggregateId.Should().Be(aggregateIdSerializer.Serialize(aggregate.AggregateId));
                jeden.AggregateName.Should().Be(typeof(BasicAggregate).AssemblyQualifiedName);
                jeden.EventIndex.Should().Be(1);
            },
            dwa => {
                dwa.Payload.Should().Be(eventSerializer.Serialize(new BasicAggregate.BasicEvent("dwa")));
                DateTimeOffset.Parse(dwa.DateTime, System.Globalization.CultureInfo.InvariantCulture).Should().Be(now2);
                dwa.EventId.Should().Be(2);
                dwa.EventName.Should().Be(typeof(BasicAggregate.BasicEvent).AssemblyQualifiedName);
                dwa.AggregateId.Should().Be(aggregateIdSerializer.Serialize(aggregate.AggregateId));
                dwa.AggregateName.Should().Be(typeof(BasicAggregate).AssemblyQualifiedName);
                dwa.EventIndex.Should().Be(2);
            },
            trzy => {
                trzy.Payload.Should().Be(eventSerializer.Serialize(new BasicAggregate.BasicEvent("trzy")));
                DateTimeOffset.Parse(trzy.DateTime, System.Globalization.CultureInfo.InvariantCulture).Should().Be(now3);
                trzy.EventId.Should().Be(3);
                trzy.EventName.Should().Be(typeof(BasicAggregate.BasicEvent).AssemblyQualifiedName);
                trzy.AggregateId.Should().Be(aggregateIdSerializer.Serialize(aggregate.AggregateId));
                trzy.AggregateName.Should().Be(typeof(BasicAggregate).AssemblyQualifiedName);
                trzy.EventIndex.Should().Be(3);
            });
    }

    [Fact(DisplayName = "A03. Loaded events should match stored ones")]
    public async Task LoadedEventsShouldMatchStoredOnes()
    {
        // Arrange
        var connection = new InMemorySqliteConnection();
        var connectionFactory = () => connection;
        var eventSerializer = new SystemTextJsonEventSerializer();

        var aggregate = new BasicAggregate(Guid.NewGuid());
        var now1 = DateTimeOffset.Now;
        var now2 = now1 + TimeSpan.FromMinutes(1);
        var now3 = now2 + TimeSpan.FromHours(24);
        aggregate.DoSomething("jeden", now1);
        aggregate.DoSomething("dwa", now2);
        aggregate.DoSomething("trzy", now3);

        var eventStore = new EventStore<Guid>(
            connectionFactory,
            eventSerializer,
            new GuidAggregateIdSerializer(),
            new DefaultAggregateTypeNamingStrategy(),
            new DefaultEventTypeNamingStrategy());

        await eventStore.StoreAsync(aggregate.UncommittedEvents, CancellationToken.None);

        // Act
        var events = await eventStore.LoadAsync(aggregate.AggregateId, CancellationToken.None);

        // Assert
        events.Should().SatisfyRespectively(
            jeden => {
                jeden.Payload.Should().BeOfType<BasicAggregate.BasicEvent>()
                    .Which.Value.Should().Be("jeden");
                jeden.DateTime.Should().Be(now1);
                jeden.EventId.Should().Be(1);
                jeden.EventType.Should().Be(typeof(BasicAggregate.BasicEvent));
                jeden.AggregateId.Should().Be(aggregate.AggregateId);
                jeden.AggregateType.Should().Be(typeof(BasicAggregate));
                jeden.EventIndex.Should().Be(1);
            },
            dwa => {
                dwa.Payload.Should().BeOfType<BasicAggregate.BasicEvent>()
                    .Which.Value.Should().Be("dwa");
                dwa.DateTime.Should().Be(now2);
                dwa.EventId.Should().Be(2);
                dwa.EventType.Should().Be(typeof(BasicAggregate.BasicEvent));
                dwa.AggregateId.Should().Be(aggregate.AggregateId);
                dwa.AggregateType.Should().Be(typeof(BasicAggregate));
                dwa.EventIndex.Should().Be(2);
            },
            trzy => {
                trzy.Payload.Should().BeOfType<BasicAggregate.BasicEvent>()
                    .Which.Value.Should().Be("trzy");
                trzy.DateTime.Should().Be(now3);
                trzy.EventId.Should().Be(3);
                trzy.EventType.Should().Be(typeof(BasicAggregate.BasicEvent));
                trzy.AggregateId.Should().Be(aggregate.AggregateId);
                trzy.AggregateType.Should().Be(typeof(BasicAggregate));
                trzy.EventIndex.Should().Be(3);
            });
    }

    [Fact(DisplayName = "A04. Events are stored inside a transaction")]
    public async Task EventsAreStoredInTransaction()
    {
        // Arrange
        var innerConnection = new InMemorySqliteConnection();
        var connection = new FailAfterNDbConnection(innerConnection, allowedCommands: 2);
        var connectionFactory = () => connection;
        var eventSerializer = new SystemTextJsonEventSerializer();

        var aggregate = new BasicAggregate(Guid.NewGuid());
        var now1 = DateTimeOffset.Now;
        var now2 = now1 + TimeSpan.FromMinutes(1);
        var now3 = now2 + TimeSpan.FromHours(24);
        aggregate.DoSomething("jeden", now1);
        aggregate.DoSomething("dwa", now2);
        aggregate.DoSomething("trzy", now3);

        var eventStore = new EventStore<Guid>(
            connectionFactory,
            eventSerializer,
            new GuidAggregateIdSerializer(),
            new DefaultAggregateTypeNamingStrategy(),
            new DefaultEventTypeNamingStrategy());

        // Act
        await eventStore.Awaiting(eventStore => eventStore.StoreAsync(aggregate.UncommittedEvents, CancellationToken.None))
            .Should().ThrowAsync<TimeoutException>()
            .Where(ex => ex.Message.StartsWith("Command execution limit exceeded"))
            .Where(ex => ex.Message.Contains("INSERT INTO"));

        // Assert
        innerConnection.QuerySingle<int>("SELECT COUNT(*) FROM Events").Should().Be(0);
    }

    public sealed class FailAfterNDbConnection : DbConnection
    {
        private readonly DbConnection _inner;
        private readonly int _allowedCommands;
        private int _executedCommands;

        public FailAfterNDbConnection(DbConnection inner, int allowedCommands)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _allowedCommands = allowedCommands;
        }

        protected override DbCommand CreateDbCommand()
        {
            var innerCmd = _inner.CreateCommand();
            return new FailAfterNDbCommand((DbCommand)innerCmd, this);
        }
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return _inner.BeginTransaction(isolationLevel);
        }

        internal void CountAndThrowIfExceeded(DbCommand command)
        {
            _executedCommands++;
            if (_executedCommands > _allowedCommands)
                throw new TimeoutException(
                    $"Command execution limit exceeded while executing {command.CommandText}. Allowed: {_allowedCommands}, Executed: {_executedCommands}");
        }

        #region IDbConnection passthrough members
        public override string ConnectionString { get => _inner.ConnectionString; set => _inner.ConnectionString = value; }
        public override string Database => _inner.Database;
        public override string DataSource => _inner.DataSource;
        public override string ServerVersion => _inner.ServerVersion;
        public override ConnectionState State => _inner.State;
        public override void ChangeDatabase(string databaseName) => _inner.ChangeDatabase(databaseName);
        public override void Close() => _inner.Close();
        public override void Open() => _inner.Open();
        #endregion

        public sealed class FailAfterNDbCommand : DbCommand
        {
            private readonly DbCommand _inner;
            private readonly FailAfterNDbConnection _owner;

            public FailAfterNDbCommand(DbCommand inner, FailAfterNDbConnection owner)
            {
                _inner = inner;
                _owner = owner;
            }

            private void CountAndThrowIfExceeded()
                => _owner.CountAndThrowIfExceeded(this);

            public override int ExecuteNonQuery()
            {
                CountAndThrowIfExceeded();
                return _inner.ExecuteNonQuery();
            }

            public override object? ExecuteScalar()
            {
                CountAndThrowIfExceeded();
                return _inner.ExecuteScalar();
            }

            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            {
                CountAndThrowIfExceeded();
                return _inner.ExecuteReader();
            }

            public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
            {
                CountAndThrowIfExceeded();
                return _inner.ExecuteNonQueryAsync(cancellationToken);
            }

            public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
            {
                CountAndThrowIfExceeded();
                return _inner.ExecuteScalarAsync(cancellationToken);
            }


            #region DbCommand passthrough
            public override string CommandText { get => _inner.CommandText; set => _inner.CommandText = value; }
            public override int CommandTimeout { get => _inner.CommandTimeout; set => _inner.CommandTimeout = value; }
            public override CommandType CommandType { get => _inner.CommandType; set => _inner.CommandType = value; }
            protected override DbConnection? DbConnection { get => _inner.Connection; set => _inner.Connection = value; }
            protected override DbTransaction? DbTransaction { get => _inner.Transaction; set => _inner.Transaction = value; }
            public override bool DesignTimeVisible { get => _inner.DesignTimeVisible; set => _inner.DesignTimeVisible = value; }
            public override UpdateRowSource UpdatedRowSource { get => _inner.UpdatedRowSource; set => _inner.UpdatedRowSource = value; }
            protected override DbParameter CreateDbParameter() => _inner.CreateParameter();
            protected override void Dispose(bool disposing) => _inner.Dispose();
            public override void Cancel() => _inner.Cancel();
            public override void Prepare() => _inner.Prepare();
            protected override DbParameterCollection DbParameterCollection => _inner.Parameters;
            #endregion
        }
    }
}