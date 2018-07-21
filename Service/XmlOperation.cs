using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Services
{
    /// <summary>
    /// XML操作类
    /// </summary>
    public class XmlOperation
    {
        /// <summary>
        /// XML实例化
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="type">XML实例化对象</param>
        /// <returns>返回的值需要强制转化为实例化对象</returns>
        public object LoadXML(string filePath, Type type)
        {
            object result = null;
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(type);
                    result = xmlSerializer.Deserialize(reader);
                }
            }
            return result;
        }
        /// <summary>
        /// 修改XML文件,此方法的XML节点和实例化对象的节点必须匹配，否则会造成XML节点丢失
        /// </summary>
        /// <param name="filepath">XML文件路径</param>
        /// <param name="type">XML实例化对象</param>
        /// <param name="obj">待修改的实例化对象值</param>
        /// <returns></returns>
        public bool SaveXML(string filepath, Type type, object obj)
        {
            bool result = false;
            using (StreamWriter writer = new StreamWriter(filepath))
            {
                try
                {
                    XmlSerializer xs = new XmlSerializer(type);
                    xs.Serialize(writer, obj);
                    result = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return result;
        }
    }
}