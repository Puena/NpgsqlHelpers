using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpgsqlHelpers
{
    /// <summary>
    /// Represent parsed string object
    /// </summary>
    /// <param name="InitQuery">User defined query, like: SELECT * FROM shops WHERE Id = @Id</param>
    /// <param name="NpgsqlQuery">Parsed query for npgsql format, like: SELECT * FROM shops WHERE Id = $1</param>
    /// <param name="ParamNames">Parameters name with strict order, like: ["Id"]</param>
    internal record ParsedQuery(string InitQuery, string NpgsqlQuery, string[] ParamNames);
}
