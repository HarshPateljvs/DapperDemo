using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
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

        public static async Task<IEnumerable<TModel>> QueryAsync<TModel>(string storedProcedure, object? parameters = null)
        {
            using var connection = CreateConnection();
            connection.Open();
            return await connection.QueryAsync<TModel>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        }

        public static async Task<TModel?> QueryFirstOrDefaultAsync<TModel>(string storedProcedure, object? parameters = null)
        {
            using var connection = CreateConnection();
            connection.Open();
            return await connection.QueryFirstOrDefaultAsync<TModel>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        }

        public static async Task<TModel> QuerySingleAsync<TModel>(string storedProcedure, object? parameters = null)
        {
            using var connection = CreateConnection();
            connection.Open();
            return await connection.QuerySingleAsync<TModel>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        }

        public static async Task<T> ExecuteScalarAsync<T>(string storedProcedure, object? parameters = null)
        {
            using var connection = CreateConnection();
            connection.Open();
            return await connection.ExecuteScalarAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
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

                        await connection.ExecuteAsync(step.SpName, dynParams, transaction, commandType: CommandType.StoredProcedure);

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
            catch
            {
                transaction?.Rollback();
                throw;
            }
        }



    }
}
