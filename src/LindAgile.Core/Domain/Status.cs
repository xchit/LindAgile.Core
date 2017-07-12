using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.Domain
{

    /// <summary>
    /// 实体状态
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// 正常
        /// </summary>
        [Description("正常")]
        Normal = 1,
        /// <summary>
        /// 隐藏
        /// </summary>
        [Description("隐藏")]
        Hidden = 2,
        /// <summary>
        /// 删除
        /// </summary>
        [Description("删除")]
        Deleted = 3,
        /// <summary>
        /// 冻结
        /// </summary>
        [Description("冻结")]
        Freeze = 4,
    }
}
