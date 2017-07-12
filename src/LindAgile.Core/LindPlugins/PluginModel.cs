using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.LindPlugins
{
    /// <summary>
    /// 插件模型
    /// Author:Lind
    /// 可以被持久化到数据库里，方便松插拨
    /// 根据数据库的值，生产对应的实例
    /// </summary>
    public class PluginModel
    {
        public PluginModel()
        {
            Id = Guid.NewGuid().ToString();
        }
        public string Id { get; set; }
        /// <summary>
        /// 模块名称：对插件进行分类管理
        /// </summary>
        public string ModuleName { get; set; }
        /// <summary>
        /// 类型显示名称：中文名称，UI上显示用的
        /// </summary>
        public string TypeDescription { get; set; }
        /// <summary>
        /// 类型名称,模块下面的类型列表,一个模块可以有多种类型,可以做为Controller名称进行模块动态设计
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 类型完整路径，命令名称+类名
        /// </summary>
        public string TypeFullName { get; set; }
    }
}
