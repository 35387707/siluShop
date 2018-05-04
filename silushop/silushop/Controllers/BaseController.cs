using RelexBarBLL;
using RelexBarDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace silushop.Controllers
{
    public class BaseController : Controller
    {
        private Users _userinfo = null;
        public Users UserInfo
        {
            get
            {
                if (_userinfo == null)
                {
                    if (Session["user"] != null)
                        _userinfo = Session["user"] as Users;
                    //else if (Request.Cookies["user"] != null)
                    //{
                    //    string user = Request.Cookies["user"].Value;
                    //    if(MD5(user+))
                    //}
                }
                return _userinfo;
            }
            set
            {
                _userinfo = value;
                Session["user"] = _userinfo;
            }
        }

        public string VerifyCode
        {
            get
            { return Session["verifycode"] != null ? Session["verifycode"].ToString() : string.Empty; }
            set
            {
                Session["verifycode"] = value;
            }
        }
        public string VerifyAccount
        {
            get
            { return Session["verifyacc"] != null ? Session["verifyacc"].ToString() : string.Empty; }
            set
            {
                Session["verifyacc"] = value;
            }
        }
        [NonAction]
        public JsonResult RJson(int code, string imsg)
        {
            return Json(new { code = code, msg = imsg },JsonRequestBehavior.AllowGet);
        }
        [NonAction]
        public string MD5(string source)
        {
            return CommonClass.EncryptDecrypt.GetMd5Hash(source + SysConfigBLL.MD5Key);
        }
        [NonAction]
        public string encodeHtml(string content)
        {
            if (content.IndexOf("<") != -1)
            {
                content = content.Replace("<", "~lt");
            }
            if (content.IndexOf(">") != -1)
            {
                content = content.Replace(">", "~gt");
            }
            return content;
        }
        [NonAction]
        public int GetTotalPage(int pageSize, int Sum)
        {
            return ((Sum - 1) / pageSize + 1);
        }
        [NonAction]
        protected bool CheckPayPSW(string psw)
        {
            UsersBLL bll = new UsersBLL();
            int result = bll.CheckPay(UserInfo.ID, psw);
            return result > 0;
        }
        [NonAction]
        public ActionResult AlertAndLinkTo(string msg, string url)
        {
            return Content("<script>alert(\"" + msg + "\");location.href='" + url + "';</script>");
        }
        [NonAction]
        public object IsNULL(object o, object def)
        {
            if (o == null)
            {
                return def;
            }
            return o;
        }
    }
}