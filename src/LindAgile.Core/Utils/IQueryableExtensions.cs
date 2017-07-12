//===================================================================================
// Microsoft Developer & Platform Evangelism
//=================================================================================== 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//===================================================================================
// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// This code is released under the terms of the MS-LPL license, 
// http://microsoftnlayerapp.codeplex.com/license
//===================================================================================
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Linq
{

    /// <summary>
    /// Class for IQuerable extensions methods
    /// <remarks>
    /// Include method in IQueryable ( base contract for IObjectSet ) is 
    /// intended for mock Include method in ObjectQuery{T}.
    /// Paginate solve not parametrized queries issues with skip and take L2E methods
    /// </remarks>
    /// </summary>
    public static class IQueryableExtensions
    {
        #region 扩展方法
        /// <summary>
        /// 随机排序扩展方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<T> OrderByNewId<T>(this IEnumerable<T> source)
        {
            return source.AsQueryable().OrderBy(d => Guid.NewGuid().ToString());
        }
        #endregion
        /// <summary>
        /// 按或进行位运算
        /// 作者：仓储大叔
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static ulong BinaryOr<TSource>(this IEnumerable<TSource> source, Func<TSource, ulong> selector)
        {
            ulong result = 0;
            foreach (var item in source)
            {
                result |= selector(item);
            }
            return result;
        }
        public static uint BinaryOr<TSource>(this IEnumerable<TSource> source, Func<TSource, uint> selector)
        {
            uint result = 0;
            foreach (var item in source)
            {
                result |= selector(item);
            }
            return result;
        }
        public static int BinaryOr<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            int result = 0;
            foreach (var item in source)
            {
                result |= selector(item);
            }
            return result;
        }
        /// <summary>
        /// 按或进行位运算
        /// 作者：仓储大叔
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static long BinaryOr<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            long result = 0;
            foreach (var item in source)
            {
                result |= selector(item);
            }
            return result;
        }
        #region Extension Methods


        /// <summary>
        /// 每次处理的记录数据
        /// </summary>
        const int DATAPAGESIZE = 10000;
        /// <summary>
        /// 对iqueryable结果每次分批ToList，防止大数量时的内存占用过高的问题
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="method"></param>
        public static void QueryablePageProcess<TEntity>(this IQueryable<TEntity> queryable, Action<IEnumerable<TEntity>> method) where TEntity : class
        {
            int DataTotalCount = 0;
            int DataTotalPages = 0;
            if (queryable != null && queryable.Count() > 0)
            {
                DataTotalCount = queryable.Count();
                DataTotalPages = queryable.Count() / DATAPAGESIZE;
                if (DataTotalCount % DATAPAGESIZE > 0)
                    DataTotalPages += 1;
                for (int pageIndex = 1; pageIndex <= DataTotalPages; pageIndex++)
                {
                    var currentItems = queryable.Skip((pageIndex - 1) * DATAPAGESIZE).Take(DATAPAGESIZE).ToList();
                    method(currentItems);
                }
            }
        }
        /// <summary>
        /// 对IEnumerable,IList,ICollection等本地结果集分批处理
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="method"></param>
        public static void EnumerablePageProcess<TEntity>(this IEnumerable<TEntity> enumerable, Action<IEnumerable<TEntity>> method) where TEntity : class
        {
            enumerable.AsQueryable().QueryablePageProcess(method);
        }
        #endregion


        /// <summary>
        /// 并行分页处理数据，提高系统利用率，提升系统性能
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="item"></param>
        /// <param name="method"></param>
        public async static Task DataPageProcessAsync<T>(IQueryable<T> item, Action<IEnumerable<T>> method) where T : class
        {
            await Task.Run(() =>
            {
                DataPageProcess<T>(item, method);
            });
        }

        /// <summary>
        /// 在主线程上分页处理数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="method"></param>
        public static void DataPageProcess<T>(IQueryable<T> item, Action<IEnumerable<T>> method) where T : class
        {
            if (item != null && item.Count() > 0)
            {
                var DataPageSize = 100;
                var DataTotalCount = item.Count();
                var DataTotalPages = item.Count() / DataPageSize;
                if (DataTotalCount % DataPageSize > 0)
                    DataTotalPages += 1;

                for (int pageIndex = 1; pageIndex <= DataTotalPages; pageIndex++)
                {
                    var currentItems = item.Skip((pageIndex - 1) * DataPageSize).Take(DataPageSize).ToList();
                    method(currentItems);
                }
            }
        }

    }
}
