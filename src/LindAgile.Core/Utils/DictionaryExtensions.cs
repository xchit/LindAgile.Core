using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace System.Collections.Generic
{
    /// <summary>
    /// 关于字典类型的功能扩展，主要用在对象解析与传输上
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// 将字典类型转换为XML字符串
        /// </summary>
        /// <param name="m_values"></param>
        /// <returns></returns>
        public static string ToXml(this IDictionary<string, object> m_values)
        {
            //数据为空时不能转化为xml格式
            if (0 == m_values.Count)
            {
                throw new ArgumentException("字典数据为空!");
            }

            string xml = "<xml>";
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                //字段值不能为null，会影响后续流程
                if (pair.Value == null)
                {
                    throw new ArgumentException("字典内部含有值为null的字段!");
                }
                if (pair.Value.GetType() == typeof(string))
                {
                    xml += "<" + pair.Key + ">" + "<![CDATA[" + pair.Value + "]]></" + pair.Key + ">";
                }
                else
                {
                    xml += "<" + pair.Key + ">" + pair.Value + "</" + pair.Key + ">";
                }
            }
            xml += "</xml>";
            return xml;
        }

        /// <summary>
        /// 从XML字符串得到字典
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static IDictionary<string, object> FromXml(string xml)
        {
            var m_values = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentException("将空的xml串转换为字典不合法!");
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            XmlNode xmlNode = xmlDoc.FirstChild;//获取到根节点<xml>
            XmlNodeList nodes = xmlNode.ChildNodes;
            foreach (XmlNode xn in nodes)
            {
                XmlElement xe = (XmlElement)xn;
                m_values[xe.Name] = xe.InnerText;//获取xml的键值对到WxPayData内部的数据中
            }

            return m_values;
        }

        /// <summary>
        /// 将字典转为URL参数的格式，各值用&分开
        /// </summary>
        /// <returns></returns>
        public static string ToUrl(this IDictionary<string, object> m_values)
        {
            string buff = "";
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                if (pair.Value == null)
                {
                    throw new ArgumentException("WxPayData内部含有值为null的字段!");
                }

                if (pair.Value.ToString() != "")
                {
                    buff += pair.Key + "=" + pair.Value + "&";
                }
            }
            buff = buff.Trim('&');
            return buff;
        }

        /// <summary>
        /// 从url参数转成k/v
        /// </summary>
        /// <param name="m_values"></param>
        /// <returns></returns>
        public static IDictionary<string, string> FromUrl(string m_values)
        {
            IDictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var item in m_values.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var entity = item.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                dic.Add(entity[0], entity[1]);
            }

            return dic;
        }



        /// <summary>
        /// 将对象转为键值对象（完全支持最复杂的类型）
        /// 作者：仓储大叔
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ToDictionary(this object obj)
        {
            try
            {
                var dic = new Dictionary<string, string>();
                var prefix = new Dictionary<string, string>();
                foreach (var p in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(i => i.Name))
                {
                    ReGenerate(obj, p, prefix, dic, null);
                    prefix.Clear();
                }
                return dic;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 转成K/V
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static NameValueCollection ToNameValueCollection(this object obj)
        {
            var nv = new NameValueCollection();
            var dic = obj.ToDictionary();
            foreach (var item in dic.Keys)
            {
                nv.Add(item, dic[item]);
            }
            return nv;
        }

        /// <summary>
        /// 将字典转为URL参数的格式，各值用&分开
        /// </summary>
        /// <returns></returns>
        public static string ToUrl(this NameValueCollection m_values)
        {
            string buff = "";
            foreach (string pair in m_values.Keys)
            {

                if (!string.IsNullOrWhiteSpace(m_values[pair]))
                {
                    buff += pair + "=" + m_values[pair] + "&";
                }
            }
            buff = buff.Trim('&');
            return buff;
        }




        #region Private Methods
        /// <summary>
        /// 集合对象
        /// </summary>
        static string[] ListNameArr = { "List`1", "IList`1", "IEnumerable`1", "ICollection`1" };
        /// <summary>
        /// 递归构建K/V对象
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="p">对象里的属性</param>
        /// <param name="prefix">前缀</param>
        /// <param name="dic">要返回的字典</param>
        /// <param name="arrIndex">集合的索引</param>
        static void ReGenerate(
            object obj,
            PropertyInfo p,
            Dictionary<string, string> prefix,
            Dictionary<string, string> dic,
            Tuple<string, int> arrIndex)
        {
            if (obj != null)
            {
                if (p != null && p.PropertyType.IsByRef && p.PropertyType != typeof(string))
                {

                    var sub = p.GetValue(obj);
                    if (sub != null)
                    {
                        if (ListNameArr.Contains(p.PropertyType.Name))//集合
                        {
                            var innerList = sub as IEnumerable;
                            int j = 0;
                            prefix.Add(p.Name, p.Name);
                            foreach (var listSub in innerList)
                            {
                                //集合里是简单类型，如List<int>,List<string>
                                if (!listSub.GetType().IsByRef || listSub.GetType() == typeof(string))
                                {
                                    dic.Add(string.Join(".", prefix.Values) + "[" + j + "]", listSub.ToString());
                                }
                                //集合里是复杂类型，如List<T>
                                else
                                {
                                    foreach (var property in listSub.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                       .Where(i => i.Name != "Capacity" && i.Name != "Count"))
                                    {
                                        ReGenerate(listSub, property, prefix, dic, new Tuple<string, int>(p.Name, j));
                                    }
                                }
                                j++;

                            }
                            prefix.Remove(p.Name);//用完清除
                        }
                        else//实体
                        {
                            if (!prefix.ContainsKey(p.Name) && sub != null)
                                prefix.Add(p.Name, p.Name);

                            foreach (var property in p.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                            {
                                ReGenerate(sub, property, prefix, dic, arrIndex);
                            }
                            prefix.Remove(p.Name);//用完清除
                        }
                    }
                }
                else//简单属性
                {
                    if (p.GetValue(obj) != null)
                        if (prefix.Count > 0)
                        {
                            if (arrIndex != null)
                            {
                                foreach (var key in prefix.Keys)
                                {
                                    if (key == arrIndex.Item1)
                                    {

                                        prefix[key] = key + "[" + arrIndex.Item2 + "]";
                                        break;
                                    }
                                }
                            }
                            dic.Add(string.Join(".", prefix.Values) + "." + p.Name, p.GetValue(obj).ToString());
                        }
                        else
                            dic.Add(p.Name, p.GetValue(obj).ToString());

                }


            }
        }

        #endregion

    }
}
