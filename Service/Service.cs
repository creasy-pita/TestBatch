using Enity;
using Log;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using SqlHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;

namespace Services
{
    public class Service
    {
        WriteLog log = new WriteLog();
        public static int AutoBackup_Symbol = 0;
        public static Project projects;//配置文件序列化对象
        public static Table tables;///配置文件序列化对象
        public static string basePath;//获取根目录
        /// <summary>
        /// project.xml文件序列化
        /// </summary>
        /// <returns></returns>
        public static Project AppConfig()
        {
            string path = string.Format("{0}\\DataSourceSetting.xml", basePath);
            XmlOperation X = new XmlOperation();
            Project G = new Project();
            G = (Project)X.LoadXML(path, typeof(Project));
            return G;
        }
        /// <summary>
        /// Table.xml文件序列化
        /// </summary>
        /// <returns></returns>
        public static Table XmlConfig()
        {
            string path = string.Format("{0}\\Table.xml", basePath);
            XmlOperation X = new XmlOperation();
            Table G = new Table();
            G = (Table)X.LoadXML(path, typeof(Table));
            return G;
        }
        /// <summary>
        /// 单例模式下XML序列化
        /// </summary>
        public class Singleton
        {
            private static object Singleton_Lock = new object();
            private static Singleton _Singleton = null;
            public static Singleton CreateInstance()
            {
                if (_Singleton == null)
                {
                    lock (Singleton_Lock)
                    {
                        if (_Singleton == null)
                        {
                            _Singleton = new Singleton();
                            basePath = "";
                            basePath = string.IsNullOrWhiteSpace(basePath) ? Directory.GetCurrentDirectory() : basePath;//读取程序根目录
                            projects = AppConfig();
                            tables = XmlConfig();
                        }
                    }
                }
                return _Singleton;
            }
        }
        /// <summary>
        /// 构造函数中初始化数据
        /// </summary>
        public Service()
        {
            Singleton singleton = Singleton.CreateInstance();
        }
        /// <summary>
        /// 将查询到待处理的数据插入到目标表中（这里DataTable的列必须和数据库中表列名必须一致）
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dataTable">带插入的数据，存入DataTable</param>
        /// <param name="sql">目标数据库查询语句，查询到的数据存入DataTable</param>
        /// <returns></returns>
        public bool InsertWithDt(SqlHelp sqlhelp,string TargetconnectionString, string FromconnectionString, DataTable dataTable, string TableName)
        {
            if(out_connection.State!=ConnectionState.Open)
            {
                out_connection.Open();
            }//如果连接关闭，重新打开
            bool result = false;
            string ColumnName = null;
            for (int iCount = 0; iCount < dataTable.Columns.Count; iCount++)
            {
                ColumnName = ColumnName + "," + dataTable.Columns[iCount].ColumnName;
            }
            ColumnName = ColumnName.Substring(1, ColumnName.Length - 1);
            string sql = string.Format("SELECT {0} FROM {1} WHERE 1 < 0 ", ColumnName, TableName); //取表结构
            try
            {
                //int im = 100;
                //im = im / 0;
                //测试出现错误是否可以自动继续执行
                OracleCommand cmd = new OracleCommand(sql, out_connection);
                OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                OracleCommandBuilder cb = new OracleCommandBuilder(adapter);
                DataTable dsNew = new DataTable();
                int count = adapter.Fill(dsNew);
                DataRow addRow = null;
                DateTime t = DateTime.Now;
                foreach (DataRow dr in dataTable.Rows)//将dataTable数据加载到dsNew
                {
                    addRow = dsNew.NewRow();
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        addRow[i] = dr[i];
                    }
                    dsNew.Rows.Add(addRow);
                }
                count = adapter.Update(dsNew);
                result = true;
                log.Logger(string.Format("外网归集库{0}:{1}表中插入{2}条数据成功！", TargetconnectionString, TableName, dsNew.Rows.Count));
            }
            catch (Exception ex)
            {
                log.Logger(ex.ToString());
                //如果外网归集库已经存在该条BSM记录,需要将不存在的BSM存到目标归集库
                if (ex.ToString().IndexOf("违反唯一约束条件") > 0) 
                {
                    try
                    {
                        OracleCommand cmd = new OracleCommand(sql, out_connection);
                        OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                        OracleCommandBuilder cb = new OracleCommandBuilder(adapter);
                        DataTable dsNew = new DataTable();
                        int count = adapter.Fill(dsNew);
                        DataRow addRow = null;
                        foreach (DataRow dr in dataTable.Rows)
                        {
                            string selectsql = string.Format("SELECT BSM FROM {0} WHERE BSM='{1}'", TableName, dr["BSM"]);
                            if (sqlhelp.ExecuteQuery(selectsql, out_connection).Rows.Count == 0)
                            {
                                addRow = dsNew.NewRow();
                                for (int i = 0; i < dataTable.Columns.Count; i++)
                                {
                                    addRow[i] = dr[i];
                                }
                                dsNew.Rows.Add(addRow);
                            }
                        }
                        count = adapter.Update(dsNew);
                        result = true;
                        log.Logger(string.Format("外网归集库{0}:{1}表中插入{2}条数据成功！", TargetconnectionString, TableName, dsNew.Rows.Count));
                    }
                    catch (Exception e)
                    {
                        log.Logger(e.ToString());
                        //throw e;
                    }
                }
                else   
                {
                    result = false;
                    //throw ex;
                }
            }
            return result;
        }
        /// <summary>
        /// 更新内网TSZT，插入TSSJ
        /// </summary>
        /// <param name="sqlhelp"></param>
        /// <param name="FromconnectionString">内网信息</param>
        /// <param name="dataTable">查询获取的数据</param>
        /// <param name="TableName">表名</param>
        /// <returns></returns>
        public bool UpdateWithDt(SqlHelp sqlhelp,  string FromconnectionString, DataTable dataTable, string TableName)
        {
            if(in_connection.State!=ConnectionState.Open)
            {
                in_connection.Open();
            }//如果连接关闭，重新打开
            bool result = false;
            try
            {
                //int im = 100;
                //im = im / 0;
                //测试出现错误是否可以自动继续执行
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    DateTime dt = DateTime.Now;
                    string updatesql = string.Format("UPDATE {0} SET TSZT='1',TSSJ=to_date('{1}','yyyy-MM-dd hh24:mi:ss') WHERE BSM='{2}'", TableName, dt.ToLocalTime().ToString(), dataTable.Rows[i]["BSM"]);
                    if (sqlhelp.ExecuteNoQuery(updatesql, in_connection))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                log.Logger($"内网归集库{FromconnectionString}:{TableName}状态更新成功！");
            }
            catch (Exception e)
            {
                log.Logger($"更新状态出错，错误信息:{ e.Message}");
                result = false;
                //throw e;
            }
            return result;
        }
        private object o =new object();
        /// <summary>
        /// 读取配置文件，按照方案执行同步
        /// </summary>
        /// <param name="Project_Name">方案名称</param>
        /// <param name="sql">Table.xml中的sql语句</param>
        /// <returns></returns>
        public bool Select_Insert_Update(SqlHelp sqlhelper,string Project_Name, string sql)
        {
            bool result = false;
            try
            {
                //int im = 100;
                //im = im / 0;
                //测试出现错误是否可以自动继续执行
                foreach (Projects p in projects.projects)
                {                  
                    if (p.name == Project_Name)
                    {
                        string in_conn = string.Format("user id = {0}; password = {1}; data source = {2}",
                            p.inner_name,
                            p.inner_pwd,
                            p.inner_source);
                        string out_conn = string.Format("user id = {0}; password = {1}; data source = {2}",
                            p.out_name,
                            p.out_pwd,
                            p.out_source);                    
                        lock (o)
                        {
                            log.Logger(string.Format("当前方案名称:{0}", Project_Name));
                            DataTable dt = new DataTable();                         
                            dt = sqlhelper.ExecuteQuery(sql, in_connection);
                            if (dt.Rows.Count > 0)
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    log.Logger(string.Format("{0}表待同步数据:{1}条", dt.Rows[i]["NAME"], dt.Rows[i]["SL"]));
                                    if (Convert.ToInt32(dt.Rows[i]["SL"]) > 0)
                                    {
                                        foreach (Tables T in tables.tables)
                                        {
                                            if (T.Name == dt.Rows[i]["NAME"].ToString())
                                            {
                                                string select_sql = T.Table;
                                                DataTable da = new DataTable();                                                                                             
                                                da = sqlhelper.ExecuteQuery(select_sql, in_connection);
                                                log.Logger(string.Format("内网归集库{0}:{1}开始同步数据", in_conn, T.Name));
                                                if (InsertWithDt(sqlhelper, out_conn, in_conn, da, T.Name))
                                                {
                                                    if (UpdateWithDt(sqlhelper, in_conn, da, T.Name))
                                                    {
                                                        result = true;
                                                    }
                                                    else
                                                        result = false;
                                                }
                                                                                                     
                                                else
                                                    result = false;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        result = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Logger($"方案执行同步时出错：{ex.Message}");
                result = false;
                //throw ex;
            }
            return result;
        }
        /// <summary>
        /// 读取xml配置文件，进行手动同步操作
        /// </summary>
        /// <returns>返回bool型操作结果</returns>
        public bool ManulaSyc()
        {
            bool result = false;
            SqlHelp sqlhelper = new SqlHelp();
            try
            {
                if (projects.projects.Count == 0 || projects.projects == null)
                {
                    log.Logger("当前未配置方案，请在配置好方案后重试！");
                    result = false;
                }
                else if (AutoBackup_Symbol == 1)
                {
                    log.Logger("自动同步正在执行,请等待自动同步执行完毕后再进行！");
                    result = false;
                }
                else
                {
                    log.Logger("批量同步开始*****************");
                    foreach (Projects p in projects.projects)
                    {
                        string in_conn = string.Format("user id = {0}; password = {1}; data source = {2}",
                                p.inner_name,
                                p.inner_pwd,
                                p.inner_source);
                        string out_conn = string.Format("user id = {0}; password = {1}; data source = {2}",
                                p.out_name,
                                p.out_pwd,
                                p.out_source);
                        in_connection = sqlhelper.OpenConn(in_conn);
                        out_connection = sqlhelper.OpenConn(out_conn);
                        result = Select_Insert_Update(sqlhelper, p.name, projects.SQL);
                        sqlhelper.Close(in_connection);
                        sqlhelper.Close(out_connection);
                    }
                }
            }
            catch (Exception e)
            {
                log.Logger($"手动同步出错，出错信息：{e.Message}");
                sqlhelper.Close(in_connection);
                sqlhelper.Close(out_connection);
                result = false;
            }          
            return result;
        }
        /// <summary>
        /// 自动同步
        /// </summary>
        public void AutoSyc()
        {
            AutoBackup_Symbol = 1;
            if (projects.projects.Count == 0 || projects.projects == null)
            {
                log.Logger("当前未配置方案，请在配置好方案后重试！");
            }
            else
            {
                try
                {
                    log.Logger("自动同步开始*****************");
                    foreach (Projects P in projects.projects)
                    {
                        AutoBackup(Convert.ToInt32(P.synchron_cycle), P.name, projects.SQL);
                    }
                }
                catch(Exception ex)
                {
                    log.Logger(ex.Message);
                    //throw ex;
                }
            }
        }
        public static OracleConnection in_connection;
        public static OracleConnection out_connection;
        /// <summary>
        /// 根据方案周期设置阻止时间
        /// </summary>
        /// <param name="Cycle">周期</param>
        /// <param name="ProjectName">方案名称</param>
        /// <param name="sql">SQL语句</param>
        public void AutoBackup(int Cycle, string ProjectName, string sql)
        {
            try
            {
                Thread thread = new Thread(new ThreadStart(Start_Realize));
                thread.Start();
                void Start_Realize()
                {
                    SqlHelp sqlhelper = new SqlHelp();
                    foreach (Projects p in projects.projects)
                    {
                        if (p.name == ProjectName)
                        {
                            string in_conn = string.Format("user id = {0}; password = {1}; data source = {2}",
                            p.inner_name,
                            p.inner_pwd,
                            p.inner_source);
                            string out_conn = string.Format("user id = {0}; password = {1}; data source = {2}",
                            p.out_name,
                            p.out_pwd,
                            p.out_source);

                            in_connection = sqlhelper.OpenConn(in_conn);
                            out_connection = sqlhelper.OpenConn(out_conn);
                            while (AutoBackup_Symbol == 1)
                            {
                                if (Select_Insert_Update(sqlhelper, ProjectName, sql) && (AutoBackup_Symbol == 1))
                                {
                                    log.Logger(string.Format("自动同步，{0}方案同步执行完毕*****************", ProjectName));
                                    if (AutoBackup_Symbol == 1)
                                    {
                                        Thread.Sleep(60000 * Cycle);
                                    }
                                }
                                else
                                {
                                    sqlhelper.Close(in_connection);
                                    sqlhelper.Close(out_connection);
                                    log.Logger("同步异常，请及时处理！*****************");
                                }
                            }
                        }
                    }
            
                }                            
            }
            catch (Exception ex)
            {
                log.Logger(ex.Message);
                //throw ex;
            }
        }
        /// <summary>
        /// 关闭自动同步
        /// </summary>
        /// <returns></returns>
        public string StopAutoSyc()
        {
            string result = null;
            if (AutoBackup_Symbol == 0)
            {
                result = "自动同步已经停止，无需重复操作！";
            }
            else
            {
                AutoBackup_Symbol = 0;
                result = "自动同步已停止!";
                log.Logger("同步已关闭*****************");
                SqlHelp sqlhelp = new SqlHelp();
                sqlhelp.Close(in_connection);
                sqlhelp.Close(out_connection);
            }
            return result;
        }
        /// <summary>
        /// 展示方案名称
        /// </summary>
        /// <returns></returns>
        public string ShowProject()
        {
            StringBuilder ProjectName = new StringBuilder();
            foreach (Projects P in projects.projects)
            {
                ProjectName.Append(P.name);
                ProjectName.Append("*");
            }
            return ProjectName.ToString().Remove(ProjectName.ToString().Length - 1);
        }
        /// <summary>
        /// 通过方案名，查看参数
        /// </summary>
        /// <param name="ProjectName">方案名</param>
        /// <returns></returns>
        public string ShowProjectParameter(string ProjectName)
        {
            string ProjectParameter = null;
            List<Projects> P = new List<Projects>();
            foreach (Projects projects in projects.projects)
            {
                if (projects.name == ProjectName)
                {
                    string FromUserName = projects.inner_name;
                    string FromUserPassWord = projects.inner_pwd;
                    string FromDataSourceName = projects.inner_source;
                    string TargetUserName = projects.out_name;
                    string TargetUserPassWord = projects.out_pwd;
                    string TargetDataSourceName = projects.out_source;
                    string Cycle = projects.synchron_cycle;
                    ProjectParameter = Cycle + "*" + TargetUserName + "*" + TargetUserPassWord + "*" + TargetDataSourceName + "*" + FromUserName + "*" + FromUserPassWord + "*" + FromDataSourceName;
                }               
            }         
            return ProjectParameter;
        }
        /// <summary>
        /// 查看是否存在该方案
        /// </summary>
        /// <param name="ProjectName">方案名称</param>
        /// <param name="dataSourceDesc">数据库对象实体</param>
        /// <returns>不存在返回true,存在返回false</returns>
        public bool IsExist(Projects project)
        {
            foreach (Projects P in projects.projects)
            {
                if (P.name == project.name)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 保存用户设置XML参数方法
        /// </summary>
        /// <param name="dataSourceDesc">数据库对象实体</param>
        /// <returns>返回处理结果</returns>
        public string SaveXML(Projects project)
        {
            SqlHelp sqlhelper = new SqlHelp();
            string result = null;
            string in_conn = string.Format("user id={0};password={1};data source={2}",
                project.inner_name,
                project.inner_pwd,
                project.inner_source);
            string out_conn = string.Format("user id={0};password={1};data source={2}",
                project.out_name,
                project.out_pwd,
                project.out_source);
            if (sqlhelper.ConnectionTest(in_conn) && sqlhelper.ConnectionTest(out_conn))
            {
                projects.projects.Add(new Projects
                {
                    inner_name = project.inner_name,
                    inner_pwd = project.inner_pwd,
                    inner_source = project.inner_source,
                    out_name = project.out_name,
                    out_pwd = project.out_pwd,
                    out_source = project.out_source,
                    name = project.name,
                    synchron_cycle = project.synchron_cycle
                });
                XmlOperation X = new XmlOperation();
                X.SaveXML(string.Format("{0}\\DataSourceSetting.xml", basePath), typeof(Project), projects);
                result = "参数保存成功！";
            }
            else if (!sqlhelper.ConnectionTest(out_conn))
            {
                result = "外网数据库连接失败，请检查配置参数！";
            }
            else if (!sqlhelper.ConnectionTest(in_conn))
            {
                result = "内网数据库连接失败，请检查配置参数！";
            }
            return result;
        }
        /// <summary>
        /// 修改Project.xml配置文件
        /// </summary>
        /// <param name="project">实例化对象</param>
        /// <returns></returns>
        public string UpdateXML(Projects project)
        {
            SqlHelp sqlhelper = new SqlHelp();
            string result = null;
            string in_conn = string.Format("user id={0};password={1};data source={2}",
                project.inner_name,
                project.inner_pwd,
                project.inner_source);
            string out_conn = string.Format("user id={0};password={1};data source={2}",
                project.out_name,
                project.out_pwd,
                project.out_source);
            for (int i = 0; i < projects.projects.Count; i++)
            {
                if (projects.projects[i].name == project.name)
                {
                    projects.projects[i].name = project.name;
                    projects.projects[i].synchron_cycle = project.synchron_cycle;
                    projects.projects[i].inner_name = project.inner_name;
                    projects.projects[i].inner_pwd = project.inner_pwd;
                    projects.projects[i].inner_source = project.inner_source;
                    projects.projects[i].out_name = project.out_name;
                    projects.projects[i].out_pwd = project.out_pwd;
                    projects.projects[i].out_source = project.out_source;
                    XmlOperation X = new XmlOperation();
                    X.SaveXML(string.Format("{0}\\DataSourceSetting.xml", basePath), typeof(Project), projects);
                    result = "参数配置修改成功！";
                }
            }
            return result;
        }
        /// <summary>
        /// 修改Project.xml里的Project节点
        /// </summary>
        /// <param name="ProjectName">方案名称</param>
        /// <returns></returns>
        public string DeleteXML(string ProjectName)
        {
            string result = null;
            bool IsExist = false; ;
            foreach (Projects P in projects.projects)
            {
                if (P.name == ProjectName)
                {
                    IsExist = true;
                }
            }
            if (!IsExist)
            {
                return "该方案不存在！";
            }
            for (int i = 0; i < projects.projects.Count; i++)
            {
                if (projects.projects[i].name == ProjectName)
                {
                    projects.projects.Remove(projects.projects[i]);
                    XmlOperation X = new XmlOperation();
                    X.SaveXML(string.Format("{0}\\DataSourceSetting.xml", basePath), typeof(Project), projects);
                    result = "删除成功！";
                }
            }
            return result;
        }
        /// <summary>
        /// 打开当天的日志
        /// </summary>
        /// <returns>返回日志内容</returns>
        public string Open_log()
        {
            string result = null;
            string file = "\\logfile\\" + DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "") + ".log";
            if (File.Exists(basePath + file))
            {
                FileStream fs = new FileStream(basePath + file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
                StringBuilder sb = new StringBuilder();
                while (!sr.EndOfStream)
                {
                    sb.AppendLine(sr.ReadLine());
                }
                result = sb.ToString();
            }
            else
            {
                result = "未找到日志文件！";
            }
            return result;
        }
        /// <summary>
        /// 通过静态全局变量查询同步状态
        /// </summary>
        /// <returns></returns>
        public int Find_AutoBackup_Symbol()
        {
            return AutoBackup_Symbol;
        }
        /// <summary>
        /// 测试
        /// </summary>
        /// <returns></returns>
        public string Test()
        {
            return JsonConvert.SerializeObject(tables);
        }
    }
}
