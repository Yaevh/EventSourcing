using Yaevh.EventSourcing.Core;

namespace Yaevh.EventSourcing.Example.Model;

public class AccountAggregate : Aggregate<AccountAggregate, AccountNumber>
{
    public AccountAggregate(AccountNumber aggregateId) : base(aggregateId) { }


    public AccountNumber AccountNumber { get; private set; }
    public string OwnerName { get; private set; }
    public decimal Balance { get; private set; } = 0m;
    public Currency Currency { get; private set; } = new Currency("CBL");
    public bool IsClosed { get; private set; } = false;



    public void OpenAccount(AccountNumber accountNumber, Currency currency, string ownerName, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(accountNumber);
        ArgumentNullException.ThrowIfNull(currency);
        if (string.IsNullOrWhiteSpace(ownerName)) throw new ArgumentException("Owner name cannot be empty", nameof(ownerName));

        if (Version != 0) throw new InvalidOperationException("Account is already opened");

        var @event = new Events.AccountOpened {
            OpenedAt = now,
            AccountNumber = accountNumber,
            OwnerName = ownerName,
            Currency = currency
        };
        RaiseEvent(@event, now);
    }

    public void Deposit(decimal amount, Currency currency, DateTimeOffset now)
    {
        if (IsTransient) throw new InvalidOperationException("Account is not yet opened");

        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive");
        ArgumentNullException.ThrowIfNull(currency);
        if (currency != Currency) throw new ArgumentException("Currency mismatch", nameof(currency));
        if (IsClosed) throw new InvalidOperationException("Cannot deposit to a closed account");

        var @event = new Events.MoneyDeposited {
            Amount = amount,
            Currency = currency
        };
        RaiseEvent(@event, now);
    }

    public void Withdraw(decimal amount, Currency currency, DateTimeOffset now)
    {
        if (IsTransient) throw new InvalidOperationException("Account is not yet opened");

        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive");
        ArgumentNullException.ThrowIfNull(currency);
        if (currency != Currency) throw new ArgumentException("Currency mismatch", nameof(currency));
        if (amount > Balance) throw new InvalidOperationException("Insufficient funds");
        if (IsClosed) throw new InvalidOperationException("Cannot withdraw from a closed account");

        var @event = new Events.MoneyWithdrawn {
            Amount = amount,
            Currency = currency
        };
        RaiseEvent(@event, now);
    }

    public void CloseAccount(DateTimeOffset now)
    {
        if (Version == 0) throw new InvalidOperationException("Account is not opened");

        if (Balance != 0) throw new InvalidOperationException("Account balance must be zero to close the account");
        if (IsClosed) throw new InvalidOperationException("Account is already closed");

        var @event = new Events.AccountClosed { ClosedAt = now };
        RaiseEvent(@event, now);
    }


    #region applying events

    protected override void Apply(AggregateEvent<AccountNumber> aggregateEvent)
    {
        ArgumentNullException.ThrowIfNull(aggregateEvent);

        // Dispatch to the appropriate Apply method based on the event type
        switch (aggregateEvent.Payload)
        {
            case Events.AccountOpened e:
                Apply(e);
                break;
            case Events.MoneyDeposited e:
                Apply(e);
                break;
            case Events.MoneyWithdrawn e:
                Apply(e);
                break;
            case Events.AccountClosed e:
                Apply(e);
                break;
            default:
                throw new InvalidOperationException($"Unknown event type: {aggregateEvent.GetType().FullName}");
        }
    }



    public void Apply(Events.AccountOpened e) 
    {
        AccountNumber = e.AccountNumber;
        OwnerName = e.OwnerName;
        Currency = e.Currency;
        IsClosed = false;
        Balance = 0m;
    }

    public void Apply(Events.MoneyDeposited e) => Balance += e.Amount;

    public void Apply(Events.MoneyWithdrawn e) => Balance -= e.Amount;

    public void Apply(Events.AccountClosed e) => IsClosed = true;

    #endregion
}
