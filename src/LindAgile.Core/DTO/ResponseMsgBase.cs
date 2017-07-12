using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.DTO
{
    /// <summary>
    /// 请求体基类
    /// </summary>
    public class ResponseMsgBase
    {
        /// <summary>
        /// 状态码
        /// 1，0失败
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 业务错误代码
        /// </summary>
        public string ErrorCode { get; set; }
        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// 返回记录的总条数
        /// </summary>
        public long RecordCounts { get; set; }
        /// <summary>
        /// 它通常是一个ReponseBase对象或者集合
        /// </summary>
        public Object Body { get; set; }
    }
}
