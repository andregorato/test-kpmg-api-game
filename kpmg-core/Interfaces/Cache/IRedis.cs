using System.Collections.Generic;

namespace kpmg_core.Interfaces.Cache
{
    public interface IRedis<T>
    {
        List<string> GetAllKeys();
        IDictionary<string, T> GetAll<T>(IEnumerable<string> keys);
        T Get<T>(string key);
        bool Remove(string key);
        bool Set<T>(string key, T value);
    }
}
