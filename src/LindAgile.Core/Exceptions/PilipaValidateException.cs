using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.Exceptions
{
    /// <summary>
    /// 验证失败引发的异常
    /// </summary>
    public class PilipaValidateException : Exception
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="message"></param>
        public PilipaValidateException(string message) : base(message)
        {

        }
    }
}
