using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;

namespace DapperDemo.DAL.Logging
{
    public static class ErrorLogger
    {
        public static void LogError(Exception ex, string? controllerName, string? actionName, string? parameters, string? userId, string? userIp, string? connectionString)
        {
            try
            {
                if (!DapperSQLManager.IsLoggingEnabled)
                    return;

                var logData = new ErrorLogData
                {
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    Source = ex.Source,
                    ControllerName = controllerName,
                    ActionName = actionName,
                    InputParameters = parameters,
                    UserId = userId,
                    UserIP = userIp,
                    MachineName = Environment.MachineName,
                    DatabaseName = !string.IsNullOrEmpty(connectionString)
                        ? new SqlConnectionStringBuilder(connectionString).InitialCatalog
                        : null
                };

                // Log to file and SQL in background
                Thread logThread = new(o => InsertErrorLogToSql(o!));
                logThread.IsBackground = true;
                logThread.Start(logData);

                SaveLogToFile(logData);
            }
            catch
            {
                // Fail silently
            }
        }

        public static void InsertErrorLogToSql(object logObj)
        {
            try
            {
                var d = logObj as ErrorLogData;
                if (d == null) return;

                using var connection = new SqlConnection(AppServicesHelper.Config.DapperConnectinString);
                connection.Open();

                string sql = @"
INSERT INTO mstErrorLog
(
    ErrorMessage, StackTrace, Source, ControllerName, ActionName,
    InputParameters, UserId, UserIP, MachineName, DatabaseName
)
VALUES
(
    @ErrorMessage, @StackTrace, @Source, @ControllerName, @ActionName,
    @InputParameters, @UserId, @UserIP, @MachineName, @DatabaseName
);";

                connection.Execute(sql, d);
            }
            catch
            {
                // Swallow logging error
            }
        }

        public static void SaveLogToFile(ErrorLogData d)
        {
            try
            {
                string logDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ErrorLogs");
                Directory.CreateDirectory(logDir);

                string logPath = Path.Combine(logDir, $"ErrorLogs_{DateTime.Now:yyyyMMdd}.txt");

                var sb = new StringBuilder();
                sb.AppendLine("==================================================");
                sb.AppendLine($"DateTime     : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Database     : {d.DatabaseName}");
                sb.AppendLine($"Controller   : {d.ControllerName}");
                sb.AppendLine($"Action       : {d.ActionName}");
                sb.AppendLine($"UserID       : {d.UserId}");
                sb.AppendLine($"UserIP       : {d.UserIP}");
                sb.AppendLine($"MachineName  : {d.MachineName}");
                sb.AppendLine($"Source       : {d.Source}");
                sb.AppendLine("ErrorMessage :");
                sb.AppendLine(d.ErrorMessage);
                sb.AppendLine("StackTrace   :");
                sb.AppendLine(d.StackTrace);
                sb.AppendLine("Parameters   :");
                sb.AppendLine(d.InputParameters);
                sb.AppendLine("==================================================");
                sb.AppendLine();

                File.AppendAllText(logPath, sb.ToString());
            }
            catch
            {
                // Ignore logging failure
            }
        }
    }
}
