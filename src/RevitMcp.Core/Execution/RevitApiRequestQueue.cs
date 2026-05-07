using System.Collections.Concurrent;

namespace RevitMcp.Core.Execution;

public sealed class RevitApiRequestQueue
{
    private readonly ConcurrentQueue<RevitApiRequest> _queue = new();
    private readonly int _maxLength;
    private int _count;

    public RevitApiRequestQueue(int maxLength)
    {
        _maxLength = maxLength;
    }

    public int Count => Volatile.Read(ref _count);

    public bool IsEmpty => Count == 0;

    public bool TryEnqueue(RevitApiRequest request)
    {
        while (true)
        {
            var current = Volatile.Read(ref _count);
            if (current >= _maxLength)
            {
                return false;
            }

            if (Interlocked.CompareExchange(ref _count, current + 1, current) == current)
            {
                _queue.Enqueue(request);
                return true;
            }
        }
    }

    public bool TryDequeue(out RevitApiRequest? request)
    {
        if (_queue.TryDequeue(out request))
        {
            Interlocked.Decrement(ref _count);
            return true;
        }

        request = null;
        return false;
    }
}
