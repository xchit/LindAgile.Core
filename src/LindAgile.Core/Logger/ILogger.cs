using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.Logger
{
    /// <summary>
    /// 日志功能接口规范
    /// </summary>
    public interface ILogger
    {
        #region 级别日志
        /// <summary>
        /// 将message记录到日志文件
        /// </summary>
        /// <param name="message"></param>
        void Logger_Info(string message);
        /// <summary>
        /// 异常发生的日志
        /// </summary>
        /// <param name="message"></param>
        void Logger_Error(string ex);
        /// <summary>
        /// 调试期间的日志
        /// </summary>
        /// <param name="message"></param>
        void Logger_Debug(string message);
        /// <summary>
        /// 引起程序终止的日志
        /// </summary>
        /// <param name="message"></param>
        void Logger_Fatal(string message);
        /// <summary>
        /// 引起警告的日志
        /// </summary>
        /// <param name="message"></param>
        void Logger_Warn(string message);
        #endregion

        /// <summary>
        /// 日志二级路径或者模块名称
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        ILogger SetPath(string path);
    }
}
