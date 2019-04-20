namespace EasyCaching.InMemory
{
    using EasyCaching.Core;
    using System;
    using System.Collections.Generic;

    public interface IInMemoryCaching
    {
        string ProviderName { get; }
        int GetCount(string prefix = "");
        CacheValue<T> Get<T>(string key);

        object Get(string key);
        IDictionary<string, CacheValue<T>> GetByPrefix<T>(string key);
        bool Add<T>(string key, T value, TimeSpan? expiresIn = null);
        bool Set<T>(string key, T value, TimeSpan? expiresIn = null);
        bool Exists(string key);
        int RemoveAll(IEnumerable<string> keys = null);
        bool Remove(string key);
        int RemoveByPrefix(string prefix);
        IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> keys);
        int SetAll<T>(IDictionary<string, T> values, TimeSpan? expiresIn = null);
        bool Replace<T>(string key, T value, TimeSpan? expiresIn = null);
        void Clear(string prefix = "");
        TimeSpan GetExpiration(string key);
    }
}
