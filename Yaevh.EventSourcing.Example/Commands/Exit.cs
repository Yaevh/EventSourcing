namespace Yaevh.EventSourcing.Example.Commands;

public class Exit
{
    internal class Handler : ICommandHandler
    {
        public Task HandleAsync(CancellationToken cancellationToken)
        {
            // this is basically just a marker class
            //Environment.Exit(0);

            return Task.CompletedTask;
        }
    }
}
