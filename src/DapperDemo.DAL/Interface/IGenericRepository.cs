using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperDemo.DAL.Interface
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(string sp, object? param = null);
        Task<T?> GetByIdAsync(string sp, object param);
        Task<int> ExecuteAsync(string sp, object param);
        Task<bool> ExecuteTransactionAsync((string sp, object param)[] calls);
    }
}
