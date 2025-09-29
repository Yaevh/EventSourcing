using Spectre.Console;
using Yaevh.EventSourcing.Example.Model;

namespace Yaevh.EventSourcing.Example.Commands;

public class Open
{
    public class Command
    {
        public required AccountNumber AccountNumber { get; init; }
        public required Currency Currency { get; init; }
        public required string OwnerName { get; init; }
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

            var currencyCode = await AnsiConsole.PromptAsync(
                new TextPrompt<string>("Enter currency code (e.g. USD, EUR)")
                    .Validate(input => !string.IsNullOrWhiteSpace(input), "Currency code cannot be empty")
                    .Validate(input => input.All(char.IsLetter), "Currency code must contain only letters")
                    .WithConverter(input => input.ToUpperInvariant())
            );

            var ownerName = await AnsiConsole.PromptAsync(
                new TextPrompt<string>("Enter owner name")
                    .Validate(input => !string.IsNullOrWhiteSpace(input), "Owner name cannot be empty")
            );

            return new Command {
                AccountNumber = new AccountNumber(accountNumber),
                Currency = new Currency(currencyCode),
                OwnerName = ownerName
            };
        }

        protected override async Task ExecuteCommand(Command command, CancellationToken cancellationToken)
        {
            var account = await _aggregateManager.LoadAsync(command.AccountNumber, cancellationToken);

            account.OpenAccount(command.AccountNumber, command.Currency, command.OwnerName, DateTimeOffset.Now);

            await _aggregateManager.CommitAsync(account, cancellationToken);
        }
    }
}
