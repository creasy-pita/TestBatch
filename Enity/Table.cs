using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Enity
{
    /// <summary>
    ///Table.xml/root节点实例
    /// </summary>
    [XmlType(TypeName = "root")]
    public class Table
    {
        [XmlElement("Table")]
        public List<Tables> tables { get; set; }
    }
    /// <summary>
    /// 电子证照同步工具/Project.xml/root/Table节点实例
    /// </summary>
    [XmlType(TypeName = "Table")]
    public class Tables
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlText]
        public string Table { get; set; }
    }
}