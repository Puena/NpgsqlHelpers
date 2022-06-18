using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NpgsqlHelpers
{
    public delegate NpgsqlCommand CreateNpgsqlCommandDelegate(object data);

    internal static class ComposeIL
    {
        /// <summary>
        /// Create IL version of function like this 
        /// <code>
        /// public static NpgsqlCommand SomeName(object data)
        /// {
        ///     Person person = (Person)data;
        ///     NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM shops WHERE name=$1");
        ///     command.Parameters.Add(new NpgsqlParameter<string>() { TypedValue = person.Name});
        ///     command.Parameters.Add(new NpgsqlParameter<string>() { TypedValue = person.LastName});
        ///     command.Parameters.Add(new NpgsqlParameter<int>() { TypedValue = person.Age });
        ///     return command;
        /// }
        /// </code>
        /// then return delegate.
        /// </summary>
        /// <param name="sql">Npgsql sql string</param>
        /// <param name="sqlParams">Parameters name</param>
        /// <param name="dataType">Type of parameters data</param>
        /// <returns>Return <see cref="CreateNpgsqlCommandDelegate"/>delegate</returns>
        /// <exception cref="MissingFieldException"></exception>
        /// <exception cref="MissingMethodException"></exception>
        public static CreateNpgsqlCommandDelegate CreateNpgsqlCommand(string sql, string[] sqlParams, Type dataType)
        {
            Type npgsqlCommandType = typeof(NpgsqlCommand);
            PropertyInfo npgsqlCommandParametersPropertyInfo = npgsqlCommandType.GetProperty("Parameters", typeof(NpgsqlParameterCollection))!;
            MethodInfo npgsqlCommandGetParametersInfo = npgsqlCommandParametersPropertyInfo.GetGetMethod()!;
            MethodInfo npgsqlCommandAddParameterInfo = typeof(NpgsqlParameterCollection).GetMethod("Add", new Type[] { typeof(NpgsqlParameter) })!;

            Type DynamicMethodReturnType = npgsqlCommandType;
            Type[] DynamicMethodAttributes = new Type[] { dataType };

            DynamicMethod dm = new DynamicMethod(
                    $"{sql.GetHashCode()}_{dataType.Name}",
                    DynamicMethodReturnType,
                    DynamicMethodAttributes
            );

            ConstructorInfo NpgsqlCommandStringCtor = npgsqlCommandType.GetConstructor(new Type[] { typeof(string) })!;

            ILGenerator il = dm.GetILGenerator();

            il.Emit(OpCodes.Ldstr, sql);
            il.Emit(OpCodes.Newobj, NpgsqlCommandStringCtor);

            foreach (string param in sqlParams)
            {
                PropertyInfo? dataPropertyInfo = dataType.GetProperty(param);
                if (dataPropertyInfo is null) throw new MissingFieldException(param);
                Type dataPropertyType = dataPropertyInfo.GetType();
                MethodInfo? dataPropertyGetMethodInfo = dataPropertyInfo.GetGetMethod();
                if (dataPropertyGetMethodInfo is null) throw new MissingMethodException($"Missing get method of {param}");

                Type npgsqlNpgsqlParameterType = ComposeNpgsqlParamInternal(dataPropertyType);
                ConstructorInfo npgsqlParameterCtor = npgsqlNpgsqlParameterType.GetConstructor(Array.Empty<Type>())!;
                MethodInfo npgsqlParameterSetTypedValueInfo = npgsqlNpgsqlParameterType.GetProperty("TypedValue")!.GetSetMethod()!;

                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Callvirt, npgsqlCommandGetParametersInfo);
                il.Emit(OpCodes.Newobj, npgsqlParameterCtor);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Callvirt, dataPropertyGetMethodInfo);
                il.Emit(OpCodes.Callvirt, npgsqlParameterSetTypedValueInfo);
                il.Emit(OpCodes.Callvirt, npgsqlCommandAddParameterInfo);
                il.Emit(OpCodes.Pop);
            }

            il.Emit(OpCodes.Ret);

            return (CreateNpgsqlCommandDelegate)dm.CreateDelegate(typeof(CreateNpgsqlCommandDelegate));
        }

        public static CreateNpgsqlCommandDelegate CreateNpgsqlCommand(ParsedQuery parsedQuery, Type dataType)
        {
            return CreateNpgsqlCommand(parsedQuery.NpgsqlQuery, parsedQuery.ParamNames, dataType);
        }
        private static Type ComposeNpgsqlParamInternal(Type ty)
        {
            var t = typeof(NpgsqlParameter<>);
            Type[] args = { ty };
            return t.MakeGenericType(args);
        }
    }
}
