using FluentValidation;
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
    public abstract class RequestMsgBase
    {
        /// <summary>
        /// 请求初始化
        /// </summary>
        public RequestMsgBase()
        {
            queuePredicate = i =>
            i.Name != "ContainFields" &&
            i.Name != "PageIndex" &&
            i.Name != "PageSize" &&
            i.Name != "Sort" &&
            i.GetValue(this) != null;

            PageSize = 10;
            PageIndex = 1;
        }

        /// <summary>
        /// 以属性作为查询条件,去掉为空的属性和公用属性
        /// </summary>
        private Func<PropertyInfo, bool> queuePredicate;

        #region 公用属性，不进行参数过滤

        /// <summary>
        /// 当前请求第几页
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 返回多少条
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 排序相关 1 升序,-1 降序
        /// 例：Sort=email|1,username-1
        /// </summary>
        public string Sort { get; set; }
        /// <summary>
        /// 需要返回的字段，其它字段将不会被序列化，这些字段使用,分开
        /// 例：ContainFields=username,realname,email
        /// </summary>
        public string ContainFields { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// 得到对象的属性，以键值对的方式返回
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, object>> GetProperyiesDictionary()
        {
            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .Where(queuePredicate)
                     .ToArray();

            foreach (var i in properties)
                yield return new KeyValuePair<string, object>(i.Name, i.GetValue(this));

        }

        /// <summary>
        /// 得到排序参数字典，以键值对的方式返回
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, object>> GetSortDictionary()
        {
            if (!string.IsNullOrWhiteSpace(Sort))
            {
                var sortFields = Sort.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in sortFields)
                {
                    var values = item.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (values.Length == 2 && (Convert.ToInt32(values[1]) == 1 || Convert.ToInt32(values[1]) == -1))
                    {
                        int val;
                        int.TryParse(values[1], out val);
                        if (val == 1 || val == -1)
                            yield return new KeyValuePair<string, object>(values[0], val);
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 请求体校验－与实体解耦
    /// </summary>
    public class RequestMsgBaseValidator : AbstractValidator<RequestMsgBase>
    {
        public RequestMsgBaseValidator()
        {
            RuleFor(o => o.PageIndex).GreaterThanOrEqualTo(0).WithMessage("页码应该是大于0的数字！");
            RuleFor(o => o.PageSize).GreaterThanOrEqualTo(0).WithMessage("每页显示记录数应该是大于0的数字！");
        }
    }
}
