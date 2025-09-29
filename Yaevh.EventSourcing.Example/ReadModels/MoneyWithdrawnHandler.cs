using Microsoft.EntityFrameworkCore;
using Yaevh.EventSourcing.Example.Model;
using Yaevh.EventSourcing.Example.Model.Events;

namespace Yaevh.EventSourcing.Example.ReadModels;

internal class MoneyWithdrawnHandler : IAggregateEventHandler<AccountAggregate, AccountNumber, MoneyWithdrawn>
{
    private readonly BasicReadModelDbContext _dbContext;
    public MoneyWithdrawnHandler(BasicReadModelDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(AccountAggregate aggegate, MoneyWithdrawn @event, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.ReadModels.SingleOrDefaultAsync(x => x.AccountNumber == aggegate.AccountNumber, cancellationToken);

        if (readModel is null)
            throw new InvalidOperationException($"Read model for account {aggegate.AccountNumber} not found");

        readModel.Balance -= @event.Amount;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
