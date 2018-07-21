using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SqlHelper
{
    /// <summary>
    /// 数据库访问接口类
    /// 关键验证点： 事务 （多个事务同时测试；事务中包含增删改 查 ；每个方法能否正常执行；操作方法考虑各种数据）,
    ///             Null 值的处理 
    /// </summary>
    public interface IDbAccess : IDisposable
    {

        bool IsTran { get; set; }

        bool IsOpen { get; set; }

        IDbConnection conn { get; set; }

        IDbTransaction tran { get; set; }

        string ConnectionStr { get; set; }

        bool IsKeepConnect { get; set; }


        #region 连接相关
        void Open();

        void Close();
        #endregion

        #region 事务相关
        void BeginTransaction();

        void Rollback();

        void Commit();
        #endregion

        #region 执行sql语句 返回受影响的行数
        /// <summary>
        /// 执行sql语句 返回受影响的行数
        /// </summary>
        /// <param name="sql">需要执行的sql语句</param>
        /// <returns>受影响的行数</returns>
        bool ExecuteNoQuery(string sql);
        /// <summary>
        /// sql查询，返回datatable
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        DataTable ExecuteQuery(string sql);
        bool ConnectionTest(string Connection);
        #endregion
    }
}

