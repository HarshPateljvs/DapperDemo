using Dapper;
using DapperDemo.DAL.Interface;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperDemo.DAL.Implementation
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly SqlConnectionFactory _factory;
        public GenericRepository(SqlConnectionFactory factory) => _factory = factory;

        public async Task<IEnumerable<T>> GetAllAsync(string sp, object? param = null)
        {
            using var conn = _factory.GetConnection();
            return await conn.QueryAsync<T>(sp, param, commandType: CommandType.StoredProcedure);
        }

        public async Task<T?> GetByIdAsync(string sp, object param)
        {
            using var conn = _factory.GetConnection();
            return await conn.QueryFirstOrDefaultAsync<T>(sp, param, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> ExecuteAsync(string sp, object param)
        {
            using var conn = _factory.GetConnection();
            return await conn.ExecuteAsync(sp, param, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> ExecuteTransactionAsync((string sp, object param)[] spCalls)
        {
            using var conn = (SqlConnection)_factory.GetConnection(); // 👈 Cast to SqlConnection
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            try
            {
                foreach (var call in spCalls)
                {
                    await conn.ExecuteAsync(
                        call.sp,
                        call.param,
                        transaction: tx,
                        commandType: CommandType.StoredProcedure // 👈 Named parameter
                    );
                }

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                return false;
            }
        }

    }

}
