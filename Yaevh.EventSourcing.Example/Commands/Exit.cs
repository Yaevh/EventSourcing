using Spectre.Console;
using Yaevh.EventSourcing.Example.Model;

namespace Yaevh.EventSourcing.Example.Commands;

public class Exit
{
    internal class Handler : ICommandHandler
    {
        public Task HandleAsync(CancellationToken cancellationToken)
        {
            Environment.Exit(0);

            return Task.CompletedTask;
        }
    }
}
