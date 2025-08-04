using Dapper;
using DapperDemo.DAL.Interface;
using DapperDemo.Domain.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperDemo.DAL.Implementation
{
    public class EmployeeRepository : DapperSQLManager, IEmployeeRepository
    {
        public async Task<bool> CreateEmployeeWithItemsAsync(Employee employee, List<EmployeeItem> items)
        {
            var steps = new List<SpExecutionStep>
            {
                new SpExecutionStep(
                    spName: "usp_AddEmployee",
                    data: employee,
                    outputParams: new[] { nameof(Employee.EmployeeId) }
                ),
                new SpExecutionStep(
                    spName: "usp_AddEmployeeItem",
                    data: items,
                    outputMappings: new[] { (nameof(Employee.EmployeeId), nameof(EmployeeItem.EmployeeId)) }
                )
            };

            return await ExecuteStepsAsync(steps);
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await QueryAsync<Employee>("usp_GetAllEmployees");
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await QueryFirstOrDefaultAsync<Employee>("usp_GetEmployeeById", new { EmployeeId = id });
        }

        public async Task<Employee> GetEmployeeSingleAsync(int id)
        {
            return await QuerySingleAsync<Employee>("usp_GetEmployeeSingle", new { EmployeeId = id });
        }

        public async Task<int> GetEmployeeCountAsync()
        {
            return await ExecuteScalarAsync<int>("usp_GetEmployeeCount");
        }
    }
}
