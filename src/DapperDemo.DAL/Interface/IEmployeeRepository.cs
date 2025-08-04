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
        Task<bool> CreateEmployeeWithItemsAsync(Employee employee, List<EmployeeItem> items);
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<Employee> GetEmployeeSingleAsync(int id);
        Task<int> GetEmployeeCountAsync();
    }
}
