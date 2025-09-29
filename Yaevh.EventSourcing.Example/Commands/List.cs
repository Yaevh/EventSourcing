using Spectre.Console;
using Yaevh.EventSourcing.Example.Model;

namespace Yaevh.EventSourcing.Example.Commands;

public class List
{
    internal class Handler : ICommandHandler
    {
        public Task HandleAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
