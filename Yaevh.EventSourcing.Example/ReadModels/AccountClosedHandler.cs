using Microsoft.EntityFrameworkCore;
using Yaevh.EventSourcing.Example.Model;
using Yaevh.EventSourcing.Example.Model.Events;

namespace Yaevh.EventSourcing.Example.ReadModels;

internal class AccountClosedHandler : IAggregateEventHandler<AccountAggregate, AccountNumber, AccountClosed>
{
    private readonly BasicReadModelDbContext _dbContext;
    public AccountClosedHandler(BasicReadModelDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(AccountAggregate aggegate, AccountClosed @event, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.ReadModels.SingleOrDefaultAsync(x => x.AccountNumber == aggegate.AccountNumber, cancellationToken);

        if (readModel is null)
            throw new InvalidOperationException($"Read model for account {aggegate.AccountNumber} not found");

        _dbContext.ReadModels.Remove(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
