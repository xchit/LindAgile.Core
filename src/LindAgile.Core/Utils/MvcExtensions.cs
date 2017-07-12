using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Collections.Specialized;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class MvcExtensions
    {
        #region 产生分页标记（前台JS,Bootstrape分页）

        /// <summary>
        /// 生成分页脚本块
        /// 当前页page和每页条目pagesize都在url上进行传递
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public static string GeneratePagger(IHtmlHelper htmlHelper, int totalRecords, NameValueCollection nv = null)
        {

            int page = 0;
            int pageSize = 10;
　         
            RouteValueDictionary route = new RouteValueDictionary();
            if (nv != null)
            {
                if (nv["pageSize"] != null)
                    int.TryParse(nv["pageSize"], out pageSize);//为页数赋值

                if (nv["page"] != null)
                    int.TryParse(nv["page"], out page);//页码

                foreach (string item in nv.Keys)
                    route.Add(item, nv[item]);

            }
            if (page <= 0) page = 1;

            string controller = htmlHelper.ViewContext.RouteData.Values["controller"].ToString();
            string action = htmlHelper.ViewContext.RouteData.Values["action"].ToString();
            //当前页,前面和后面显示的页数
            int showNum = 2;
            //最前面和最后面显示的页数
            int lastNum = 0;
            int totalPage = (int)Math.Ceiling((double)((double)totalRecords / (double)pageSize));
            if (page > totalPage) page = totalPage;
            int prevPage = page > 1 ? page - 1 : 1;
            int nextPage = page == totalPage ? totalPage : page + 1;
            long startNum = 0;
            long endNum = 0;

            startNum = pageSize * (page - 1) + 1;
            if (totalPage == page)
            {
                endNum = totalRecords;
            }
            else
            {
                endNum = pageSize * page;
            }
            var url = new UrlHelper(htmlHelper.ViewContext);
            StringBuilder sb = new StringBuilder();
            sb.Append("<nav>");
            sb.Append("<ul class=\"pagination  pagination-sm\">");
            var first = new RouteValueDictionary(route);
            first["page"] = 1;
            var prev = new RouteValueDictionary(route);
            prev["page"] = prevPage;
            var next = new RouteValueDictionary(route);
            next["page"] = nextPage;
            var end = new RouteValueDictionary(route);
            end["page"] = totalPage;
            sb.Append("<li><a href=\"" + url.Action(action, controller, first) + "\">首页</a></li>");

            sb.Append("<li><a href=\"" + url.Action(action, controller, prev) + "\" aria-label=\"Previous\"><span aria-hidden=\"true\">&laquo;</span></a></li>");
            #region 遍历分页页签
            if (page <= showNum + lastNum + 1)
            {

                for (int i = 1; i < page; i++)
                {
                    var temp = new RouteValueDictionary(route);
                    temp["page"] = i;
                    sb.Append("<li><a href=\"" + url.Action(action, controller, temp) + "\">" + i + "</a></li>");
                }
            }
            else
            {
                for (int i = 1; i <= lastNum; i++)
                {
                    var temp = new RouteValueDictionary(route);
                    temp["page"] = i;

                    sb.Append("<li><a href=\"" + url.Action(action, controller, temp) + "\">" + i + "</a></li>");
                }

                sb.Append("<li><a>…</a></li>");

                for (int i = page - showNum; i < page; i++)
                {
                    var temp = new RouteValueDictionary(route);
                    temp["page"] = i;

                    sb.Append("<li><a href=\"" + url.Action(action, controller, temp) + "\">" + i + "</a></li>");
                }

            }


            if (!(page == 1 && totalPage == 1))
                sb.Append("<li class=\"active\"><span>" + page + "<span class=\"sr-only\">(current)</span></span></li>");


            //后半部分
            for (int i = page + 1; i <= page + showNum && i <= totalPage; i++)
            {
                var temp = new RouteValueDictionary(route);
                temp["page"] = i;
                sb.Append("<li><a href=\"" + url.Action(action, controller, temp) + "\">" + i + "</a></li>");
            }

            if (page + showNum + lastNum < totalPage)
            {


                for (int i = totalPage - lastNum + 1; i <= totalPage; i++)
                {
                    var temp = new RouteValueDictionary(route);
                    temp["page"] = i;
                    sb.Append("<li><a href=\"" + url.Action(action, controller, temp) + "\">" + i + "</a></li>");
                }
                sb.Append("<li><a>…</a></li>");
            }
            else
            {

                for (int i = page + showNum + 1; i <= totalPage; i++)
                {
                    var temp = new RouteValueDictionary(route);
                    temp["page"] = i;
                    sb.Append("<li><a href=\"" + url.Action(action, controller, temp) + "\">" + i + "</a></li>");
                }

            }
            #endregion

            sb.Append("<li><a href=\"" + url.Action(action, controller, next) + "\" aria-label=\"Next\"><span aria-hidden=\"true\">&raquo;</span></a></li>");
            sb.Append("<li><a href=\"" + url.Action(action, controller, end) + "\">尾页</a></li>");
            sb.Append("<li><a title='" + totalRecords + "'>共" + totalRecords + "条记录</a></li>");
            sb.Append("</ul>");
            sb.Append("</nav>");
            return sb.ToString();
        }
        #endregion
    }
}
