using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Database
{
    public class DBManager : IDisposable
    {
        private SqlConnectionString _sqlconnectstring = new SqlConnectionString(AppConfigurationInfo.strConnectionString);
        private SqlConnection _Sqlconnection = new SqlConnection();

        public SqlConnection Sqlconnection
        {
            get { return _Sqlconnection; }
            set { _Sqlconnection = value; }
        }

        public DBManager()
        {
            try
            {
                if (_Sqlconnection.State != ConnectionState.Open)
                {
                    _Sqlconnection.ConnectionString = _sqlconnectstring.ConnectionString;
                    _Sqlconnection.Open();
                }
            }
            catch (SqlException ex)
            {
                ExceptionHelper cusEx = new ExceptionHelper(ex);
            }
        }

        public DBManager(int _timeout)
        {
            try
            {
                if (_Sqlconnection.State != ConnectionState.Open)
                {
                    _sqlconnectstring.Timeout = _timeout;

                    _Sqlconnection.ConnectionString = _sqlconnectstring.ConnectionString;
                    _Sqlconnection.Open();
                }
            }
            catch (SqlException ex)
            {
                ExceptionHelper cusEx = new ExceptionHelper(ex);
            }
        }

        public DBManager(string connString)
        {
            _Sqlconnection.ConnectionString = connString;
            AppConfigurationInfo.strConnectionString = connString;
        }

        /// <summary>
        /// Test connection string
        /// </summary>
        /// <returns></returns>
        public Boolean Test()
        {
            try
            {
                _Sqlconnection.Open();
                if (_Sqlconnection.State == ConnectionState.Open) return true;
                else return false;
            }
            catch (SqlException sqlexp)
            {
                return false;
            }
            catch (Exception exp)
            {
                return false;
            }
            finally
            {
                _Sqlconnection.Close();
            }
        }

        private SqlTransaction _Sqltransaction;

        public SqlTransaction Sqltransaction
        {
            get { return _Sqltransaction; }
            set { _Sqltransaction = value; }
        }

        public void BeginTransaction()
        {
            if (this._Sqlconnection == null)
                this._Sqltransaction = this._Sqlconnection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (this._Sqltransaction != null)
                this._Sqltransaction.Commit();
            _Sqltransaction = null;
        }

        public void RollbackTransaction()
        {
            if (this._Sqltransaction != null)
                this._Sqltransaction.Rollback();
            _Sqltransaction = null;
        }

        public void Open()
        {
            try
            {
                if (_Sqlconnection.State != ConnectionState.Open)
                {
                    //_Sqlconnection.ConnectionTimeout = 200;
                    _Sqlconnection.ConnectionString = AppConfigurationInfo.strConnectionString;
                    _Sqlconnection.Open();
                }
            }
            catch (SqlException sqlEx)
            {
                ExceptionHelper cusEx = new ExceptionHelper(sqlEx);
                throw cusEx;
            }
            catch (Exception ex)
            {
                ExceptionHelper cusEx = new ExceptionHelper(ex);
                throw cusEx;
            }
        }

        public void Close()
        {
            try
            {
                if (_Sqlconnection.State != ConnectionState.Closed)
                    _Sqlconnection.Close();
            }
            catch (SqlException sqlEx)
            {
                ExceptionHelper cusEx = new ExceptionHelper(sqlEx);
                throw cusEx;
            }
            catch (Exception ex)
            {
                ExceptionHelper cusEx = new ExceptionHelper(ex);
                throw cusEx;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Close();
            this._Sqlconnection = null;
            this._Sqltransaction = null;
        }

        #endregion
    }
}
