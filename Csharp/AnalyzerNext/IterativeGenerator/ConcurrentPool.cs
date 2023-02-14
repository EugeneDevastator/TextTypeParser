using System.Collections.Concurrent;

namespace AnalyzerNext;

class ConcurrentPool<T> where T : class
{
    private readonly ConcurrentQueue<T> _queue;
    private readonly Func<T> _objectGenerator;
    private int _count;

    public ConcurrentPool(Func<T> objectGenerator)
    {
        _queue = new ConcurrentQueue<T>();
        _objectGenerator = objectGenerator;
    }

    public T GetObject()
    {
        if (_queue.TryDequeue(out var item))
        {
            return item;
        }

        int currentCount = Interlocked.Increment(ref _count);
        //Console.WriteLine($"Creating object {currentCount}");
        return _objectGenerator();
    }

    public void PutObject(T item)
    {
        _queue.Enqueue(item);
    }
}