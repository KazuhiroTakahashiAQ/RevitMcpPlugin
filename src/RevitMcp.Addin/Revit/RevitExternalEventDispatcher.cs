using Autodesk.Revit.UI;
using RevitMcp.Core.Execution;

namespace RevitMcp.Addin.Revit;

public sealed class RevitExternalEventDispatcher : IExternalEventDispatcher
{
    private readonly ExternalEvent _externalEvent;
    private int _pending;

    public RevitExternalEventDispatcher(ExternalEvent externalEvent)
    {
        _externalEvent = externalEvent;
    }

    public void RequestRaise()
    {
        if (Interlocked.Exchange(ref _pending, 1) == 1)
        {
            return;
        }

        _externalEvent.Raise();
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
