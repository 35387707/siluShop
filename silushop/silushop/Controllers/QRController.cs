using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RelexBarBLL;

namespace silushop.Controllers
{
    public class QRController : BaseController
    {
        [Filter.CheckLogin]
        public void getMyQRcode()
        {
            var q = RelexBarBLL.Common.GetQrCodeImgAndLogo("http://" + Request.Url.Authority + "/account/Register?fid=" + UserInfo.ID
                    , Server.MapPath(string.IsNullOrEmpty(UserInfo.HeadImg1) ? "/img/defaulthead.jpg" : UserInfo.HeadImg1));
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

        [Filter.CheckLogin]
        public void getMyQRcodeFile()
        {
            var q = RelexBarBLL.Common.GetQrCodeImgAndLogo("http://" + Request.Url.Authority + "/account/Register?fid=" + UserInfo.ID
                    , Server.MapPath(string.IsNullOrEmpty(UserInfo.HeadImg1) ? "/img/defaulthead.jpg" : UserInfo.HeadImg1));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Response.ClearContent();
                Response.AddHeader("Content-Disposition", "attachment; filename=" + UserInfo.Name+".jpg");
                q.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                Response.BinaryWrite(ms.ToArray());
                q.Dispose();
                Response.End();
            }
        }
    }
}