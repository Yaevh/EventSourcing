using Spectre.Console;
using Yaevh.EventSourcing.Example.Model;

namespace Yaevh.EventSourcing.Example.Commands;

public class Close
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
            );

            return new Command {
                AccountNumber = new AccountNumber(accountNumber)
            };
        }

        protected override async Task ExecuteCommand(Command command, CancellationToken cancellationToken)
        {
            var account = await _aggregateManager.LoadAsync(command.AccountNumber, cancellationToken);

            account.CloseAccount(DateTimeOffset.Now);

            await _aggregateManager.CommitAsync(account, cancellationToken);
        }
    }
}
