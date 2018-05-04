using RelexBarBLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace silushop.Controllers
{
    public class AreaController : BaseController
    {
       

        //通过父id获得地址
        public JsonResult GetAreas(int fid)
        {
            WebAreaBll bll = new WebAreaBll();
            return Json(bll.Areas(fid));
        }
        /// <summary>
        /// 新增收货地址
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sex"></param>
        /// <param name="phone"></param>
        /// <param name="qu"></param>
        /// <param name="detail"></param>
        /// <returns></returns>
        [Filter.CheckLogin]
        public JsonResult EditAddress(Guid? id, string name, int sex, string phone, int qu, string detail)
        {
            RecAddressBLL bll = new RecAddressBLL();
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(detail))
            {
                return RJson(-1, "参数错误");
            }
            sex = sex == 0 ? 0 : 1;
            if (id == null)
            {
                int i = bll.Insert(UserInfo.ID, name, sex, phone, qu, detail);

                if (i > 0)
                {
                    return RJson(1, "添加成功");
                }
                else
                {
                    return RJson(-1, "添加失败");
                }
            }
            else
            {
                int i = bll.Update(UserInfo.ID, id.Value, name, sex, phone, qu, detail);
                if (i > 0)
                {
                    return RJson(1, "修改成功");
                }
                else
                {
                    return RJson(-1, "修改失败");
                }
            }

        }
    }
}