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
        private readonly IPublisher _publisher;
        public Handler(
            BasicReadModelDbContext dbContext,
            IAggregateManager<AccountAggregate, AccountNumber> aggregateManager,
            IPublisher publisher)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _aggregateManager = aggregateManager ?? throw new ArgumentNullException(nameof(aggregateManager));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
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


            foreach (var readModel in readModels)
            {
                AnsiConsole.MarkupLine($"Rebuilding read model for account [yellow]{readModel.AccountNumber}[/]...");

                var aggregate = await _aggregateManager.LoadAsync(readModel.AccountNumber, cancellationToken);
                foreach (var @event in aggregate.AllEvents)
                    await _publisher.Publish(aggregate, @event, cancellationToken);

                AnsiConsole.MarkupLine($"[green]Marked read model for account {readModel.AccountNumber} for rebuilding.[/]");
            }
        }
    }
}
