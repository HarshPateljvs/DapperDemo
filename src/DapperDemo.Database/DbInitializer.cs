using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
namespace DapperDemo.Database
{
    public class DbInitializer
    {
        private readonly string _connStr;

        public DbInitializer(IConfiguration config) =>
            _connStr = config.GetConnectionString("DefaultConnection")!;

        public async Task RunScriptsAsync()
        {
            var scripts = Directory.GetFiles("Scripts", "*.sql").OrderBy(f => f);
            foreach (var file in scripts)
            {
                var sql = await File.ReadAllTextAsync(file);
                using var conn = new SqlConnection(_connStr);
                using var cmd = new SqlCommand(sql, conn);
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

}
