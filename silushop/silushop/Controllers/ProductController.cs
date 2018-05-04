using RelexBarBLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace silushop.Controllers
{
    public class ProductController : BaseController
    {

        //获取商品编号
        public JsonResult GetProductNum()
        {
            return Json(new{ code=1,msg=RelexBarBLL.Common.GetNumer() });
        }
        //已收货
        [Filter.CheckLogin]
        public JsonResult RecOrder(Guid id)
        {
            OrdersBLL bll = new OrdersBLL();
            int i = bll.RecOrder(id, UserInfo.ID);
            if (i > 0)
            {
                return RJson(1, "收货成功");
            }
            else
            {
                return RJson(-1, ((Common.ErrorCode)i).ToString());
            }
        }
    }
}