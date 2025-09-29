using Spectre.Console;
using Yaevh.EventSourcing.Example.Model;

namespace Yaevh.EventSourcing.Example.Commands;

public class Detail
{
    public class Command
    {
        public required AccountNumber AccountNumber { get; init; }
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

            return new Command {
                AccountNumber = new AccountNumber(accountNumber)
            };
        }

        protected override async Task ExecuteCommand(Command command, CancellationToken cancellationToken)
        {
            var account = await _aggregateManager.LoadAsync(command.AccountNumber, cancellationToken);

            if (account.Version == 0)
            {
                AnsiConsole.MarkupLine($"[red]Account {command.AccountNumber} does not exist.[/]");
                return;
            }

            var table = new Table()
                .AddColumn("Property")
                .AddColumn("Value")
                .AddRow("Account Number", account.AccountNumber.ToString())
                .AddRow("Owner Name", account.OwnerName)
                .AddRow("Currency", account.Currency.ToString())
                .AddRow("Balance", $"{account.Balance} {account.Currency}")
                .AddRow("Is Closed", account.IsClosed.ToString())
                .AddRow("Version", account.Version.ToString());
            AnsiConsole.Write(table);
        }
    }
}
