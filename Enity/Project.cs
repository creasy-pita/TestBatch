using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Enity
{
    /// <summary>
    /// root根节点
    /// </summary>
    [XmlType(TypeName = "root")]
    public class Project
    {
        [XmlElement("SQL")]
        public string SQL { get; set; }
        [XmlElement("Project")]
        public List<Projects> projects { get; set; }
    }
    /// <summary>
    /// 电子证照同步工具/Project.xml/root/Project节点实例
    /// </summary>
    [XmlType(TypeName = "Project")]
    public class Projects
    {
        /// <summary>
        /// 方案名称
        /// </summary>
        [XmlAttribute]
        public string name { get; set; }
        /// <summary>
        /// 同步周期
        /// </summary>
        [XmlElement("Cycle")]
        public string synchron_cycle { get; set; }
        /// <summary>
        /// 内网数据库地址
        /// </summary>
        [XmlElement("FromDataSource")]
        public string inner_source { get; set; }
        /// <summary>
        /// 内网数据库名
        /// </summary>
        [XmlElement("FromUserName")]
        public string inner_name { get; set; }
        /// <summary>
        /// 内网数据库密码
        /// </summary>
        [XmlElement("FromUserPassWord")]
        public string inner_pwd { get; set; }
        /// <summary>
        /// 外网数据库地址
        /// </summary>
        [XmlElement("TargetDataSourceName")]
        public string out_source { get; set; }
        /// <summary>
        /// 外网数据库用户名
        /// </summary>
        [XmlElement("TargetUserName")]
        public string out_name { get; set; }
        /// <summary>
        /// 外网数据库用户密码
        /// </summary>
        [XmlElement("TargetUserPassWord")]
        public string out_pwd { get; set; }
    }
}
