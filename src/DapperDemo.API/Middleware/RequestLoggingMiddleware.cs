using DapperDemo.DAL;
using DapperDemo.DAL.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace DapperDemo.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string logTrace = context.Request.Headers["LogTrace"];

            if (!DapperSQLManager.IsLoggingEnabled)
            {
                await _next(context);
                return;
            }

            // Enable buffering and capture request
            var requestBody = await FormatRequest(context.Request);

            var originalResponseBody = context.Response.Body;
            await using var newResponseBody = new MemoryStream();
            context.Response.Body = newResponseBody;

            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                var requestBodyerror = await FormatRequest(context.Request);

                // Use ErrorLogger class
                ErrorLogger.LogError(
                    ex,
                    controllerName: context.GetRouteValue("controller")?.ToString(),
                    actionName: context.GetRouteValue("action")?.ToString(),
                    parameters: requestBody,
                    userId: context.User?.Identity?.Name ?? "Anonymous",
                    userIp: context.Connection.RemoteIpAddress?.ToString(),
                    connectionString: AppServicesHelper.Config.DapperConnectinString
                );

                // Set HTTP response
                context.Response.Clear();
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    StatusCode = 500,
                    Message = "An unexpected error occurred.",
                    Details = ex.Message // Optional: Hide in production
                };

                var errorJson = JsonSerializer.Serialize(errorResponse);
                await context.Response.WriteAsync(errorJson);

                return;
            }
            finally
            {
                stopwatch.Stop();

                newResponseBody.Seek(0, SeekOrigin.Begin);
                var responseBodyText = await new StreamReader(newResponseBody).ReadToEndAsync();

                newResponseBody.Seek(0, SeekOrigin.Begin);
                await newResponseBody.CopyToAsync(originalResponseBody);
                context.Response.Body = originalResponseBody;

                var logData = new PerformanceLogData
                {
                    CommandText = context.Request.Path,
                    commandType = "API",
                    RequiredTime = stopwatch.Elapsed,
                    RequiredMs = stopwatch.ElapsedMilliseconds,
                    CommandParameter = requestBody,
                    CommandResponse = responseBodyText,
                    MachineName = Environment.MachineName,
                    DatabaseName = "API"
                };

                DapperDemo.DAL.Logging.SqlLogger.InsertSqlCommandPerformanceLog(logData);
                DapperDemo.DAL.Logging.SqlLogger.SaveLogToFile(logData);
            }
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();
            request.Body.Position = 0;

            using var reader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            string body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }
    }
}
