using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.Logger
{
    /// <summary>
    /// 日志工厂
    /// </summary>
    public static class LoggerFactory
    {
        #region Members
        private static ILogger _currentLogFactory = null;
        #endregion

        #region Public Methods
        /// <summary>
        ///    Set the  log factory to use
        /// </summary>
        /// <param name="logFactory">Log factory to use</param>
        public static void SetCurrent(ILogger logFactory)
        {
            _currentLogFactory = logFactory;
        }


        /// <summary>
        /// 建立一个日志对象
        /// </summary>
        /// <returns></returns>
        public static ILogger CreateLog()
        {
            if (_currentLogFactory == null)
            {
                _currentLogFactory = new EmptyLogger();
            }
            return _currentLogFactory;
        }
        #endregion
    }

}
