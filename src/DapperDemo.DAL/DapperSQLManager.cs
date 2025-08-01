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
        protected IDbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

    }
}
