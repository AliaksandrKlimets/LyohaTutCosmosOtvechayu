using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _222.Models
{
    public class DocEmployeeConfig
{
        public DocEmployeeConfig()
        {
            string nc = "Not configured";
            Endpoint = nc;
            AuthKey = nc;
            Database = nc;
            EmployeeCollection = nc;
        }

        public string Endpoint { get; set; }
        public string AuthKey { get; set; }
        public string Database { get; set; }
        public string EmployeeCollection { get; set; }
    }
}
