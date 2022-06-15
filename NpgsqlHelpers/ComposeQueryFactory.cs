using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpgsqlHelpers
{
    internal static class ComposeQueryFactory
    {
        private static readonly char[] _stopChars = new char[] { ' ', ',', ':', ')', '\t', '\n', ';' };
        private static readonly char _atSign = '@';

        /// <summary>
        /// Compose Npgsql query from user query!
        /// </summary>
        /// <param name="initQuery"></param>
        /// <returns>Return <see cref="ParsedQuery">ParsedQuery record</see>.</returns>
        /// <exception cref="ArgumentNullException">Throw exception if <paramref name="initQuery"/> is null</exception>
        internal static ParsedQuery ParseQuery(string initQuery)
        {
            if (string.IsNullOrEmpty(initQuery)) throw new ArgumentNullException(nameof(initQuery), "Query can not be null or empy string!");

            StringBuilder _parsedQuery = new();
            StringBuilder _fieldName = new();
            List<string> _fieldsNames = new();
            bool start = false;
            int position = 0;

            foreach (char c in initQuery)
            {
                if (start)
                {
                    if (_stopChars.Any(sc => sc == c))
                    {
                        _fieldsNames.Add(_fieldName.ToString());
                        _parsedQuery.Append(c);
                        start = false;
                        _fieldName.Clear();
                    }
                    else
                    {
                        _fieldName.Append(c);
                    }
                }
                else if (c == _atSign)
                {
                    start = true;
                    position++;
                    _parsedQuery.Append($"${position}");
                }
                else
                {
                    _parsedQuery.Append(c);
                }
            }

            if (start)
            {
                _fieldsNames.Add(_fieldName.ToString());
                _fieldName.Clear();
            }

            return new ParsedQuery(initQuery, _parsedQuery.ToString(), _fieldsNames.ToArray());
        }
    }
}
