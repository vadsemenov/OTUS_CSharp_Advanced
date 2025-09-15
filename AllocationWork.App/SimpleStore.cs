namespace AllocationWork.App;

public class SimpleStore
{
    private readonly Dictionary<string, byte[]> _store = new();

    public void Set(string key, byte[] value)
    {
        _store.Add(key, value);
    }

    byte[]? Get(string key)
    {
        return _store.TryGetValue(key, out var value) ? value: null;
    }


    void Delete(string key)
    {
        _store.Remove(key);
    }
}