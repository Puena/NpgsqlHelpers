using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpgsqlHelpers
{
    public class CachedCommand
    {
        private static readonly MemoryCache<CreateNpgsqlCommandDelegate> cache = new();

        private NpgsqlCommand npgsqlCommand;
        public object? Data;

        public CachedCommand(string query, object? data)
        {
            var c_command = GetOrAddCache(query, data);

        }

        private NpgsqlCommand GetOrAddCache(string key, object? data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Data cannot be null");
            }

            var command = cache.Get(key);

            if (command == null)
            {
                var parsedQuery = ComposeQuery.ParseQuery(key);
                var createdDelegate = ComposeIL.CreateNpgsqlCommand(parsedQuery, data.GetType());
                cache.TryAdd(key, createdDelegate);
                command = cache.Get(key);
            }

            return command(data);
        }
    }
}
