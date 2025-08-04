using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace DapperDemo.DAL.Logging
{
    public static class SqlLogger
    {
        public static void LogStoredProcedureCall(string storedProcedure, DynamicParameters parameters, Stopwatch watch, string connectionString)
        {
            try
            {
                if (!DapperSQLManager.IsLoggingEnabled || storedProcedure.Contains("tmpSQLCommandPerformance"))
                    return;

                string formattedParams = GetFormattedParameters(parameters);
                string fullCommandText = $"{storedProcedure}";

                var logData = new PerformanceLogData
                {
                    CommandText = fullCommandText,
                    commandType = CommandType.StoredProcedure.ToString(),
                    RequiredTime = watch.Elapsed,
                    RequiredMs = watch.ElapsedMilliseconds,
                    CommandParameter = formattedParams,
                    DatabaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog,
                    MachineName = Environment.MachineName
                };

                // Log to file and SQL in background
                Thread logThread = new(o => InsertSqlCommandPerformanceLog(o!));
                logThread.IsBackground = true;
                logThread.Start(logData);

                SaveLogToFile(logData);
            }
            catch
            {
                // Fail silently
            }
        }

        public static void InsertSqlCommandPerformanceLog(object logObj)
        {
            try
            {
                var d = logObj as PerformanceLogData;
                if (d == null) return;

                using var connection = new SqlConnection(AppServicesHelper.Config.DapperConnectinString);
                connection.Open();

                string sql = @"
INSERT INTO tmpSQLCommandPerformance
(
    CommandText, CommandType, RequiredTime, AddDate,
    ComputerName, DataBaseName, CommandParameter,
    RequiredMilliSeconds, IsTransfered, CommandResponse
)
VALUES
(
    @CommandText, @CommandType, @RequiredTime, GETDATE(),
    @ComputerName, @DataBaseName, @CommandParameter,
    @RequiredMilliSeconds, 0, @CommandResponse
);";

                connection.Execute(sql, new
                {
                    CommandText = d.CommandText,
                    CommandType = d.commandType,
                    RequiredTime = d.RequiredTime.ToString(),
                    ComputerName = d.MachineName,
                    DataBaseName = d.DatabaseName,
                    CommandResponse = d.CommandResponse,
                    CommandParameter = d.CommandParameter,
                    RequiredMilliSeconds = d.RequiredMs
                });
            }
            catch
            {
                // Swallow logging error
            }
        }

        public static void SaveLogToFile(PerformanceLogData d)
        {
            try
            {
                string logDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PerformanceLogs");
                Directory.CreateDirectory(logDir);

                string logPath = Path.Combine(logDir, $"SqlLogs_{DateTime.Now:yyyyMMdd}.txt");

                var sb = new StringBuilder();
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine($"DateTime   : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Database   : {d.DatabaseName}");
                sb.AppendLine($"Elapsed    : {d.RequiredTime}");
                sb.AppendLine($"CommandType: {d.commandType}");
                sb.AppendLine("CommandText:");
                sb.AppendLine(d.CommandText);
                sb.AppendLine("Parameters:");
                sb.AppendLine(d.CommandParameter);
                sb.AppendLine("Response:");
                sb.AppendLine(d.CommandResponse);
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine();

                File.AppendAllText(logPath, sb.ToString());
            }
            catch
            {
                // Ignore logging failure
            }
        }

        private static string GetFormattedParameters(DynamicParameters parameters)
        {
            var sb = new StringBuilder();
            var paramNames = parameters?.ParameterNames?.ToList();

            if (paramNames != null)
            {
                foreach (var name in paramNames)
                {
                    object value;
                    try
                    {
                        value = parameters.Get<dynamic>(name);
                    }
                    catch
                    {
                        value = "UNKNOWN";
                    }

                    string formattedValue = value == null ? "NULL" : $"'{value}'";
                    sb.AppendLine($"@{name} = {formattedValue},");
                }

                if (sb.Length > 0)
                    sb.Length -= 3;
            }

            return sb.ToString();
        }
    }
}
