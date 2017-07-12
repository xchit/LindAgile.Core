using AutoMapper;
using MongoDB.Bson;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    /// <summary>
    /// AutoMapper映射扩展方法
    /// </summary>
    public static partial class AutoMapExtensions
    {
        /// <summary>#E
        /// 为集合进行automapper
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> MapTo<TResult>(this IEnumerable self)
        {
            if (self == null)
                throw new ArgumentNullException();
            return self.MapTo<TResult>();
        }

        /// <summary>
        /// 为新对象进行automapper
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static TResult MapTo<TResult>(this object self)
        {
            if (self == null)
                throw new ArgumentNullException();
            return self.MapTo<TResult>();
        }

        /// <summary>
        /// 为已经存在的对象进行automapper
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="self"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static TResult MapTo<TResult>(this object self, TResult result)
        {
            if (self == null)
                throw new ArgumentNullException();
            return self.MapTo<TResult>(result);

        }

    }
}
