using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RelexBarBLL;
using RelexBarDLL;
using silushop.Utils;
using static RelexBarBLL.Common;

namespace silushop.Controllers
{

    public class ShopController : Controller
    {
        [NonAction]
        public JsonResult RJson(int code, string msg)
        {
            return Json(new { code = code, msg = msg });
        }
        [NonAction]
        public ActionResult AlertAndLinkTo(string msg, string url)
        {
            return Content("<script>alert(\"" + msg + "\");location.href='" + url + "';</script>");
        }
        // GET: Shop
        public ActionResult Index()
        {
            List<Models.CateGoryProductModel> list = new List<Models.CateGoryProductModel>();
            CategoryBLL bll = new CategoryBLL();
            List<Category> clist = bll.GetAllList(enStatus.Enabled);
            for (int i = 0; i < clist.Count; i++)
            {
                list.Add(new Models.CateGoryProductModel { category = clist[i], PList = bll.GetProductByCID(clist[i].ID, 5) });
            }
            ProductsBLL pbll = new ProductsBLL();
            ViewData["randProduct"] = pbll.GetRandProduct(6);
            AdsListBLL adbll = new AdsListBLL();
            ViewData["ads"] = adbll.GetList(1);
            ViewData["ads1"] = adbll.GetList(2, 4);
            int sum = 0;
            ViewData["toutiao"] = new UserMsgBLL().GetList(1, 2, out sum, Guid.Empty, 1, null, enMessageType.TouTiao);

            //牙商总数，会员总数
            UsersBLL ubll = new UsersBLL();
            ViewData["yaCount"] = ubll.GetTotalYSCount();
            ViewData["hyCount"] = ubll.GetTotalUserCount();

            return View(list);
        }
        //头条
        public ActionResult HeadDetail(Guid id)
        {
            UserMsg m = new UserMsgBLL().Get(id);
            if (m == null)
            {
                AlertAndLinkTo("文章不存在", Request.UrlReferrer.ToString());
            }
            return View(m);
        }

        public ActionResult MessageList()//用户消息列表
        {
            int sum = 0;
            var m = new UserMsgBLL().GetList(1, 100, out sum, Guid.Empty, 1, null, enMessageType.TouTiao);
            return View(m);
        }

        [Filter.CheckLogin]
        public ActionResult PersonCenter()
        {
            //丝路易物卷、流通易物卷、易物卷2
            //今日行票、行票回购、行票消费券

            UsersBLL ubll = new UsersBLL();
            Users user = Session["user"] as Users;
            user = ubll.GetUserById(user.ID);

            decimal totalJF = 0;
            decimal? totalTSF = new TransferOutBLL().GetAllTransforout(user.ID, 1);
            var jfAll = ubll.GetSiLuJiFen2(user.ID, out totalJF);
            ViewData["sljf"] = jfAll.ToString("0.##");//剩余丝路易物卷（未释放未使用）
            ViewData["liutong"] = totalTSF.HasValue ? (totalTSF.Value).ToString("0.##") : "0";//流通易物卷(提现总额)
            //ViewData["sljf2"] = (totalJF * 0.3M).ToString("0.##");//易物卷2(30%)
            //回馈卷
            var huikui = ubll.GetHuikui(user.ID);
            ViewData["sljf2"] = huikui.ToString("0.##");//易物卷2(30%)
            ViewData["level"] = (Common.enUserLV)user.LV;
            ViewData["sy"] = new PayListBLL().GetDailySalary(user.ID).ToString("0.##");//今日行票
            ViewData["xfj"] = (jfAll * 0.1M).ToString("0.##");//行票消费券

            var rec = ubll.GetLastRecord(user.ID);
            var productlist = new ProductsBLL().GetAccountTicket(user.ID).Where(m => m.IsBuyProduct == 0);
            if (productlist.Count() > 0)
            {
                ViewData["dh"] = productlist.Sum(m => m.Price);
            }
            else
            {
                ViewData["dh"] = 0;
            }

            ViewData["td"] = ubll.GetNextSumPrice(user.ID);
            bool btn_kd = true;
            if (user.UserType == (int)Common.enUserType.Shop)
            {
                ViewData["shopid"] = new ShopBLL().GetByUID(user.ID).ID;
                btn_kd = false;
            }
            PayRecord pr = ubll.GetNewPayRecordByUID(user.ID);
            if (pr == null || pr.MaxMoney < 30000)
            {//1521项目不能拿这个字段判断
                btn_kd = false;
            }
            ViewData["btn_kd"] = btn_kd;
            Session["user"] = user;
            return View(user);
        }
        public ActionResult Category()
        {
            CategoryBLL bll = new CategoryBLL();
            List<Category> list = bll.GetAllList(enStatus.Enabled);
            return View(list);
        }
        [Filter.CheckLogin]
        public ActionResult ShoppingCart()
        {
            CartBLL bll = new CartBLL();
            Users user = Session["user"] as Users;
            List<RelexBarBLL.Models.CartModel> list = bll.GetList(user.ID);
            return View(list);
        }
        public ActionResult Login()
        {
            return View();
        }
        public JsonResult DoLogin(string name, string pwd)
        {
            UsersBLL bll = new UsersBLL();
            Users user = bll.GetUser(name, pwd);
            if (user == null || user.UserType != (int)Common.enUserType.Shop)
            {
                return RJson(-1, "登陆失败");
            }
            Session["shop"] = user;
            Session["user"] = user;
            string token = MD5(user.ID + user.Psw);
            HttpCookie cookie = new HttpCookie("shopToken", user.ID + "|" + token);
            Response.Cookies.Add(cookie);
            return RJson(1, "登陆成功");
        }
        [Filter.CheckShop]
        public ActionResult Manager()
        {
            return View(Session["shop"]);
        }
        //商家后台首页
        [Filter.CheckShop]
        public ActionResult ManagerDefault()
        {
            OrdersBLL bll = new OrdersBLL();
            Users user = Session["shop"] as Users;
            string[] data = bll.ShopIndexData(new ShopBLL().GetByUID(user.ID).ID);
            return View(data);
        }
        //商品管理
        [Filter.CheckShop]
        public ActionResult ProductManager()
        {
            return PartialView();
        }
        //订单管理
        [Filter.CheckShop]
        public ActionResult OrderManager()
        {
            return PartialView();
        }
        //账单管理
        [Filter.CheckShop]
        public ActionResult PayListManager()
        {
            return PartialView();
        }
        [Filter.CheckShop]
        public ActionResult PayList(string name, enPayFrom? FromTo, DateTime? beginTime, DateTime? endTime, enPayInOutType? InOut, int index = 1, int pageSize = 10)
        {
            Users u = Session["shop"] as Users;
            PayListBLL bll = new PayListBLL();
            int sum = 0;
            List<RelexBarBLL.Models.AdminPayListModel> list = bll.GetPayList(null, FromTo, InOut, beginTime, endTime, index, pageSize, out sum, u.ID, name);
            @ViewData["sum"] = sum;
            return PartialView(list);
        }
        //商家信息编辑
        [Filter.CheckShop]
        public ActionResult EditShopInfo()
        {
            Users user = Session["shop"] as Users;
            Shop shop = new ShopBLL().GetByUID(user.ID);
            return PartialView(shop);
        }
        //商家扫码
        [Filter.CheckShop]
        public ActionResult Scan()
        {
            return View();
        }
        public ActionResult LoginOut()
        {
            Session["shop"] = null;
            if (Response.Cookies["shopToken"] != null)
                Response.Cookies["shopToken"].Expires = DateTime.Now.AddDays(-1);
            return Redirect("Login");
        }
        [Filter.CheckShop]
        public ActionResult OrderManagerList(string Number, int? status, enOrderType? type, DateTime? beginTime, DateTime? endtime, int index = 1, int pageSize = 10)
        {
            Users user = Session["shop"] as Users;
            Shop shop = new ShopBLL().GetByUID(user.ID);
            OrdersBLL bll = new OrdersBLL();
            int sum = 0;
            enOrderStatus? Status = (enOrderStatus?)status;
            enOrderType? Type = (enOrderType?)type;
            List<RelexBarBLL.Models.OrderListModel> list = bll.GetOrderList(Number, Status, Type, beginTime, endtime, pageSize, index, out sum, shop.ID);
            @ViewData["sum"] = sum;
            return PartialView(list);
        }
        //修改商家信息
        [Filter.CheckShop]
        public JsonResult UpdateShopInfo(string Img, string ShopName, string ChatQQ, string BackImg, string ServicePhone)
        {
            try
            {
                Users user = Session["shop"] as Users;
                ShopBLL bll = new ShopBLL();
                Shop shop = bll.GetByUID(user.ID);
                int i = bll.Update(shop.ID, Img, ShopName, ChatQQ, BackImg, ServicePhone);
                if (i > 0)
                {
                    return RJson(1, "修改成功");
                }
                else
                {
                    return RJson(-1, "修改失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, "修改失败：" + ex.Message);
            }



        }
        public ActionResult AllProductList(string id)
        {
            return View((object)id);
        }
        public JsonResult MobileProductList(int? index, string key, int? Category, Guid? ShopID, int? Status, Common.enOrderBy OrderType = Common.enOrderBy.OrderID, int pageSize = 10)
        {
            int sum;
            ProductsBLL bll = new ProductsBLL();

            List<RelexBarBLL.Models.ProductListModel> list = bll.GetAllProductList(Category, key, Status, enProductType.Real, pageSize, index == null ? 1 : index.Value, out sum, ShopID, OrderType);
            ViewData["sum"] = sum;
            List<RelexBarBLL.Models.MobileProductListModel> list2 = new List<RelexBarBLL.Models.MobileProductListModel>();
            foreach (var item in list.Where(m => m.CategoryID.Value > 0))
            {
                list2.Add(new RelexBarBLL.Models.MobileProductListModel()
                {
                    ID = item.ID,
                    Name = item.Name,
                    Title = item.Title,
                    Img = item.Img,
                    Descrition = item.Descrition,
                    Price = item.Price,
                    Payed = item.Payed,
                    RealPrice=item.RealPrice,
                });
            }
            return Json(list2);
        }
        /// <summary>
        /// 商品列表
        /// </summary>
        /// <returns></returns>
        [Filter.CheckShop]
        public ActionResult ProductList(int? index, string key, int? Category, int? Status, int pageSize = 10)
        {
            int sum;
            ProductsBLL bll = new ProductsBLL();
            Users user = Session["shop"] as Users;
            Shop shop = new ShopBLL().GetByUID(user.ID);
            List<RelexBarBLL.Models.ProductListModel> list = bll.GetAllProductList(Category, key, Status, enProductType.Real, pageSize, index == null ? 1 : index.Value, out sum, shop.ID);
            //List<RelexBarBLL.Models.ProductListModel> list = bll.GetAllProductList(Taste, Category, key, Status, pageSize, index == null ? 1 : index.Value, out sum);
            ViewData["sum"] = sum;
            return PartialView(list);
        }
        /// <summary>
        /// 编辑商品
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [Filter.CheckShop]
        public ActionResult EditProduct(Guid? ID)
        {

            ProductsBLL bll = new ProductsBLL();
            ProductList p = ID == null ? null : bll.GetProduct(ID.Value);
            SelectList sl = new SelectList(new CategoryBLL().GetAllList(), "ID", "Name");
            ViewData["clist"] = sl;
            RelexBarBLL.ProductsBLL probll = new RelexBarBLL.ProductsBLL();
            var ls = ID == null ? null : probll.GetProductSpec(ID.Value);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (ls != null)
                foreach (var q in ls)
                {
                    sb.AppendFormat("{0}||{1}||{2}||{3}||{4}||{5}||{6}||{7}$$", q.SPID, q.SPName, q.SPDesc, q.Number, q.RealPrice, q.Price, q.Stock, q.Remark);
                }
            if (sb.Length > 0)
            {
                sb = sb.Remove(sb.Length - 2, 2);
            }
            ViewData["hfSPValues"] = sb.ToString();
            return PartialView(p);
        }
        private void SaveSpecValues(Guid proID, string hfSPValues)
        {
            string spvalue = hfSPValues;

            RelexBarBLL.ProductsBLL probll = new RelexBarBLL.ProductsBLL();
            probll.DeleteSpec(proID);

            Guid? SPID;
            string SPName;
            string SPDesc;
            string Number;
            decimal RealPrice;
            decimal Price; decimal Stock;
            string Remark;
            if (!string.IsNullOrEmpty(spvalue))
            {
                var line = spvalue.Split(new string[] { "$$" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var q in line)
                {
                    var t = q.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                    if (t[0] != "0")
                        SPID = Guid.Parse(t[0]);
                    else
                        SPID = null;

                    SPName = t[1];
                    SPDesc = t[2];
                    Number = t[3];
                    RealPrice = decimal.Parse(t[4]);
                    Price = decimal.Parse(t[5]);
                    Stock = decimal.Parse(t[6]);
                    Remark = t[7];
                    probll.InsertSpecProduct(SPID, proID, SPName, SPDesc, Number, 0, RealPrice, RelexBarBLL.Common.enPayType.Coin, Price, Stock, Remark);
                }
            }
        }
        [ValidateInput(false)]
        [Filter.CheckShop]
        public JsonResult ProductSave(Guid? ID, string Name, int CategoryID, string Title, string Number, int PriceType, decimal RealPrice, decimal Price
            , decimal Stock, string Img, string ImgList, string Descrition, int? OrderID, int Type, string hfSPValues, decimal? CashDiscount)
        {

            #region 判断
            if (string.IsNullOrEmpty(Name))
            {
                return RJson(-1, "商品名字不能为空！");
            }
            if (string.IsNullOrEmpty(Title))
            {
                return RJson(-1, "商品标题不能为空！");
            }
            if (string.IsNullOrEmpty(Number))
            {
                return RJson(-1, "商品编号不能为空！");
            }
            if (string.IsNullOrEmpty(Descrition))
            {
                return RJson(-1, "商品详情不能为空！");
            }
            if (string.IsNullOrEmpty(Img))
            {
                return RJson(-1, "商品首图不能为空！");
            }
            #endregion

            RelexBarBLL.ProductsBLL probll = new RelexBarBLL.ProductsBLL();
            Users user = Session["shop"] as Users;
            Shop shop = new ShopBLL().GetByUID(user.ID);
            if (OrderID == null) OrderID = 0;
            if (ID == null)//ID不正确，新增
            {

                ID = probll.Insert(shop.ID, Name, Title, Number, CategoryID, Img, ImgList, Descrition, RealPrice
                    , (RelexBarBLL.Common.enPayType)PriceType, Price, Stock, OrderID.Value, (RelexBarBLL.Common.enProductType)Type, DateTime.Now, DateTime.Now.AddYears(10), CashDiscount.HasValue ? CashDiscount.Value : 1, Common.enStatus.Unabled);
                if (ID != Guid.Empty)
                {
                    SaveSpecValues(ID.Value, hfSPValues);

                    return RJson(1, "商品添加成功！");
                }
                else
                {
                    return RJson(-1, "商品添加失败！");
                }
            }
            else//编辑
            {
                var model = probll.GetProduct(ID.Value);
                if (model.ShopID != shop.ID)
                {
                    return RJson(-1, "没有权限编辑该商品");
                }

                model.CategoryID = CategoryID;
                model.Descrition = Descrition;
                model.Img = Img;
                model.ImgList = ImgList;
                model.Name = Name;
                model.Number = Number;
                model.OrderID = OrderID.Value;
                model.Price = Price;
                model.PriceType = PriceType;
                model.RealPrice = RealPrice;
                //model.Status = Status;
                model.Stock = Stock;
                model.Title = Title;
                model.Type = Type;
                model.UpdateTime = DateTime.Now;
                model.CashDiscount = CashDiscount.HasValue ? CashDiscount.Value : 1;
                if (probll.Update(model) > 0)
                {
                    SaveSpecValues(model.ID, hfSPValues);

                    return RJson(1, "更新成功！");
                }
                else
                {
                    return RJson(-2, "更新失败！");
                }

            }
        }
        //[Filter.CheckLogin]
        public ActionResult ProductDetail(Guid id)
        {
            ProductsBLL bll = new ProductsBLL();
            RelexBarDLL.ProductList p = bll.GetProduct(id);
            if (p == null)
            {
                return Redirect("ProductList");
            }
            ViewData["Spec"] = bll.GetProductSpec(id);
            //ViewData["carcount"] = new CartBLL().GetCarCount(UserInfo.ID, 0);
            //获取商家信息
            Users user = Session["user"] as Users;
            if (user != null)
            {
                ViewData["isLogin"] = true;
                ViewData["fav"] = bll.IsFavorites(user.ID, p.ID);
            }
            else
            {
                ViewData["isLogin"] = false;
            }
            ViewData["shop"] = new ShopBLL().Get(p.ShopID.Value);
            return View(p);

        }
        public ActionResult ShopHome(Guid id)
        {
            ShopBLL bll = new ShopBLL();
            Shop shop = bll.Get(id);
            if (shop == null)
            {
                return AlertAndLinkTo("商家不存在", Request.UrlReferrer.ToString());
            }
            ViewData["shopcount"] = bll.GetShopProductCount(shop.ID);
            return View(shop);
        }
        public JsonResult GetPrice(Guid id, string SPDesc)
        {
            ProductsBLL bll = new ProductsBLL();
            decimal? Price, RealPrice;
            bll.GetPrice(id, SPDesc, out Price, out RealPrice);
            return Json(new { Price = Price, RealPrice = RealPrice });
        }
        /// <summary>
        /// 添加商品到收藏夹
        /// </summary>
        /// <param name="PID"></param>
        /// <returns></returns>
        [Filter.CheckLogin]
        public JsonResult AddFavorites(Guid PID)
        {
            try
            {
                Users user = Session["user"] as Users;
                int i = new ProductsBLL().AddFavorites(user.ID, PID);
                if (i > 0)
                {
                    return RJson(1, "添加成功");
                }
                else
                {
                    return RJson(-1, "添加失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, "添加失败:" + ex.Message);
            }
        }
        [Filter.CheckLogin]
        public JsonResult DelFavorites(Guid PID)
        {

            Users user = Session["user"] as Users;
            int i = new ProductsBLL().DelFavorites(user.ID, PID);
            if (i > 0)
            {
                return RJson(1, "添加成功");
            }
            else
            {
                return RJson(-1, "添加失败");
            }


        }
        [Filter.CheckLogin]//收藏
        public ActionResult Favorites()
        {
            return View();
        }
        [Filter.CheckLogin]
        public JsonResult GetFavList(int? index, int pageSize = 10)
        {
            Users user = Session["user"] as Users;

            List<RelexBarBLL.Models.FavoritesModel> list = new ProductsBLL().Favorites(user.ID, index == null ? 1 : index.Value, pageSize);
            return Json(list);
        }
        /// <summary>
        /// 会员特权
        /// </summary>
        /// <returns></returns>
        public ActionResult UserPower()
        {
            Users a = Session["user"] as Users;
            return View(a);
        }

        //添加到购物车
        [Filter.CheckLogin]
        public JsonResult AddCar(Guid id, string SPDesc, int count)
        {
            try
            {
                if (count < 1)
                {
                    return RJson(-1, "商品数量不正确");
                }
                ProductsBLL pbll = new ProductsBLL();
                int i = pbll.CheckStock(id, SPDesc, count);
                if (i < 1)
                {
                    return RJson(-1, "库存不足");
                }

                Users user = Session["user"] as Users;
                int j = pbll.Insert(user.ID, id, SPDesc, count);
                if (i > 0)
                {
                    return RJson(1, "添加成功");
                }
                else
                {
                    return RJson(-1, "添加失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, "添加失败：" + ex.Message);
            }


        }
        //删除购物车商品
        [Filter.CheckLogin]
        public JsonResult DeleteCar(Guid id)
        {
            CartBLL bll = new CartBLL();
            int i = bll.Delete(id);
            if (i > 0)
            {
                return RJson(1, "删除成功");
            }
            return RJson(-1, "删除失败");
        }
        //更新购物车商品
        public JsonResult UpdateCar(string data)
        {
            List<RelexBarBLL.Models.UpdateCartModel> list = Utils.NewtonJSONHelper.DeserializeObject<List<RelexBarBLL.Models.UpdateCartModel>>(data);
            CartBLL bll = new CartBLL();
            int i = bll.UpdateCount(list);
            if (i > 0)
            {
                return RJson(1, "购物车更新成功");
            }
            return RJson(-1, "购物车更新失败");
        }
        //把购物车的商品添加到订单
        [Filter.CheckLogin]
        public JsonResult OrderConfirmByCar(string cid)
        {
            try
            {
                Users user = Session["user"] as Users;
                string[] temp = cid.Split(',');
                Guid[] CIDS = Array.ConvertAll(temp, s => Guid.Parse(s));
                OrdersBLL bll = new OrdersBLL();
                Guid OID;
                int i = bll.AddOrderByCar(user.ID, CIDS, out OID);
                if (i > 0)
                {
                    return RJson(1, OID.ToString());
                }
                else
                {
                    return RJson(i, "提交失败");
                }
            }
            catch (Exception ex)
            {

                return RJson(-1, "提交失败：" + ex.Message);
            }

        }
        //下单
        [Filter.CheckLogin]
        public JsonResult CreateOrder(Guid id, string SPDesc, int count)
        {
            if (count < 1)
            {
                return RJson(-1, "商品数量不正确");
            }
            Users user = Session["user"] as Users;
            OrdersBLL bll = new OrdersBLL();
            Guid OID;
            int i = bll.Insert(user.ID, id, SPDesc, count, out OID);
            if (i > 0)
            {
                return RJson(1, OID.ToString());
            }
            else
            {
                return RJson(i, ((Common.ErrorCode)i).ToString());
            }
        }
        //获取订单状态
        [Filter.CheckLogin]
        public JsonResult GetOrderStatus(Guid ID)
        {
            OrdersBLL bll = new OrdersBLL();
            var model = bll.GetDetail(ID);
            if (model != null && model.Status.HasValue)
            {
                return RJson(model.Status.Value, "");
            }
            return RJson(0, "");
        }
        //提交订单
        [Filter.CheckLogin]
        public ActionResult OrderConfirm(Guid id)
        {
            Users user = Session["user"] as Users;
            OrdersBLL bll = new OrdersBLL();
            OrderList order = bll.GetOrderListByID(id, user.ID);
            if (order == null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            ViewData["order"] = order;
            List<OrderProductList> list = bll.GetOrderProList(id);
            List<RecAddress> addlist = new RecAddressBLL().GetUserAddressList(user.ID);
            ViewData["oid"] = id;
            ViewData["addlist"] = addlist;
            // ViewData["recaddress"] = bll.GetRecAddress(id);
            if (order.RecID == null)
            {
                ViewData["defaultRecAdd"] = new RecAddressBLL().GetRecDefault(user.ID);
            }
            else
            {
                ViewData["defaultRecAdd"] = new RecAddressBLL().GetAddressDetail(order.RecID.Value);
            }

            return View(list);
        }
        [Filter.CheckLogin]
        public JsonResult GetAddressList()
        {
            Users user = Session["user"] as Users;
            OrdersBLL bll = new OrdersBLL();
            List<RecAddress> addlist = new RecAddressBLL().GetUserAddressList(user.ID);
            return RJson(1, addlist.SerializeObject());
        }

        [Filter.CheckLogin]
        //订单列表
        public ActionResult OrderList(int? id = null)
        {
            return View(id);
        }
        //取消订单
        [Filter.CheckLogin]
        public JsonResult CancelOrder(Guid id)
        {
            Users user = Session["user"] as Users;
            OrdersBLL bll = new OrdersBLL();
            int i = bll.CancelOrder(user.ID, id);
            if (i > 0)
            {
                return RJson(1, "取消成功");
            }
            else
            {
                return RJson(-1, ((Common.ErrorCode)i).ToString());
            }
        }
        [Filter.CheckLogin]
        public JsonResult OrderListItem(int? index, int? status = null)
        {
            Users user = Session["user"] as Users;
            int sum;
            OrdersBLL bll = new OrdersBLL();
            List<OrderList> list = bll.GetOrderListByUID(user.ID, index == null ? 1 : index.Value, 2, out sum, status);
            Guid[] ids = new Guid[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                ids[i] = list[i].ID;
            }
            List<OrderProductList> opllist = bll.GetorderProductList(ids);

            //List<RelexBarDLL.OrderProductList> list = bll.GetOrderProListByUID(user.ID, index == null ? 1 : index.Value, 10, out sum, status);
            // ViewData["sum"] = sum;
            return Json(new { Order = list, OrderList = opllist });
        }
        //商家入驻申请
        [Filter.CheckLogin]
        public ActionResult ShopReq()
        {
            Users user = Session["user"] as Users;
            if (user.UserType == (int)Common.enUserType.Shop)
            {
                return AlertAndLinkTo("您已经是商家了", Request.UrlReferrer.ToString());
            }
            return View();
        }
        [Filter.CheckLogin]
        public JsonResult DoShopReq(string ShopName, string HeadImg, string IDcard_img, string IDcard_img2, string BackImg, string BLImg, string AreaID, string Address)
        {
            if (string.IsNullOrEmpty(ShopName) || string.IsNullOrEmpty(HeadImg) || string.IsNullOrEmpty(IDcard_img) || string.IsNullOrEmpty(IDcard_img2)
                || string.IsNullOrEmpty(BackImg) || string.IsNullOrEmpty(BLImg) || string.IsNullOrEmpty(AreaID) || string.IsNullOrEmpty(Address))
            {
                return RJson(-1, "参数不正确");
            }
            Users user = Session["user"] as Users;
            AdminMsgBLL bll = new AdminMsgBLL();
            if (bll.HasShopReq(user.ID))
            {
                return RJson(-1, "提交失败，还有未处理完的请求");
            }
            Web_Area a = new WebAreaBll().Detail(AreaID);
            if (a == null)
            {
                return RJson(-1, "地址选择不正确");
            }
            AreaID = a.Family;
            string content = "{" + string.Format("\"HeadImg\":\"{0}\",\"ShopName\":\"{1}\",\"IDcard_img\":\"{2}\",\"IDcard_img2\":\"{3}\",\"BackImg\":\"{4}\",\"BLImg\":\"{5}\",\"AreaID\":\"{6}\",\"Address\":\"{7}\""
                , HeadImg, ShopName, IDcard_img, IDcard_img2, BackImg, BLImg, AreaID, Address) + "}";

            int i = bll.Add(user.ID, content, enAdminMsgType.ShopReq, "");
            if (i > 0)
            {
                return RJson(1, "提交成功");
            }
            else
            {
                return RJson(-1, "提交失败");
            }
        }
        //删除订单里的商品
        [Filter.CheckLogin]
        public JsonResult DeleteOrderItem(Guid id)
        {
            Users user = Session["user"] as Users;
            OrdersBLL bll = new OrdersBLL();
            try
            {
                int i = bll.DeleteProduct(id);
                if (i > 0)
                {
                    return RJson(1, "删除成功");
                }
                else
                {
                    return RJson(-1, "删除失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, "删除失败：" + ex.Message);
            }

        }
        //更新订单
        [Filter.CheckLogin]
        public JsonResult UpdateOrder(Guid id, string data, Guid address, string notes)
        {
            List<dynamic> list = Utils.NewtonJSONHelper.DeserializeObject<List<dynamic>>(data);
            OrdersBLL bll = new OrdersBLL();
            try
            {
                int i = bll.UpdateOrder(id, list, address, notes);
                if (i >= 0)
                {
                    return RJson(1, id.ToString());
                }
                else
                {
                    return RJson(-1, "订单更新失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, "订单更新失败：" + ex.Message);
            }

        }
        [Filter.CheckLogin]
        public ActionResult PaymentPattern(Guid id, string code)
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
            OrdersBLL bll = new OrdersBLL();
            OrderList o = bll.GetDetail(id);
            if (o == null || o.Status != (int)Common.enOrderStatus.Order)
            {
                return Redirect("/Shop/PersonCenter");
            }
            return View(o);
        }

        /// <summary>
        /// 评论
        /// </summary>
        /// <returns></returns>
        [Filter.CheckLogin]
        public JsonResult OrderComment(Guid id, int score, string comment)
        {
            OrdersBLL bll = new OrdersBLL();
            try
            {
                Users user = Session["user"] as Users;
                int i = bll.AddOrderComment(user.ID, id, score, comment);
                if (i > 0)
                {
                    return RJson(1, "评价成功");
                }
                else
                {
                    return RJson(-1, "评价失败");
                }
            }
            catch (Exception e)
            {
                return RJson(-1, e.Message);
            }
        }

        public ActionResult PayQRCode()
        {
            return View();
        }
        /// <summary>
        /// 生成二维码内容为用户id||验证码id
        /// </summary>
        [Filter.CheckLogin]
        public void GetQRCode()
        {
            Users user = Session["user"] as Users;
            VerifyCodesBLL verbll = new VerifyCodesBLL();
            var result = verbll.InsertPayCode(user.ID, Common.enPayType.Point);//插入验证码到数据库返回验证码id
            string content = Common.Encrypt(user.ID + "|" + result.ToString());//用户id，验证码id
            Response.ClearContent();
            Response.ContentType = "image/jpeg";
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                string log = null;

                if (string.IsNullOrEmpty(user.HeadImg1))
                {
                    log = "/img/defaulthead.jpg";
                }
                else
                {
                    log = user.HeadImg1;
                }
                string root = Server.MapPath("/");
                try
                {
                    var q = Common.GetQrCodeImgAndLogo(content, root + log);
                    q.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    Response.BinaryWrite(ms.ToArray());
                    q.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }


            }
            Response.End();
        }
        /// <summary>
        /// 验证码5分钟过期
        /// </summary>
        /// <param name="data"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        [Filter.CheckShop]
        public JsonResult DoScan(string data, string money)
        {
            try
            {
                Users shop = Session["shop"] as Users;
                data = Common.Decrypt(data.Replace(" ", "+"));
                string[] datas = data.Split('|');
                if (!(datas.Length == 2))
                {
                    return RJson(-1, "用户或商家id不正确");
                }
                Guid uid = Guid.Empty, vid = Guid.Empty;
                if (!Guid.TryParse(datas[0], out uid) || !Guid.TryParse(datas[1], out vid))
                {
                    return RJson(-1, "数据不正确");
                }
                string[] temp = money.Split('.');
                if (temp.Length > 2)
                {
                    return RJson(-1, "数据不正确");
                }
                if (temp.Length > 1)
                {
                    if (temp[1].Length > 2)
                    {
                        return RJson(-1, "只能包含两位小数");
                    }
                }
                decimal Money = 0;
                if (!decimal.TryParse(money, out Money))
                {
                    return RJson(-1, "数字转换失败");
                }
                UsersBLL bll = new UsersBLL();
                int i = bll.PayToShop(uid, shop.ID, Money, vid);
                if (i > 0)
                {
                    return RJson(1, "支付成功");
                }
                else
                {
                    return RJson(-1, "支付失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, ex.Message);
            }


        }

        [HttpPost]
        [Filter.CheckShop]
        public JsonResult Bill(DateTime? date, enPayFrom? from, enPayInOutType? inout, int? index)
        {
            Users shop = Session["shop"] as Users;
            PayListBLL bll = new PayListBLL();
            int sum = 0;
            List<PayList> list = bll.GetPayList(shop.ID, date, from, inout, null, 20, index == null ? 1 : index.Value, out sum);
            return Json(new { sum = sum, data = list });
        }
    }

}