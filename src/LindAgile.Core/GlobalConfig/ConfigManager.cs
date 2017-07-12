using System.Collections.Generic;
using System.IO;
using LindAgile.Core.GlobalConfig.Models;
using Newtonsoft.Json;
using System.Text;

namespace LindAgile.Core.GlobalConfig
{
    /// <summary>
    /// 框架级配置信息初始化，默认使用xml进行存储
    /// </summary>
    public class ConfigManager
    {
        #region Constructors & Fields
        private static ConfigModel _config;
        private static string _fileName = Path.Combine(Directory.GetCurrentDirectory(), "ConfigConstants.json");
        private static object _lockObj = new object();

        static ConfigManager()
        {
            _init = new ConfigModel();
            _init.Caching.ExpireMinutes = 20;
            _init.Caching.Provider = "RedisCache";
            _init.Logger.Level = "DEBUG";
            _init.Logger.ProjectName = "Lind.DDD";
            _init.Logger.Type = "File";
            _init.MongoDB.DbName = "Test";
            _init.MongoDB.Host = "localhost:27017";
            _init.MongoDB.UserName = string.Empty;
            _init.MongoDB.Password = string.Empty;
            _init.Redis.Host = "localhost:6379";
            _init.Redis.Proxy = 0;

            string[] blacklist = {
                                     "System",
                                     "StackExchange",
                                     "Microsoft",
                                     "Autofac",
                                     "Quartz",
                                     "EntityFramework",
                                     "MySql",
                                     "MongoDB",
                                     "log4net",
                                     "AutoMapper",
                                     "NPOI",
                                     "CrystalQuartz",
                                     "Gma.QrCodeNet",
                                     "HtmlAgilityPack",
                                     "Common.Logging",
                                     "NetPay",
                                     "ServiceStack",
                                     "Newtonsoft.Json",
                                     "Robots",
                                     "CsQuery",
                                     "Dapper"};
        }
        /// <summary>
        /// 模型初始化
        /// </summary>
        private static ConfigModel _init;
        #endregion

        /// <summary>
        /// 配置字典,单例模式
        /// </summary>
        /// <returns></returns>
        public static ConfigModel Config
        {
            get
            {

                if (_config == null)
                {
                    lock (_lockObj)
                    {

                        ConfigModel old = null;
                        try
                        {
                            using (FileStream fs = new FileStream(_fileName, FileMode.OpenOrCreate))
                            {
                                using (StreamReader sw = new StreamReader(fs, Encoding.UTF8))
                                {
                                    old = JsonConvert.DeserializeObject<ConfigModel>(sw.ReadToEnd());
                                }
                            }

                        }
                        catch (System.Exception)
                        {
                            //读文件出错
                        }

                        if (old != null)
                        {
                            _config = old;
                        }
                        else
                        {
                            using (FileStream fs = new FileStream(_fileName, FileMode.OpenOrCreate))
                            {
                                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                                {
                                    sw.Write(JsonConvert.SerializeObject(_init));
                                }

                            }
                            _config = _init;
                        }

                    }

                }
                return _config;

            }
        }
    }
}
