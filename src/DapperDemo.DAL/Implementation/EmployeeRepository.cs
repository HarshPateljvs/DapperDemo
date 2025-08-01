using Dapper;
using DapperDemo.DAL.Interface;
using DapperDemo.Domain.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperDemo.DAL.Implementation
{
    public class EmployeeRepository : DapperSQLManager, IEmployeeRepository
    {
        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await QueryAsync<Employee>("sp_GetAllEmployees");
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await QueryFirstOrDefaultAsync<Employee>("sp_GetEmployeeById", new { Id = id });
        }

        public async Task<int> AddAsync(Employee employee, SqlTransaction? transaction = null)
        {
            return await ExecuteAsync("sp_AddEmployee", employee, transaction);
        }

        public async Task<int> UpdateAsync(Employee employee, SqlTransaction? transaction = null)
        {
            return await ExecuteAsync("sp_UpdateEmployee", employee, transaction);
        }

        public async Task<int> DeleteAsync(int id, SqlTransaction? transaction = null)
        {
            return await ExecuteAsync("sp_DeleteEmployee", new { Id = id }, transaction);
        }

        // Example: transaction with multiple steps
        public async Task<bool> SaveWithTransactionAsync(Employee employee, List<EmployeeItem> items)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync("sp_AddEmployee", employee, transaction, commandType: CommandType.StoredProcedure);

                foreach (var item in items)
                {
                    await connection.ExecuteAsync("sp_AddEmployeeItem", item, transaction, commandType: CommandType.StoredProcedure);
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }
}
