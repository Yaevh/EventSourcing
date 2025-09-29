using Spectre.Console;
using Yaevh.EventSourcing.Example.Model;

namespace Yaevh.EventSourcing.Example.Commands;

public class Deposit
{
    public class Command
    {
        public required AccountNumber AccountNumber { get; init; }
        public required decimal Amount { get; init; }
        public required Currency Currency { get; init; }
    }

    internal class Handler : CommandHandlerBase<Command>
    {
        private readonly IAggregateManager<AccountAggregate, AccountNumber> _aggregateManager;
        public Handler(IAggregateManager<AccountAggregate, AccountNumber> aggregateManager)
        {
            _aggregateManager = aggregateManager ?? throw new ArgumentNullException(nameof(aggregateManager));
        }


        protected override async Task<Command> BuildCommand(CancellationToken cancellationToken)
        {
            var accountNumber = await AnsiConsole.PromptAsync(
                new TextPrompt<string>("Enter account number")
                    .Validate(input => !string.IsNullOrWhiteSpace(input), "Account number cannot be empty")
                    .Validate(input => input.Length <= 20, "Account number cannot be longer than 20 characters")
            );

            var amount = await AnsiConsole.PromptAsync(
                new TextPrompt<decimal>("Enter amount")
            );

            var currencyCode = await AnsiConsole.PromptAsync(
                new TextPrompt<string>("Enter currency code (e.g. USD, EUR)")
                    .Validate(input => !string.IsNullOrWhiteSpace(input), "Currency code cannot be empty")
                    .Validate(input => input.All(char.IsLetter), "Currency code must contain only letters")
                    .WithConverter(input => input.ToUpperInvariant())
            );


            return new Command {
                AccountNumber = new AccountNumber(accountNumber),
                Currency = new Currency(currencyCode),
                Amount = amount
            };
        }

        protected override async Task ExecuteCommand(Command command, CancellationToken cancellationToken)
        {
            var account = await _aggregateManager.LoadAsync(command.AccountNumber, cancellationToken);

            account.Deposit(command.Amount, command.Currency, DateTimeOffset.Now);

            await _aggregateManager.CommitAsync(account, cancellationToken);
        }
    }
}
