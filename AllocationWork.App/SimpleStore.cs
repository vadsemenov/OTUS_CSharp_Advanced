namespace AllocationWork.App;

public class SimpleStore : IDisposable
{
    private readonly Dictionary<string, byte[]> _store = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private long _setCount;
    private long _getCount;
    private long _deleteCount;

    public void Set(string key, byte[] value)
    {
        _lock.EnterWriteLock();
        try
        {
            _store[key] = value;
            Interlocked.Increment(ref _setCount);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public byte[]? Get(string key)
    {
        _lock.EnterReadLock();
        try
        {
            var value = _store.GetValueOrDefault(key);
            Interlocked.Increment(ref _getCount);
            return value;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Delete(string key)
    {
        _lock.EnterWriteLock();
        try
        {
            _store.Remove(key);
            Interlocked.Increment(ref _deleteCount);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public (long setCount, long getCount, long deleteCount) GetStatistics()
        => (_setCount, _getCount, _deleteCount);

    public void Dispose() => _lock.Dispose();
}