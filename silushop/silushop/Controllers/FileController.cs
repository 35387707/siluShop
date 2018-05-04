using RelexBarBLL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace silushop.Controllers
{
    public class FileController : BaseController
    {
        string[] last = { "jpg", "jpeg", "gif", "bmp", "png" };
        [Filter.CheckLogin]
        public JsonResult UserHeadUpload(HttpPostedFileBase file)
        {
            try
            {
                string[] tname = file.FileName.Split('.');
                if (!last.Contains(tname[1].ToLower()))
                {
                    return RJson(-1, "文件类型不正确");
                }
                string newfilename = Guid.NewGuid() + "." + tname[1];
                string serverPath = Server.MapPath("/");
                string dirPath = "/upload/" + DateTime.Now.ToString("yyyyMMdd");
                string uploadPath = dirPath + "/" + newfilename;
                string fullPath = serverPath + uploadPath;

                if (!Directory.Exists(serverPath + dirPath))
                {
                    Directory.CreateDirectory(serverPath + dirPath);
                }
                Image upimg = Bitmap.FromStream(file.InputStream);
                //获取头像宽高
                int w = upimg.Width;
                int h = upimg.Height;
                Bitmap bmPhoto = null;
                if (w > h)
                {
                    Rectangle section = new Rectangle(new Point(((w - h) / 2), 0), new Size(h, h));
                    bmPhoto = CutImage(new Bitmap(upimg), section);
                }
                else if (w < h)
                {
                    Rectangle section = new Rectangle(new Point(0, ((h - w) / 2)), new Size(w, w));
                    bmPhoto = CutImage(new Bitmap(upimg), section);
                }
                else
                {
                    bmPhoto = new Bitmap(upimg);
                }


                Image img = bmPhoto.GetThumbnailImage(86, 86, () => { return true; }, IntPtr.Zero);
                img.Save(fullPath);
                UsersBLL bll = new UsersBLL();
                int i = bll.UpdateHead(UserInfo.ID, uploadPath);
                if (i > 0)
                {
                    UserInfo.HeadImg1 = uploadPath;
                    return RJson(1, uploadPath);
                }
                return RJson(-1, "修改失败");


            }
            catch (Exception ex)
            {
                return RJson(-1, ex.Message);
            }

        }
        public Bitmap CutImage(Bitmap sourceImage, Rectangle section)
        {
            Bitmap bmp = new Bitmap(section.Width, section.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(sourceImage, 0, 0, section, GraphicsUnit.Pixel);
            return bmp;
        }
        public JsonResult UploadHeadImg(HttpPostedFileBase file)
        {
            try
            {
                string[] tname = file.FileName.Split('.');
                if (!last.Contains(tname[1].ToLower()))
                {
                    return RJson(-1, "文件类型不正确");
                }
                string newfilename = Guid.NewGuid() + "." + tname[1];
                string serverPath = Server.MapPath("/");
                string dirPath = "/upload/" + DateTime.Now.ToString("yyyyMMdd");
                string uploadPath = dirPath + "/" + newfilename;
                string fullPath = serverPath + uploadPath;

                if (!Directory.Exists(serverPath + dirPath))
                {
                    Directory.CreateDirectory(serverPath + dirPath);
                }

                Image img = Bitmap.FromStream(file.InputStream).GetThumbnailImage(86, 86, () => { return true; }, IntPtr.Zero);
                img.Save(fullPath);
                return RJson(1, uploadPath);



            }
            catch (Exception ex)
            {
                return RJson(-1, ex.Message);
            }
        }
        public string ImgUpload2(HttpPostedFileBase wangEditorH5File)
        {
            try
            {
                string[] tname = wangEditorH5File.FileName.Split('.');
                if (!last.Contains(tname[1].ToLower()))
                {
                    return null;
                }
                string newfilename = Guid.NewGuid() + "." + tname[1];
                string serverPath = Server.MapPath("/");
                string dirPath = "/upload/" + DateTime.Now.ToString("yyyyMMdd");
                string uploadPath = dirPath + "/" + newfilename;
                string fullPath = serverPath + uploadPath;
                if (!Directory.Exists(serverPath + dirPath))
                {
                    Directory.CreateDirectory(serverPath + dirPath);
                }
                wangEditorH5File.SaveAs(fullPath);
                return uploadPath;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public JsonResult ImgUpload(HttpPostedFileBase file)
        {
            try
            {
                string[] tname = file.FileName.Split('.');
                if (!last.Contains(tname[1].ToLower()))
                {
                    return RJson(-1, "文件类型不正确");
                }
                string newfilename = Guid.NewGuid() + "." + tname[1];
                string serverPath = Server.MapPath("/");
                string dirPath = "/upload/" + DateTime.Now.ToString("yyyyMMdd");
                string uploadPath = dirPath + "/" + newfilename;
                string fullPath = serverPath + uploadPath;
                if (!Directory.Exists(serverPath + dirPath))
                {
                    Directory.CreateDirectory(serverPath + dirPath);
                }
                file.SaveAs(fullPath);
                return RJson(1, uploadPath);
            }
            catch (Exception)
            {

                throw;
            }
        }



    }
}