using Yaevh.EventSourcing.Example.Model;
using Yaevh.EventSourcing.Example.Model.Events;

namespace Yaevh.EventSourcing.Example.ReadModels;

internal class AccountOpenedHandler : IAggregateEventHandler<AccountAggregate, AccountNumber, AccountOpened>
{
    private readonly BasicReadModelDbContext _dbContext;
    public AccountOpenedHandler(BasicReadModelDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(AccountAggregate aggegate, AccountOpened @event, CancellationToken cancellationToken)
    {
        var readModel = new BasicAccountReadModel { 
            AccountNumber = aggegate.AccountNumber, 
            OwnerName = @event.OwnerName,
            Balance = 0m,
            Currency = @event.Currency,
            IsClosed = false
        };

        _dbContext.ReadModels.Add(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
