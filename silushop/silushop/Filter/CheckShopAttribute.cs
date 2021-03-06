﻿using RelexBarBLL;
using RelexBarDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace silushop.Filter
{
    public class CheckShopAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            /*
             * 这里为了方便演示，直接在请求参数中获取了userName
             * 假设某一个Action只有Lucy可以访问。
             */
            object u = filterContext.HttpContext.Session["shop"];
            object[] attrs = filterContext.ActionDescriptor.GetCustomAttributes(typeof(NoFilter), true);
            if (u == null && attrs.Length != 1)
            {
                HttpCookie cookie = filterContext.HttpContext.Request.Cookies["shopToken"];
                if (cookie != null)
                {
                    string[] str = cookie.Value.Split('|');
                    if (str.Length == 2)
                    {
                        Guid uid = Guid.Empty;
                        if (Guid.TryParse(str[0], out uid))
                        {
                            UsersBLL bll = new UsersBLL();
                            Users user = bll.GetUserById(uid);
                            if (user != null&&user.UserType==(int)Common.enUserType.Shop&& CommonClass.EncryptDecrypt.GetMd5Hash(user.ID + user.Psw + SysConfigBLL.MD5Key) == str[1])
                            {
                                filterContext.HttpContext.Session["shop"] = user;
                                filterContext.HttpContext.Session["user"] = user;
                                base.OnActionExecuting(filterContext);
                                return;
                            }
                        }
                    }
                }
                //filterContext.Result = new RedirectToRouteResult("Default", new System.Web.Routing.RouteValueDictionary(new Dictionary<string, object>() { { "controller", "Home" }, { "action", "Login" } }, true));
                filterContext.HttpContext.Response.Redirect("/Shop/Login");
                return;



            }
            base.OnActionExecuting(filterContext);
        }
    }
}