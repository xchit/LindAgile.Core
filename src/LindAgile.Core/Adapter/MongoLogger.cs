using LindAgile.Core.Domain;
using LindAgile.Core.GlobalConfig;
using LindAgile.Core.Logger;
using LindAgile.Core.NoSql;
using System;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace LindAgile.Core.Adapter
{
    /// <summary>
    /// 日志表
    /// </summary>
    public class GlobalLogger : EntityString
    {
        public GlobalLogger()
        {
            this.CurrentUserName = "";//当前登陆的用户
        }
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; }
        /// <summary>
        /// 线种ID，用户不需传递
        /// </summary>
        public int ThreadId { get; set; }
        /// <summary>
        /// 日志级别:debug,info,warn,error,fatal
        /// </summary>
        public string Level { get; set; }
        /// <summary>
        /// 日志主要内容
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 当前用户名
        /// </summary>
        public string CurrentUserName { get; set; }
        /// <summary>
        /// 来源地址
        /// </summary>
        public string FromUri { get; set; }
    }
    /// <summary>
    /// 使用MongoDB进行日志持久化
    /// </summary>
    public class MongoLogger : LoggerBase
    {
        protected override void InputLogger(Level level, string message)
        {
            var entity = new GlobalLogger
            {
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                Level = level.ToString(),
                Message = message,
                ProjectName =ConfigManager.Config.Logger.ProjectName ?? string.Empty,
            };

            MongodbManager<GlobalLogger>.Instance.InsertOne(entity);
        }

    }
}
