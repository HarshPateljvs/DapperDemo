using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DapperDemo.DAL
{
    public class SpExecutionStep
    {
        public string SpName { get; set; } = string.Empty;
        public object Data { get; set; } = default!;
        public string[]? OutputParams { get; set; }
        public (string From, string To)[]? OutputMappings { get; set; }

        public SpExecutionStep(
            string spName,
            object data,
            string[]? outputParams = null,
            (string From, string To)[]? outputMappings = null)
        {
            SpName = spName;
            Data = data;
            OutputParams = outputParams;
            OutputMappings = outputMappings;
        }
    }

    public class PerformanceLogData
    {
        public string CommandText { get; set; }
        public string commandType { get; set; }
        public TimeSpan RequiredTime { get; set; }
        public long RequiredMs { get; set; }
        public string CommandParameter { get; set; }
        public string CommandResponse { get; set; }
        public string DatabaseName { get; set; }
        public string MachineName { get; set; }
    }

    public class ErrorLogData
    {
        public string? ErrorMessage { get; set; }
        public string? StackTrace { get; set; }
        public string? Source { get; set; }
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public string? InputParameters { get; set; }
        public string? UserId { get; set; }
        public string? UserIP { get; set; }
        public string? MachineName { get; set; }
        public string? DatabaseName { get; set; }
    }
    public static class DapperSQLHelper
    {

        public static Dictionary<string, object?> ToDictionary(this object? obj)
        {
            if (obj == null)
                return new Dictionary<string, object?>();

            if (obj is Dictionary<string, object?> dict)
                return dict;

            if (obj is ExpandoObject expando)
                return new Dictionary<string, object?>(expando);

            return obj
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(obj));
        }


        public static DbType InferDbTypeFromObject(object obj, string propertyName)
        {
            var propInfo = obj.GetType().GetProperty(propertyName);
            if (propInfo == null) return DbType.Object;

            var type = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;
            return InferDbTypeFromType(type);
        }

        private static DbType InferDbTypeFromType(Type type)
        {
            if (type == typeof(string)) return DbType.String;
            if (type == typeof(int)) return DbType.Int32;
            if (type == typeof(long)) return DbType.Int64;
            if (type == typeof(short)) return DbType.Int16;
            if (type == typeof(bool)) return DbType.Boolean;
            if (type == typeof(DateTime)) return DbType.DateTime;
            if (type == typeof(decimal)) return DbType.Decimal;
            if (type == typeof(double)) return DbType.Double;
            if (type == typeof(float)) return DbType.Single;
            if (type == typeof(Guid)) return DbType.Guid;
            if (type == typeof(byte)) return DbType.Byte;
            if (type == typeof(byte[])) return DbType.Binary;

            return DbType.Object;
        }
    }
}
