using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yaevh.EventSourcing;
using Yaevh.EventSourcing.Core;
using Yaevh.EventSourcing.Example.Model;
using Yaevh.EventSourcing.Persistence;
using Yaevh.EventSourcing.SQLite;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(logging =>
    logging.ClearProviders().AddConsole());

builder.Services
    .AddDefaultAggregateManager()
    .AddSingleton<IAggregateIdSerializer<AccountNumber>, AccountNumber.AggregateIdSerializer>()
    .AddNullPublisher()
    .AddSQLiteEventStore("Data Source=example.sqlite");

using IHost host = builder.Build();

using (var sp = host.Services.CreateScope())
{
    await OpenAccount(sp.ServiceProvider, new AccountNumber("2137"), new Currency("CBL"), "Andrzej Strzelba", CancellationToken.None);
}


static async Task OpenAccount(IServiceProvider sp, AccountNumber accountNumber, Currency currency, string ownerName, CancellationToken cancellationToken)
{
    var aggregateManager = sp.GetRequiredService<IAggregateManager<AccountAggregate, AccountNumber>>();

    var account = await aggregateManager.LoadAsync(accountNumber, cancellationToken);

    account.OpenAccount(accountNumber, currency, ownerName, DateTimeOffset.Now);

    await aggregateManager.CommitAsync(account, cancellationToken);
}
