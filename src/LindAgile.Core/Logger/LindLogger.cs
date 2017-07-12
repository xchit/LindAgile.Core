using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LindAgile.Core.Logger
{
    /// <summary>
    /// 以普通的文字流的方式写日志
    /// </summary>
    [Serializable]
    public class LindLogger : LoggerBase
    {
        static readonly object objLock = new object();

        /// <summary>
        /// 实现基类－写日志文件的逻辑
        /// </summary>
        /// <param name="message"></param>
        protected override void InputLogger(Level level, string message)
        {
            if (string.IsNullOrWhiteSpace(FileUrl))
                FileUrl = Path.Combine(Directory.GetCurrentDirectory(), "LoggerDir");
            if (!System.IO.Directory.Exists(FileUrl))
                System.IO.Directory.CreateDirectory(FileUrl);
            string filePath = Path.Combine(FileUrl, _defaultLoggerName);

            //写日志委托
            Action<string> write = (fileName) =>
            {
                lock (objLock)//防治多线程读写冲突
                {

                    using (FileStream srFile = new FileStream(fileName, FileMode.OpenOrCreate))
                    {
                        var info = System.Text.Encoding.UTF8.GetBytes(string.Format("{0}{1}{2}{3}"
                            , DateTime.Now.ToString().PadRight(18)
                            , ("[Id:" + Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(2, '0') + "]").PadRight(6)
                            , level.ToString()
                            , message));
                        srFile.Write(info, 0, info.Length);
                    }
                }
            };
            Console.WriteLine(message);
            try
            {

                write(filePath + ".log");
            }
            catch (Exception)
            {
                write(filePath + Process.GetCurrentProcess().Id + ".log");
            }

        }

    }
}
