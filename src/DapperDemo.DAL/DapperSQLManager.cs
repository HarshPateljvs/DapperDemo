using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        // ExecuteAsync - Insert/Update/Delete
        public static async Task<int> ExecuteAsync(
            string storedProcedure,
            object? parameters = null,
            SqlTransaction? externalTransaction = null)
        {
            using var connection = new SqlConnection(ConnectionString);
            if (externalTransaction != null)
            {
                return await connection.ExecuteAsync(storedProcedure, parameters, externalTransaction, commandType: CommandType.StoredProcedure);
            }
            else
            {
                await connection.OpenAsync();
                return await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        // QueryAsync - List
        public static async Task<IEnumerable<TModel>> QueryAsync<TModel>(
            string storedProcedure,
            object? parameters = null,
            SqlTransaction? externalTransaction = null)
        {
            using var connection = new SqlConnection(ConnectionString);
            if (externalTransaction != null)
            {
                return await connection.QueryAsync<TModel>(storedProcedure, parameters, externalTransaction, commandType: CommandType.StoredProcedure);
            }
            else
            {
                await connection.OpenAsync();
                return await connection.QueryAsync<TModel>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        // QueryFirstOrDefaultAsync - Single Item or null
        public static async Task<TModel?> QueryFirstOrDefaultAsync<TModel>(
            string storedProcedure,
            object? parameters = null,
            SqlTransaction? externalTransaction = null)
        {
            using var connection = new SqlConnection(ConnectionString);
            if (externalTransaction != null)
            {
                return await connection.QueryFirstOrDefaultAsync<TModel>(storedProcedure, parameters, externalTransaction, commandType: CommandType.StoredProcedure);
            }
            else
            {
                await connection.OpenAsync();
                return await connection.QueryFirstOrDefaultAsync<TModel>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        // QuerySingleAsync - Must return one row
        public static async Task<TModel> QuerySingleAsync<TModel>(
            string storedProcedure,
            object? parameters = null,
            SqlTransaction? externalTransaction = null)
        {
            using var connection = new SqlConnection(ConnectionString);
            if (externalTransaction != null)
            {
                return await connection.QuerySingleAsync<TModel>(storedProcedure, parameters, externalTransaction, commandType: CommandType.StoredProcedure);
            }
            else
            {
                await connection.OpenAsync();
                return await connection.QuerySingleAsync<TModel>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        // ExecuteScalarAsync - Return single value
        public static async Task<T> ExecuteScalarAsync<T>(
            string storedProcedure,
            object? parameters = null,
            SqlTransaction? externalTransaction = null)
        {
            using var connection = new SqlConnection(ConnectionString);
            if (externalTransaction != null)
            {
                return await connection.ExecuteScalarAsync<T>(storedProcedure, parameters, externalTransaction, commandType: CommandType.StoredProcedure);
            }
            else
            {
                await connection.OpenAsync();
                return await connection.ExecuteScalarAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }
        
    }
}
