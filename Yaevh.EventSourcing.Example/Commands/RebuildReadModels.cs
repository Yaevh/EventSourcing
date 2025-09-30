using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using Yaevh.EventSourcing.Example.Model;
using Yaevh.EventSourcing.Example.ReadModels;

namespace Yaevh.EventSourcing.Example.Commands;

public class RebuildReadModels
{
    internal class Handler : ICommandHandler
    {
        private readonly BasicReadModelDbContext _dbContext;
        private readonly IAggregateManager<AccountAggregate, AccountNumber> _aggregateManager;
        private readonly IEventStore<AccountNumber> _eventStore;
        private readonly IServiceProvider _serviceProvider;
        public Handler(
            BasicReadModelDbContext dbContext,
            IAggregateManager<AccountAggregate, AccountNumber> aggregateManager,
            IEventStore<AccountNumber> eventStore,
            IServiceProvider serviceProvider)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _aggregateManager = aggregateManager ?? throw new ArgumentNullException(nameof(aggregateManager));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task HandleAsync(CancellationToken cancellationToken)
        {
            var readModels = await _dbContext.ReadModels.ToListAsync(cancellationToken);

            foreach (var readModel in readModels)
            {
                AnsiConsole.MarkupLine($"Removing read model for account [yellow]{readModel.AccountNumber}[/]...");
                _dbContext.ReadModels.Remove(readModel);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            AnsiConsole.MarkupLine($"[green]Removed {readModels.Count} read models.[/]");


            var accountNumbers = await _eventStore.GetAllAggregateIdsAsync(cancellationToken);

            foreach (var accountNumber in accountNumbers)
            {
                AnsiConsole.MarkupLine($"Rebuilding read model for account [yellow]{accountNumber}[/]...");

                var aggregate = await _aggregateManager.LoadAsync(accountNumber, cancellationToken);
                await RebuildAggregate<AccountAggregate, AccountNumber>(aggregate, cancellationToken);

                AnsiConsole.MarkupLine($"[green]Marked read model for account {accountNumber} for rebuilding.[/]");
            }
        }


        public async Task RebuildAggregate<TAggegate, TAggregateId>(TAggegate aggegate, CancellationToken cancellationToken)
            where TAggegate : IAggregate<TAggregateId>
            where TAggregateId : notnull
        {
            await EventDispatcher.DispatchEvents(aggegate, aggegate.CommittedEvents, _serviceProvider, cancellationToken);
        }
    }
}
