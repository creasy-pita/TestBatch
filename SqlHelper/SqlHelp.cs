using Log;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;

namespace SqlHelper
{
    public class SqlHelp:IDbAccess
    {
        WriteLog log = new WriteLog();
        public bool IsTran
        {
            get;
            set;
        }

        public bool IsOpen
        {
            get;
            set;
        }

        public IDbConnection conn
        {
            get;
            set;
        }

        public IDbTransaction tran
        {
            get;
            set;
        }

        public string ConnectionStr
        {
            get;
            set;
        }

        public bool IsKeepConnect
        {
            get;
            set;
        }

        public void Open()
        {
            if (this.conn.State != ConnectionState.Open)
            {
                this.conn.Open();
                IsOpen = true;
            }
        }

        public void Close()
        {
            IsOpen = false;
            IsTran = false;
            IsKeepConnect = false;
            this.conn.Close();
        }

        public void Open(string Connection)
        {
            this.conn = new OracleConnection(Connection);
            if(conn.State!=ConnectionState.Open)
            {
                try
                {
                    conn.Open();
                }
                catch(Exception e)
                {
                    log.Logger(e.Message);
                }
            }
        }
        public OracleConnection OpenConn(string Connection)
        {
            OracleConnection conn = new OracleConnection(Connection);
            if(conn.State!=ConnectionState.Open)
            {
                try
                {
                    conn.Open();
                }
                catch(Exception e)
                {
                    log.Logger(e.Message);
                }
            }
            return conn;
        }
        public void Close(OracleConnection conn)
        {
            if(conn.State == ConnectionState.Open)
            {
                try
                {
                    conn.Close();
                }
                catch (Exception e)
                {
                    log.Logger(e.Message);
                }
            }
        }
        public void BeginTransaction()
        {
            if (this.tran == null)
            {
                if (conn.State != ConnectionState.Open)
                    Open();
                tran = conn.BeginTransaction();
                IsTran = true;
            }
        }

        public void Rollback()
        {
            if (this.tran != null)
            {
                tran.Rollback();
                tran = null;
                IsTran = false;
            }
        }

        public void Commit()
        {
            if (this.tran != null)
            {
                tran.Commit();
                tran = null;
                IsTran = false;
            }
        }
        public void Dispose()
        {
            if (this.conn != null && this.conn.State != ConnectionState.Closed)
            {
                Close();
            }
        }
        /// <summary>
        /// 执行数据库的插入和更新语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="conn">数据库连接对象</param>
        /// <returns></returns>
        public bool ExecuteNoQuery(string sql,OracleConnection conn)
        {
            if(conn.State!=ConnectionState.Open)
            {
                conn.Open();
            }//如果连接关闭，重新打开
            OracleTransaction trans = conn.BeginTransaction();
            try
            {
                //Open();
                OracleCommand comm = new OracleCommand(sql, conn);
                comm.Transaction = trans;
                if (comm.ExecuteNonQuery() > 0)
                {
                    trans.Commit();
                    return true;
                }                   
                else
                {
                    trans.Rollback();
                    return false;
                }                   
            }
            catch (Exception ex)
            {
                trans.Rollback();
                log.Logger(ex.Message);
                throw ex;
            }
            finally
            {
            }
        }
        /// <summary>
        /// 执行查询操作
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="conn">数据库连接对象</param>
        /// <returns></returns>
        public DataTable ExecuteQuery(string sql,OracleConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }//如果连接关闭，重新打开
            DataTable da = new DataTable();
            try
            {
               // Open();
                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.CommandText = sql;
                OracleDataReader ds = cmd.ExecuteReader();
                da.Load(ds);
            }
            catch (Exception ex)
            {
               
                log.Logger(ex.Message);
                //Close(conn);
                throw ex;
            }
            finally
            {
                //Close();
            }
            return da;
        }
        public bool ExecuteNoQuery(string sql)
        {
            try
            {
                //Open();
                OracleCommand comm = new OracleCommand(sql, this.conn as OracleConnection);
                if (IsTran) comm.Transaction = (OracleTransaction)this.tran;
                if (comm.ExecuteNonQuery() > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.Logger(ex.Message);
                throw ex;
            }
            finally
            {
                //if (!IsTran && !IsKeepConnect)
                //{
                //    Close();
                //}
            }
        }
        public DataTable ExecuteQuery(string sql)
        {
            DataTable da = new DataTable();
            try
            {
                // Open();
                OracleCommand cmd = new OracleCommand(sql, this.conn as OracleConnection);
                cmd.CommandText = sql;
                OracleDataReader ds = cmd.ExecuteReader();
                log.Logger(string.Format("当前取得行数是否大于0:{0}", ds.HasRows));
                da.Load(ds);
            }
            catch (Exception ex)
            {

                log.Logger(ex.Message);
                throw ex;
            }
            finally
            {
                //Close();
            }
            return da;
        }
        /// <summary>
        /// 测试Oracle数据库是否可以连接
        /// </summary>
        /// <param name="Connection">数据库连接字符串</param>
        /// <returns></returns>
        public bool ConnectionTest(string Connection)
        {
            bool result = false;
            using (OracleConnection conn = new OracleConnection(Connection))
            {
                if (conn.State != ConnectionState.Open)
                {
                    try
                    {
                        conn.Open();
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        log.Logger(ex.ToString());
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            return result;
        }
    }
}
