using DapperDemo.Domain.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperDemo.DAL.Interface
{

    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<int> AddAsync(Employee employee, SqlTransaction? transaction = null);
        Task<int> UpdateAsync(Employee employee, SqlTransaction? transaction = null);
        Task<int> DeleteAsync(int id, SqlTransaction? transaction = null);
        Task<bool> SaveWithTransactionAsync(Employee employee, List<EmployeeItem> items);
    }
}
