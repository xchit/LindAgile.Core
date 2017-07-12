using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace LindAgile.Core.Domain
{
    /// <summary>
    /// 领域模型，实体模型基类，它可能有多种持久化方式，如DB,File,Redis,Mongodb,XML等
    /// Lind.DDD框架的领域模型与数据库实体合二为一
    /// </summary>
    [Serializable]
    public abstract class EntityBase
    {
        #region Contructors
        /// <summary>
        /// 实体初始化
        /// </summary>
        public EntityBase()
        {
            this.DataStatus = Status.Normal;
            this.DataUpdateDateTime = DateTime.Now;
            this.DataCreateDateTime = DateTime.Now;
        }
        #endregion

        #region Properties

        /// <summary>
        /// 建立时间
        /// </summary>
        [DisplayName("建立时间")]
        public DateTime DataCreateDateTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [DisplayName("更新时间")]
        public DateTime DataUpdateDateTime { get; set; }
        /// <summary>
        /// 实体状态
        /// </summary>
        [DisplayName("状态")]
        public Status DataStatus { get; set; }
        #endregion

        #region Methods


        #endregion

    }
}
