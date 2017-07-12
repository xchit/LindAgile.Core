using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.Exceptions
{
    /// <summary>
    /// 仓储持久化失败引发的异常
    /// </summary>
    public class PilipaRepositoryException : Exception
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="message"></param>
        public PilipaRepositoryException(string message) : base(message)
        {

        }
    }
}
