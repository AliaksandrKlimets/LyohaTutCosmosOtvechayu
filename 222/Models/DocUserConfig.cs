using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _222.Models
{
    public class DocUserConfig
{
        public DocUserConfig()
        {
            string nc = "Not configured";
            Endpoint = nc;
            AuthKey = nc;
            Database = nc;
            UserCollection = nc;
        }

        public string Endpoint { get; set; }
        public string AuthKey { get; set; }
        public string Database { get; set; }
        public string UserCollection { get; set; }

    }
}
