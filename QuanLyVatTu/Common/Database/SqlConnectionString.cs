using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.Database
{
    /// <summary>
    /// Manages Sql Connection String
    /// </summary>
    public class SqlConnectionString
    {
        static Regex parseServer = new Regex(
            @"(?i:((Data\sSource)|(Server)|(Address)|(Addr)|(Network Address))=(?<server>[^;]+))"
            , RegexOptions.Compiled);

        static Regex parseAuth = new Regex(
            @"(?i:((Integrated\sSecurity)|(Trusted_Connection))=(?<auth>(true)|(yes)|(sspi)))"
            , RegexOptions.Compiled);

        static Regex parseDatabase = new Regex(
            @"(?i:((Initial\sCatalog)|(Database))=(?<database>[^;]+))"
            , RegexOptions.Compiled);

        static Regex parsePassword = new Regex(
            @"(?i:((Password)|(Pwd))=(?<password>[^;]+))"
            , RegexOptions.Compiled);

        static Regex parseUser = new Regex(
            @"(?i:(User\sID=(?<user>[^;]+)))"
            , RegexOptions.Compiled);

        static Regex parseTimeout = new Regex(
            @"(?i:(Connection Timeout=(?<timeout>[^;])))"
            , RegexOptions.Compiled);

        /// <summary>
        /// Check exist connect
        /// </summary>
        /// <returns></returns>
        public Boolean CheckConnect()
        {
            string _conn = ConnectionString;

            SqlConnection sqlconn = new SqlConnection(_conn);
            try
            {
                sqlconn.Open();
                sqlconn.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        //string connectionString = "";
        public string ConnectionString
        {
            get
            {
                if (server.Length == 0 && database.Length == 0)
                    return "";

                StringBuilder sb = new StringBuilder();
                if (server.Length > 0)
                    sb.Append("Server=").Append(server).Append(";");

                if (database.Length > 0)
                    sb.Append("Database=").Append(database).Append(";");

                if (auth)
                    //sb.Append ("Integrated Security=SSPI;");
                    sb.Append("Persist Security Info=False;Integrated Security=True");

                if (user.Length > 0)
                    sb.Append("User ID=").Append(user).Append(";");

                if (!string.IsNullOrEmpty(password))
                    sb.Append("Pwd=").Append(password).Append(";");

                if (timeout > 0)
                    sb.Append("Connection Timeout=").Append(timeout.ToString()).Append(";");

                return sb.ToString();
            }
            set
            {
                server = parseServer.Match(value).Groups["server"].Value;
                database = parseDatabase.Match(value).Groups["database"].Value;
                auth = database.Length == 0 ||
                    parseAuth.Match(value).Groups["auth"].Value.Length > 0;
                user = parseUser.Match(value).Groups["user"].Value;
                password = parsePassword.Match(value).Groups["password"].Value;

                string _s = parseTimeout.Match(value).Groups["timeout"].Value;
                if (string.IsNullOrEmpty(_s))
                    _s = "0";
                timeout = int.Parse(_s);
            }
        }

        string server = "";
        public string Server
        {
            get
            {
                return server;
            }
            set
            {
                server = value;
            }
        }

        string database = "";
        public string Database
        {
            get
            {
                return database;
            }
            set
            {
                database = value;
            }
        }

        string user = "";
        public string UserID
        {
            get
            {
                return user;
            }
            set
            {
                user = value;
            }
        }

        string password = "";
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }

        bool auth = true;
        public bool Authentication
        {
            get
            {
                return auth;
            }
            set
            {
                auth = value;
            }
        }

        int timeout = 30;
        public int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
            }
        }

        public SqlConnectionString(string _ConnectionString)
        {
            ConnectionString = _ConnectionString;
        }

        public SqlConnectionString()
        {
            ConnectionString = "";
        }

        #region ICloneable Members

        //public object Clone()
        //{
        //    SqlConnectionString scs = new SqlConnectionString ();
        //    scs.auth = auth;
        //    scs.database = database;
        //    scs.password = password;
        //    scs.server = server;
        //    scs.user = user;
        //    scs.timeout = timeout;
        //    return scs;
        //}

        #endregion
    }
}
