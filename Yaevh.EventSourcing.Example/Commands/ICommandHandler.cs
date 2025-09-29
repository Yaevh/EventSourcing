namespace Yaevh.EventSourcing.Example.Commands;

public interface ICommandHandler
{
    Task HandleAsync(CancellationToken cancellationToken);
}

public abstract class CommandHandlerBase<TCommand> : ICommandHandler
{
    public async Task HandleAsync(CancellationToken cancellationToken)
    {
        var settings = await BuildCommand(cancellationToken);
        await ExecuteCommand(settings, cancellationToken);
    }

    protected abstract Task<TCommand> BuildCommand(CancellationToken cancellationToken);
    protected abstract Task ExecuteCommand(TCommand settings, CancellationToken cancellationToken);

}
