using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperDemo.DAL
{
    public class SqlConnectionFactory
    {
        private readonly IConfiguration _config;
        public SqlConnectionFactory(IConfiguration config) => _config = config;

        public IDbConnection GetConnection() => new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    }

}
