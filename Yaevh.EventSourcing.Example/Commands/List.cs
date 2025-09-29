using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using Yaevh.EventSourcing.Example.Model;
using Yaevh.EventSourcing.Example.ReadModels;

namespace Yaevh.EventSourcing.Example.Commands;

public class List
{
    internal class Handler : ICommandHandler
    {
        private readonly BasicReadModelDbContext _dbContext;
        public Handler(BasicReadModelDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task HandleAsync(CancellationToken cancellationToken)
        {
            var readModels = await _dbContext.ReadModels.ToListAsync(cancellationToken);

            var table = new Table();
            table.AddColumn("Account Number");
            table.AddColumn("Owner Name");
            table.AddColumn("Balance");
            table.AddColumn("Currency");
            table.AddColumn("Is Closed");

            foreach (var readModel in readModels)
            {
                table.AddRow(
                    readModel.AccountNumber.ToString(),
                    readModel.OwnerName,
                    readModel.Balance.ToString("F2"),
                    readModel.Currency.ToString(),
                    readModel.IsClosed ? "Yes" : "No"
                );
            }

            AnsiConsole.Write(table);
        }
    }
}
