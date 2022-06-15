using NpgsqlHelpers.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpgsqlHelpers
{
    internal class MemoryCache<T>
    {

        private readonly ConcurrentDictionary<string, CacheItem<T>> _cache = new();

        public void Clear()
        {
            _cache.Clear();
        }

        public bool Contains(string key)
        {
            return _cache.ContainsKey(key);
        }

        public CacheItem<T>? Get(string key)
        {
            _cache.TryGetValue(key, out CacheItem<T>? item);
            return item;
        }

        public bool Remove(string key)
        {
            return _cache.TryRemove(key, out _);
        }

        public bool TryAdd(string key, T value, bool force = false)
        {
            if (force)
            {
                _cache[key] = new CacheItem<T>(key, value);
                return true;
            }

            return _cache.TryAdd(key, new CacheItem<T>(key, value));
        }
    }
}
