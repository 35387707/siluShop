using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RelexBarBLL;
using RelexBarDLL;
using silushop.Utils;

namespace silushop.Controllers
{
    [Filter.CheckLogin]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ProductsBLL bll = new ProductsBLL();
            List<ProductList> list = bll.GetProductList(Common.enProductType.Virtual);//可升级成牙商所用的商品
            AdsListBLL adbll = new AdsListBLL();
            ViewData["adlist"] = adbll.GetList(0);
            ViewData["user"] = UserInfo;
            return View(list);
        }
        public ActionResult ProductDetail(Guid id)
        {
            ProductsBLL bll = new ProductsBLL();
            ProductList p = bll.GetProduct(id);
            if (p == null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            return View(p);
        }
        /// <summary>
        /// 开户页面 两种方式
        /// </summary>
        /// <param name="pid">pid不为空通过购买方式</param>
        /// <param name="prid">prid不为空通过未使用的开户券</param>
        /// <returns></returns>
        [Filter.CheckLogin]
        public ActionResult NewAccount(Guid? pid, Guid? prid)
        {
            if (pid != null)
            {
                ProductsBLL bll = new ProductsBLL();
                ProductList p = bll.GetProduct(pid.Value);
                if (p == null)
                {
                    return Redirect(Request.UrlReferrer.ToString());
                }
                ViewData["price"] = p.Price;
            }
            ViewData["pid"] = pid;
            ViewData["prid"] = prid;
            ViewData["cnum"] = "A" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10000, 99999);
            ViewData["tjr"] = UserInfo.Name;
            return View();
        }
        [Filter.CheckLogin]
        public ActionResult PersonCenter()
        {
            UsersBLL ubll = new UsersBLL();
            UserInfo = ubll.GetUserById(UserInfo.ID);
            ViewData["sljf"] = ubll.GetSiLuJiFen(UserInfo.ID);
            ViewData["level"] = (Common.enUserLV)UserInfo.LV;
            ViewData["sy"] = new PayListBLL().GetDailySalary(UserInfo.ID);//收益//分享积分
                                                                          // ViewData["sy"] = new PayListBLL().GetPayListDetailPrice(UserInfo.ID, DateTime.Now, Common.enPayListType.KaiHu);
            ViewData["td"] = ubll.GetNextSumPrice(UserInfo.ID);
            return View(UserInfo);
        }
        /// <summary>
        /// 购买产品
        /// </summary>
        /// <param name="id">商品id</param>
        /// <returns></returns>
        public JsonResult Buy(Guid id)
        {
            ProductsBLL bll = new ProductsBLL();
            try
            {
                int i = bll.Buy(UserInfo.ID, id);
                if (i > 0)
                {
                    return RJson(1, "购买成功");
                }
                else
                {
                    return RJson(-1, "购买失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, ex.Message);
            }
        }
        [Filter.CheckLogin]
        public ActionResult PaymentPattern(Guid pid, string code)
        {
            Users user = Session["user"] as Users;
            WX_Services.WxPayApi wxclient = new WX_Services.WxPayApi();
            if (!string.IsNullOrEmpty(code))
            {
                var wxuser = wxclient.GetWXUserinfo();
                if (wxuser != null && !string.IsNullOrEmpty(wxuser.unionid))
                {
                    UsersBLL ubll = new UsersBLL();
                    user.WX_OpenID = wxuser.openid;
                    user.WX_UnionID = wxuser.unionid;
                    user.WxName = wxuser.nickname;
                    ubll.UpdateUser(user);
                }
            }
            if (string.IsNullOrEmpty(user.WX_OpenID))
            {
                ViewData["wxloginurl"] = wxclient.GetWXLoginURL(Request.Url.ToString(), WX_Services.euScope.snsapi_userinfo, string.Empty);
            }
            ViewData["isWxLogin"] = string.IsNullOrEmpty(user.WX_OpenID);
            ProductsBLL bll = new ProductsBLL();
            var product = bll.GetProduct(pid);
            if (product == null || product.Status != (int)Common.enProductType.Virtual)
            {
                return Redirect("/Shop/PersonCenter");
            }
            return View(product);
        }

        public ActionResult OrderConfirm(Guid id)
        {
            ThirdServices th = new ThirdServices();
            var t = th.GetDetails(id);
            if (t == null || t.UID != UserInfo.ID || t.OrderType != (int)Common.enPayFrom.UpdateUserLV)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            ProductsBLL bll = new ProductsBLL();
            var p = bll.GetProduct(t.TOID.Value);
            if (p == null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            RecAddressBLL recbll = new RecAddressBLL();
            ViewData["defaultRecAdd"] = recbll.GetRecDefault(UserInfo.ID);
            ViewData["reclist"] = recbll.GetUserAddressList(UserInfo.ID);
            return View(p);

        }

        public ActionResult UserHelp(int id)
        {
            UserHelpBLL bll = new UserHelpBLL();
            ViewData["id"] = id;
            return View(bll.GetListByType(id));
        }
        public ActionResult UserHelpDetail(Guid id)
        {
            UserHelpBLL bll = new UserHelpBLL();
            return View(bll.Get(id));
        }
        /// <summary>
        /// 开户之前操作
        /// </summary>
        /// <returns></returns>
        public ActionResult NewAccountBefore()
        {
            return View();
        }
        public JsonResult CheckNewAccountBefore(Guid FID)
        {
            UsersBLL ubll = new UsersBLL();
            if (ubll.CanCreateUser(UserInfo.ID, FID))
            {
                TempData["FID"] = FID;
                return RJson(1, "success");
            }
            else
            {
                return RJson(-1, "该节点不可用");
            }
        }
    }
}