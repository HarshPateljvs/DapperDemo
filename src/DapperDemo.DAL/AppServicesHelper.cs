using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperDemo.DAL
{
    public static class AppServicesHelper
    {
        static IServiceProvider services = null;
        public static Hashtable htException = new Hashtable();

        /// <summary>
        /// Provides static access to the framework's services provider
        /// </summary>
        public static IServiceProvider Services
        {
            get { return services; }
            set
            {
                if (services != null)
                {
                    throw new Exception("Can't set once a value has already been set.");
                }
                services = value;
            }
        }

        /// <summary>
        /// Provides static access to the current HttpContext
        /// </summary>
        public static HttpContext HttpContext_Current
        {
            get
            {
                IHttpContextAccessor httpContextAccessor = services.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
                return httpContextAccessor?.HttpContext;
            }
        }


        /// <summary>
        /// Configuration settings from appsetting.json.
        /// </summary>
        public static MyAppSettings Config
        {
            get
            {
                //This works to get file changes.
                var s = services.GetService(typeof(IOptionsMonitor<MyAppSettings>)) as IOptionsMonitor<MyAppSettings>;
                MyAppSettings config = s.CurrentValue;

                return config;
            }
        }
    }
    public class MyAppSettings
    {
        public string DapperConnectinString { get; set; }
        public string OtherDbConectionString { get; set; }
        public bool IsLocal { get; set; }
    }
}
