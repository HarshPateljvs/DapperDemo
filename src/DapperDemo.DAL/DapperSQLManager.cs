using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static DapperDemo.DAL.DapperSQLHelper;

namespace DapperDemo.DAL
{


    public interface IConnectionString
    {

    }
    public class DapperConnectinString : IConnectionString
    {
    }
    public class OtherDbConectionString : IConnectionString
    {
    }
    public abstract class DapperSQLManager : DapperSQLManager<DapperConnectinString>
    {

    }


    public abstract class DapperSQLManager<T> where T : IConnectionString
    {
        private const int DefaultTimeOut = 30;
        public const bool IsLoggingEnabled = true;
        internal static string ConnectionString
        {
            get
            {
                if (typeof(T) == typeof(OtherDbConectionString))
                    return AppServicesHelper.Config.OtherDbConectionString;
                else
                    return AppServicesHelper.Config.DapperConnectinString;
            }
        }

        static DapperSQLManager()
        {

        }
        protected static IDbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static Task<IEnumerable<TModel>> QueryAsync<TModel>(string storedProcedure, object? parameters = null)
        {
            return ExecuteAndLogAsync(
                (conn, dynParams) => conn.QueryAsync<TModel>(storedProcedure, dynParams, commandType: CommandType.StoredProcedure),
                storedProcedure,
                parameters
            );
        }


        public static Task<TModel?> QueryFirstOrDefaultAsync<TModel>(string storedProcedure, object? parameters = null)
        {
            return ExecuteAndLogAsync(
                (conn, dynParams) => conn.QueryFirstOrDefaultAsync<TModel>(storedProcedure, dynParams, commandType: CommandType.StoredProcedure),
                storedProcedure,
                parameters
            );
        }

        public static Task<TModel> QuerySingleAsync<TModel>(string storedProcedure, object? parameters = null)
        {
            return ExecuteAndLogAsync(
                (conn, dynParams) => conn.QuerySingleAsync<TModel>(storedProcedure, dynParams, commandType: CommandType.StoredProcedure),
                storedProcedure,
                parameters
            );
        }

        public static Task<T> ExecuteScalarAsync<T>(string storedProcedure, object? parameters = null)
        {
            return ExecuteAndLogAsync(
                (conn, dynParams) => conn.ExecuteScalarAsync<T>(storedProcedure, dynParams, commandType: CommandType.StoredProcedure),
                storedProcedure,
                parameters
            );
        }

        public static async Task<bool> ExecuteStepsAsync(List<SpExecutionStep> steps)
        {
            var outputStore = new Dictionary<string, object?>();
            if (steps.Count == 0) return false;

            using var connection = (SqlConnection)CreateConnection();
            await connection.OpenAsync();
            using var transaction = steps.Count > 1 ? connection.BeginTransaction() : null;

            try
            {
                foreach (var step in steps)
                {
                    var dataList = (step.Data is IEnumerable<object> list && step.Data is not string)
                        ? list.ToList()
                        : new List<object> { step.Data };

                    foreach (var item in dataList)
                    {
                        var dynParams = new DynamicParameters();
                        var paramDict = item.ToDictionary();

                        // Apply output mappings
                        if (step.OutputMappings != null)
                        {
                            foreach (var (from, to) in step.OutputMappings)
                            {
                                if (outputStore.TryGetValue(from, out var val))
                                {
                                    paramDict[to] = val;
                                }
                            }
                        }

                        // Add input parameters
                        foreach (var kvp in paramDict)
                            dynParams.Add(kvp.Key, kvp.Value);

                        // Add output parameters
                        if (step.OutputParams != null)
                        {
                            foreach (var outParam in step.OutputParams)
                            {
                                DbType dbType = DapperSQLHelper.InferDbTypeFromObject(item, outParam);
                                dynParams.Add(outParam, dbType: dbType, direction: ParameterDirection.Output, size: 4000);
                            }
                        }
                        await ExecuteAndLogAsync(
                                (conn, dynParams) => conn.ExecuteAsync(step.SpName, dynParams, transaction, commandType: CommandType.StoredProcedure),
                                    step.SpName,
                                    dynParams,
                                    transaction: transaction
                                );
                        // Capture output
                        if (step.OutputParams != null)
                        {
                            foreach (var outParam in step.OutputParams)
                                outputStore[outParam] = dynParams.Get<object?>(outParam);
                        }
                    }
                }

                transaction?.Commit();
                return true;
            }
            catch (SqlException ex)
            {
                Console.WriteLine("SQL Error: " + ex.Message);
                Console.WriteLine("Error Number: " + ex.Number);

                if (ex.Number == 50002)  // Match your custom THROW error number
                {
                    Console.WriteLine("Caught specific business logic error.");
                }
                transaction?.Rollback();
                throw;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                throw;
            }
        }

        #region SQL logging here 

        private static async Task<T> ExecuteAndLogAsync<T>(
    Func<IDbConnection, DynamicParameters, Task<T>> dbOperation,
    string storedProcedure,
    object? parameters,
      IDbTransaction? transaction = null,
    string? connectionStringOverride = null)
        {
            using var connection = transaction == null ? CreateConnection() : null;
            var conn = transaction?.Connection ?? connection!;
            if (conn.State != ConnectionState.Open) conn.Open();

            var dynParams = parameters is DynamicParameters dp ? dp : new DynamicParameters(parameters);

            var watch = Stopwatch.StartNew();
            T result = await dbOperation(conn, dynParams);
            watch.Stop();

            if (IsLoggingEnabled)
            {
                DapperDemo.DAL.Logging.SqlLogger.LogStoredProcedureCall(storedProcedure, dynParams, watch, conn.ConnectionString);
            }

            return result;
        }
        #endregion

    }
}
