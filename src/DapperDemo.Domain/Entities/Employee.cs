using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperDemo.Domain.Entities
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public class EmployeeItem
    {
        public int EmployeeId { get; set; }

        public string ItemName { get; set; } = string.Empty;
    }
}
