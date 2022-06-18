using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpgsqlHelpers
{
    public static class CachCommand
    {
        #region Fields
        static readonly MemoryCache<CreateNpgsqlCommandDelegate> cache = new();
        #endregion

        private static NpgsqlCommand ComposeNpgsqlCommand(string key, object? data)
        {
            if (data == null)
            {
                return new NpgsqlCommand(key);
            }

            var command = cache.Get(key);

            if (command == null)
            {
                var parsedQuery = ComposeQuery.ParseQuery(key);
                var createdDelegate = ComposeIL.CreateNpgsqlCommand(parsedQuery, data.GetType());

                cache.TryAdd(key, createdDelegate);
                command = cache.Get(key);
            }

            return command!.Value(data);
        }

        public static NpgsqlCommand GetNpgsqlCommand(string key, object? data)
        {
            return ComposeNpgsqlCommand(key, data);
        }

        public static NpgsqlCommand GetNpgsqlCommand(string key, object? data, NpgsqlConnection conn)
        {

            var command = ComposeNpgsqlCommand(key, data);
            command.Connection = conn;

            return command;
        }
    }
}
