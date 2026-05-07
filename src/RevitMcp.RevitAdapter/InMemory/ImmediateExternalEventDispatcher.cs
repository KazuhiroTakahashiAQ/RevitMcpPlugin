using RevitMcp.Core.Execution;

namespace RevitMcp.RevitAdapter.InMemory;

public sealed class ImmediateExternalEventDispatcher : IExternalEventDispatcher
{
    private Func<CancellationToken, Task>? _handler;
    private int _pending;

    public void Bind(Func<CancellationToken, Task> handler)
    {
        _handler = handler;
    }

    public void RequestRaise()
    {
        if (Interlocked.Exchange(ref _pending, 1) == 1)
        {
            return;
        }

        if (_handler is null)
        {
            return;
        }

        _ = Task.Run(() => _handler(CancellationToken.None));
    }

    public void OnExecuteStarted()
    {
        Interlocked.Exchange(ref _pending, 0);
    }

    public void OnExecuteCompleted(bool hasRemainingRequests)
    {
        if (hasRemainingRequests)
        {
            RequestRaise();
        }
    }
}
