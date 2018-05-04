using RelexBarBLL;
using RelexBarDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static RelexBarBLL.Common;

namespace silushop.Filter
{
    public class AutoLoginAttribute: System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            /*
             * 这里为了方便演示，直接在请求参数中获取了userName
             * 假设某一个Action只有Lucy可以访问。
             */
            object u = filterContext.HttpContext.Session["user"];
            if (u == null)
            {
                HttpCookie cookie = filterContext.HttpContext.Request.Cookies["token"];
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
                            if (user != null && CommonClass.EncryptDecrypt.GetMd5Hash(user.ID + user.Psw + SysConfigBLL.MD5Key) == str[1])
                            {
                                filterContext.HttpContext.Session["user"] = user;
                                new LogsBLL().InsertLog(user.ID, string.Format("用户{0}-{1}-{2}使用cookie登录成功...", user.ID, user.Phone, user.CardNumber), enLogType.Login);
                                base.OnActionExecuting(filterContext);
                                return;
                            }
                        }
                    }
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}