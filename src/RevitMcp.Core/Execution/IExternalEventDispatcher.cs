namespace RevitMcp.Core.Execution;

public interface IExternalEventDispatcher
{
    void RequestRaise();

    void OnExecuteStarted();

    void OnExecuteCompleted(bool hasRemainingRequests);
}
