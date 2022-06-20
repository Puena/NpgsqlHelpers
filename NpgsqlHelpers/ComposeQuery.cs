using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpgsqlHelpers
{
    public static class ComposeQuery
    {
        private static readonly char[] stopChars = new char[] { ' ', ',', ':', ')', '\t', '\n', ';' };
        private static readonly char atSign = '@';

        /// <summary>
        /// Compose Npgsql query from user query!
        /// </summary>
        /// <param name="initQuery">Raw user string, like: SELECT * FROM shops WHERE Id = @Id"</param>
        /// <returns>Return <see cref="ParsedQuery"> ParsedQuery </see>record.</returns>
        /// <exception cref="ArgumentNullException">Throw exception if <paramref name="initQuery"/> is null</exception>
        public static ParsedQuery ParseQuery(string initQuery)
        {
            if (string.IsNullOrEmpty(initQuery)) throw new ArgumentNullException(nameof(initQuery), "Query can not be null or empy string!");

            StringBuilder parsedQuery = new();
            StringBuilder fieldName = new();
            List<string> fieldsNames = new();

            bool start = false;
            int position = 0;

            foreach (char c in initQuery.AsSpan())
            {
                if (start)
                {
                    if (stopChars.Any(sc => sc == c))
                    {
                        fieldsNames.Add(fieldName.ToString());
                        parsedQuery.Append(c);
                        start = false;
                        fieldName.Clear();
                    }
                    else
                    {
                        fieldName.Append(c);
                    }
                }
                else if (c == atSign)
                {
                    start = true;
                    position++;
                    parsedQuery.Append($"${position}");
                }
                else
                {
                    parsedQuery.Append(c);
                }
            }

            if (start)
            {
                fieldsNames.Add(fieldName.ToString());
                fieldName.Clear();
            }

            return new ParsedQuery(initQuery, parsedQuery.ToString(), fieldsNames.ToArray());
        }
    }
}
