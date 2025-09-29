using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Yaevh.EventSourcing;
using Yaevh.EventSourcing.Core;
using Yaevh.EventSourcing.Example;
using Yaevh.EventSourcing.Example.Commands;
using Yaevh.EventSourcing.Example.Model;
using Yaevh.EventSourcing.Example.Model.Events;
using Yaevh.EventSourcing.Example.ReadModels;
using Yaevh.EventSourcing.Persistence;
using Yaevh.EventSourcing.SQLite;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(logging =>
    logging.ClearProviders().AddConsole()
        .AddFilter((category, logLevel) => logLevel < LogLevel.Information || category.StartsWith("Microsoft.EntityFramework") ? false : true));

builder.Services
    .AddDefaultAggregateManager()
    .AddSingleton<IAggregateIdSerializer<AccountNumber>, AccountNumber.AggregateIdSerializer>()
    .AddSQLiteEventStore("Data Source=events.sqlite");

builder.Services.AddScoped<IPublisher, Publisher>();
builder.Services.AddScoped<IAggregateEventHandler<AccountAggregate, AccountNumber, AccountOpened>, AccountOpenedHandler>();
builder.Services.AddScoped<IAggregateEventHandler<AccountAggregate, AccountNumber, AccountClosed>, AccountClosedHandler>();
builder.Services.AddScoped<IAggregateEventHandler<AccountAggregate, AccountNumber, MoneyDeposited>, MoneyDepositedHandler>();

builder.Services.AddDbContext<BasicReadModelDbContext>(
    config => config.UseSqlite("Data Source=basicReadModels.sqlite"));

builder.Services.AddTransient<List.Handler>();
builder.Services.AddTransient<Detail.Handler>();
builder.Services.AddTransient<Open.Handler>();
builder.Services.AddTransient<Close.Handler>();
builder.Services.AddTransient<Deposit.Handler>();
builder.Services.AddTransient<Withdraw.Handler>();
builder.Services.AddTransient<Exit.Handler>();

using IHost host = builder.Build();

await host.Services.GetRequiredService<BasicReadModelDbContext>().Database.MigrateAsync(CancellationToken.None);


while (true)
{
    var command = await AnsiConsole.PromptAsync(
        new SelectionPrompt<Type>()
            .Title("Choose your destiny")
            .AddChoices(new[] {
                typeof(List.Handler), typeof(Detail.Handler),
                typeof(Open.Handler), typeof(Close.Handler),
                typeof(Deposit.Handler), typeof(Withdraw.Handler),
                typeof(Exit.Handler)
            })
            .UseConverter(type => type.FullName!
                .Replace("Yaevh.EventSourcing.Example.Commands.", string.Empty)
                .Replace("+Handler", string.Empty)
                .ToLower())
    );

    AnsiConsole.Clear();


    using (var sp = host.Services.CreateScope())
    {
        var commandHandler = (ICommandHandler)sp.ServiceProvider.GetRequiredService(command);

        try
        {
            await commandHandler.HandleAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
    }
}