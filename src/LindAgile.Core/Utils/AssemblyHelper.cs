using LindAgile.Core.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.Utils
{
    /// <summary>
    /// 程序集相关帮助类
    /// </summary>
    public class AssemblyHelper
    {
        /// <summary>
        /// 黑名单
        /// </summary>
        static string[] BlackList = { "LindAgile.Core", "System", "FluentValidation", "StackExchange", "Microsoft", "Autofac", "Quartz", "EntityFramework", "MySql", "MongoDB", "log4net", "AutoMapper", "NPOI", "CrystalQuartz", "Gma.QrCodeNet", "HtmlAgilityPack", "Common.Logging", "NetPay", "ServiceStack", "Newtonsoft.Json", "LitJson", "Robots", "CsQuery" };
        static object lockObj = new object();
        static List<Type> modelList = new List<Type>();
        static AssemblyHelper() { Init(); }

        /// <summary>
        /// 初始化
        /// </summary>
        static void Init()
        {
            lock (lockObj)
            {

                lock (lockObj)
                {
                    var logger = LoggerFactory.CreateLog();
                    var pathList = new List<string> { };
                    List<Assembly> AssemblyList = new List<Assembly>();
                    string path = Directory.GetCurrentDirectory();
                    var bin = Directory.GetDirectories(path).FirstOrDefault(i => i.EndsWith("bin", StringComparison.CurrentCultureIgnoreCase));
                    if (bin != null)
                    {
                        var debug = Directory.GetDirectories(bin).FirstOrDefault(i => i.EndsWith("debug", StringComparison.CurrentCultureIgnoreCase));
                        if (debug != null)
                        {
                            var core = Directory.GetDirectories(debug).FirstOrDefault(i => i.Contains("netcoreapp"));
                            if (core != null)
                                pathList.Add(core);
                            else
                                pathList.Add(debug);
                        }
                        else
                        {
                            pathList.Add(bin);
                        }
                    }
                    else
                        pathList.Add(path);

                    try
                    {
                        //当前运行的项目dll
                        modelList.AddRange(Assembly.GetEntryAssembly().ExportedTypes);
                        foreach (var _path in pathList)
                        {
                            foreach (var dir in Directory.GetFiles(_path)
                                            .Where(i => i.Contains(Assembly.GetEntryAssembly().FullName))
                                            .Where(i => i.EndsWith("dll", StringComparison.CurrentCultureIgnoreCase)
                                            || i.EndsWith("exe", StringComparison.CurrentCultureIgnoreCase)))
                            {
                                //当前bin下的程序集
                                var dll = AssemblyLoadContext.Default.LoadFromAssemblyPath(dir);

                                if (BlackList.Where(j => dll.FullName.StartsWith(j, StringComparison.CurrentCultureIgnoreCase)).Count() == 0)
                                    modelList.AddRange(dll.ExportedTypes);
                            }
                        }
                    
                    }
                    catch (Exception ex)
                    {
                        logger.Logger_Debug("Plugin error\r\n" + ex.Message + ex.StackTrace);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 某个类型是否派生于别一个类型，支持泛型接口
        /// </summary>
        /// <param name="subType"></param>
        /// <param name="basicType"></param>
        /// <returns></returns>
        static bool IsAssignableToGenericType(Type subType, Type basicType)
        {
            if (basicType.IsAssignableFrom(subType))
                return true;

            var interfaceTypes = subType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsConstructedGenericType && it.GetGenericTypeDefinition() == basicType)
                    return true;
            }

            if (subType.IsConstructedGenericType && subType.GetGenericTypeDefinition() == basicType)
                return true;

            Type baseType = subType.DeclaringType;
            if (baseType == null) return false;

            return false;
        }


        /// <summary>
        /// 得到BIN下面加载程序集里，实现某接口的类型集合
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypesByInterfaces(Type @interface)
        {
            if (modelList != null)
                return modelList.Where(x => IsAssignableToGenericType(x, @interface));
            else
                return Enumerable.Empty<Type>();
        }

        /// <summary>
        /// 得到BIN下面加载程序集里，实现某接口的类型名称的集合
        /// </summary>
        /// <param name="interface"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetTypeNamesByInterfaces(Type @interface)
        {
            return GetTypesByInterfaces(@interface).Select(i => i.Name);
        }


        /// <summary>
        /// 根据接口，返回程序运行时里所有实现它的类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="interface"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetCurrentTypesByInterfaces<T>(Type @interface)
        {
            var handlers = @interface.GetNestedTypes(BindingFlags.Public);
            return handlers;
        }

    }
}
