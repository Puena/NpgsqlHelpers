using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpgsqlHelpers.Interfaces
{
    internal interface ICache<T>
    {
        CacheItem<T>? Get(string key);
        bool TryAdd(CacheItem<T> item, bool force);
        bool Contains(CacheItem<T> item);
        bool Remove(CacheItem<T> item);
        void Clear();
    }
}
