using RelexBarBLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace silushop.Controllers
{
    public class VerifycodeController : BaseController
    {
        [Filter.CheckLogin]
        public void getMyVerifyCode() {
            SmsVerifyCode(UserInfo.Name);
        }

        [Filter.NoFilter]
        public void getVerifyCode(string t, string r)
        {
            if (string.IsNullOrEmpty(t) || t == "1")//图片验证码
            {
                var q = ImgVerifyCode();

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    Response.ClearContent();
                    Response.ContentType = "image/jpeg";
                    q.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    Response.BinaryWrite(ms.ToArray());
                    q.Dispose();
                    Response.End();
                }
            }
            else if (t == "2")//手机验证码
            {
                if (Common.IsPhone(r))
                {
                    Session["Phone"] = r;
                    SmsVerifyCode(r);
                    WriteSuccess();
                }
                else
                {
                    WriteFaild();
                }
            }
            else if (t == "3")//邮箱验证码
            {
                if (Common.IsPhone(r))
                {
                    EmailVerifyCode(r);
                    WriteSuccess();
                }
                else
                {
                    WriteFaild();
                }
            }
        }

        public System.Drawing.Bitmap ImgVerifyCode()
        {
            string code;
            var q = Common.SendImgVerify(out code, 4);
            VerifyCode = code;

            VerifyCodesBLL verbll = new VerifyCodesBLL();
            verbll.InsertCode(UserInfo == null ? Guid.Empty : UserInfo.ID, code, Common.enCodeType.Img);

            return q;
        }
        public void SmsVerifyCode(string recphone)
        {
            string code;
            Common.SendSmsVerify(out code, recphone);
            VerifyCode = code;

            VerifyCodesBLL verbll = new VerifyCodesBLL();
            verbll.InsertCode(UserInfo != null ? UserInfo.ID : Guid.Empty, code, Common.enCodeType.SMS);
        }
        public void EmailVerifyCode(string recEmail)
        { }
        public void WriteSuccess()
        {
            Response.Write("1");
        }
        public void WriteFaild()
        { Response.Write("0"); }
    }
}