using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpgsqlHelpers
{
    public static class CacheCommand
    {
        #region Fields
        static readonly MemoryCache<CreateNpgsqlCommandDelegate> cache = new();
        #endregion

        #region Methods
        /// <summary>
        /// Get delegate from cache or add it and return.
        /// </summary>
        /// <param name="key">Key is a query.</param>
        /// <param name="data">Parameters data object.</param>
        /// <returns>Return <see cref="NpgsqlCommand"/>NpgsqlCommand.</returns>
        private static NpgsqlCommand GetOrAddNpgsqlCommand(string key, object? data)
        {
            if (data == null)
            {
                return new NpgsqlCommand(key);
            }

            var cacheItem = cache.Get(key);

            if (cacheItem == null)
            {
                var parsedQuery = ComposeQuery.ParseQuery(key);
                var createdDelegate = ComposeIL.CreateNpgsqlCommand(parsedQuery, data.GetType());

                cache.TryAdd(key, createdDelegate);
                cacheItem = cache.Get(key);
            }

            return cacheItem!.Value(data);
        }

        public static NpgsqlCommand GetNpgsqlCommand(string query, object? data)
        {
            return GetOrAddNpgsqlCommand(query, data);
        }

        public static NpgsqlCommand GetNpgsqlCommand(string query, object? data, NpgsqlConnection conn)
        {

            var command = GetOrAddNpgsqlCommand(query, data);
            command.Connection = conn;

            return command;
        }
        #endregion
    }
}
