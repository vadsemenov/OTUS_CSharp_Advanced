namespace HighLoadedCache.Services.Abstraction;

public interface ISimpleStore
{
    void Set(string key, byte[] value);
    byte[]? Get(string key);
    void Delete(string key);
    (long setCount, long getCount, long deleteCount) GetStatistics();
}