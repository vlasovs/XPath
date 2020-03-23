using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Tutorial.SqlConn
{
    class DBUtils
    {
        public static MySqlConnection GetDBConnection()
        {
            string host = "10.10.0.101";
            int port = 3306;
            string database = "XPath";
            string username = "py";
            string password = "pass";
            string characterset = "utf8";

            return DBMySQLUtils.GetDBConnection(host, port, database, username, password, characterset);
        }

    }
}