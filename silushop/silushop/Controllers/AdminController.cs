using RelexBarBLL;
using RelexBarDLL;
using silushop.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static RelexBarBLL.Common;

namespace silushop.Controllers
{
    public static class FinalData
    {
        public static string Date { get; set; }
        public static string NewUser { get; set; }
        public static string loginCount { get; set; }
        public static DateTime NewUserCreateTime { get; set; }

    }
    [Filter.CheckAdmin]
    public class AdminController : Controller
    {
        [NonAction]
        public JsonResult RJson(string icode, string imsg)
        {
            return Json(new { code = icode, msg = imsg });
        }
        [NonAction]
        public JsonResult RJson(int icode, string imsg)
        {
            return Json(new { code = icode, msg = imsg });
        }
        [NonAction]
        public string MD5(string source)
        {
            return CommonClass.EncryptDecrypt.GetMd5Hash(source + SysConfigBLL.MD5Key);
        }
        [Filter.NoFilter]
        public ActionResult Login()
        {

            return View();
        }
        public ActionResult Index()
        {
            AdminUser user = Session["admin"] as AdminUser;
            AdminUserBLL bll = new AdminUserBLL();
            ViewData["menu"] = bll.GetMenu(user.ID);
            return View(user);
        }
        public ActionResult Default()
        {
            if (string.IsNullOrEmpty(FinalData.Date) || string.IsNullOrEmpty(FinalData.NewUser) || DateTime.Now.Day != FinalData.NewUserCreateTime.Day)
            {
                string[] lable = new string[7];
                string[] newusercount = new string[7];
                string[] logincount = new string[7];
                ReportBLL report = new ReportBLL();
                for (int i = 1; i <= 7; i++)
                {
                    lable[i - 1] = "'" + DateTime.Now.AddDays(0 - i).ToString("MM-dd") + "'";
                    newusercount[i - 1] = report.GetNewUser(DateTime.Now.AddDays(0 - i)).ToString();
                    logincount[i - 1] = report.GetLoginCount(DateTime.Now.AddDays(0 - i)).ToString();
                }
                FinalData.Date = string.Join(",", lable);
                FinalData.NewUser = string.Join(",", newusercount);
                FinalData.loginCount = string.Join(",", logincount);
                FinalData.NewUserCreateTime = DateTime.Now;
            }

            ViewData["lable"] = FinalData.Date;
            ViewData["newuser"] = FinalData.NewUser;
            ViewData["loginCount"] = FinalData.loginCount;
            return PartialView();
        }
        //用户管理
        public ActionResult UserManager()
        {
            return PartialView();
        }
        //商品分类管理
        public ActionResult CategoryManager()
        {
            return PartialView();
        }
        //商品管理
        public ActionResult ProductManager()
        {
            return PartialView();
        }
        //账单管理
        public ActionResult PayListManager()
        {
            return PartialView();
        }
        //收入明细
        public ActionResult PayListDetailManager()
        {
            return PartialView();
        }
        //生日牌管理
        public ActionResult BirthdayCardManager()
        {
            return PartialView();
        }
        //订单管理
        public ActionResult OrderManager()
        {
            return PartialView();
        }
        //提现管理
        public ActionResult TransforoutManager()
        {
            return PartialView();
        }
        //管理员管理
        public ActionResult AdminManager()
        {
            AdminUserBLL bll = new AdminUserBLL();
            return PartialView(bll.GetAdminRoleList());
        }
        //广告管理
        public ActionResult AdsManager()
        {
            return PartialView();
        }
        //文章管理/头条管理
        public ActionResult SysMsgManager()
        {
            return PartialView();
        }
        //系统配置
        public ActionResult SysConfig()
        {
            return PartialView();
        }
        //节假日管理
        public ActionResult HolidaysManager()
        {
            return PartialView();
        }
        //兑换券
        public ActionResult TicketManager()
        {
            return PartialView();
        }
        //关于我们/帮助中心
        public ActionResult UserHelpManager()
        {
            return PartialView();
        }
        //商家入驻申请
        public ActionResult ShopReqManager()
        {
            return PartialView();
        }
        //商家管理
        public ActionResult ShopManager()
        {
            return PartialView();
        }
        //商家信息编辑
        public ActionResult EditShopInfo()
        {
            Shop shop = new ShopBLL().GetByUID(Guid.Empty);
            return PartialView(shop);
        }
        public ActionResult UserList(int? index, string key, int pageSize = 10)
        {
            RelexBarBLL.UsersBLL bll = new RelexBarBLL.UsersBLL();
            int sum;
            List<RelexBarBLL.Models.AdminUsers> list = bll.GetUsersSearch(key, pageSize, index == null ? 1 : index.Value, out sum);
            ViewData["sum"] = sum;
            return PartialView(list);
        }
        /// <summary>
        /// 获取银行列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetBankList(Guid UID)
        {
            BankListBLL bll = new BankListBLL();
            List<BankList> list = bll.GetUserBankList(UID);
            return RJson("1", list.SerializeObject());
        }
        /// <summary>
        /// 更改用户状态
        /// </summary>
        /// <returns></returns>
        public JsonResult CUStatus(Guid UID, int status)
        {
            UsersBLL bll = new UsersBLL();
            enStatus s = enStatus.Enabled;
            if (status == 0)
            {
                s = enStatus.Unabled;
            }
            int i = bll.ChangeUserStatus(UID, s);
            if (i > 0)
                return RJson("1", "修改成功");
            return RJson("-1", "修改失败");
        }
        /// <summary>
        /// 查询交易记录
        /// </summary>
        /// <param name="name"></param>
        /// <param name="FromTo"></param>
        /// <param name="InOut"></param>
        /// <param name="index"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult PayList(string name, enPayFrom? FromTo, DateTime? beginTime, DateTime? endTime, enPayInOutType? InOut, int index = 1, int pageSize = 10)
        {
            PayListBLL bll = new PayListBLL();
            int sum = 0;
            List<RelexBarBLL.Models.AdminPayListModel> list = bll.GetPayList(null, FromTo, InOut, beginTime, endTime, index, pageSize, out sum, null, name);
            @ViewData["sum"] = sum;
            return PartialView(list);
        }
        //
        public ActionResult GetPayListDetail(string name, enPayListType? type, DateTime? beginTime, DateTime? endTime, enPayInOutType? InOut, int index = 1, int pageSize = 10)
        {
            PayListDetailBLL bll = new PayListDetailBLL();
            int sum = 0;
            List<RelexBarBLL.Models.PayListDetailModel> list = bll.GetPayListDetail(name, type, beginTime, endTime, index, pageSize, out sum);
            @ViewData["sum"] = sum;
            return PartialView(list);
        }
        /// <summary>
        /// 修改用户
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult EditUser(Guid? ID)
        {
            Users u;
            if (ID != null)
            {
                u = new UsersBLL().GetUserById(ID.Value);
                decimal totalJF;
                var jfAll = new UsersBLL().GetSiLuJiFen2(u.ID, out totalJF);
                ViewData["sljf"] = jfAll.ToString("0.##");//剩余丝路易物卷（未释放未使用）
            }
            else
            {
                u = new Users();
                ViewData["sljf"] = "";
            }
            return View(u);
        }
        public JsonResult DoEditUser(Guid ID, string Name, string TrueName, string HeadImg1, int LV, int Sex,
            decimal? AddScore, decimal? AddBalance, int addScoreBefore, decimal? YWQ, int addYWQ, 
            decimal? addShoppingVoucher, int addShoppingVoucherBe)
        {
            if (addScoreBefore == 2 && AddBalance != null)
            {
                AddBalance = 0 - AddBalance;
            }
            if (addYWQ == 2 && YWQ.HasValue)
            {
                YWQ = 0 - YWQ;
            }
            if (addShoppingVoucherBe == 2 && addShoppingVoucher.HasValue)
            {
                addShoppingVoucher = 0 - addShoppingVoucher;
            }
            UsersBLL bll = new UsersBLL();
            try
            {
                int i = bll.UpdateUser(ID, Name, TrueName, HeadImg1, LV, Sex, AddScore, AddBalance, YWQ,addShoppingVoucher);
                if (i > 0)
                {
                    return RJson("1", "修改成功");
                }
                return RJson("-2", "修改失败");
            }
            catch (Exception ex)
            {
                return RJson("-1", "修改失败" + ex.Message);
            }
        }
        /// <summary>
        /// 提现列表
        /// </summary>
        /// <param name="index"></param>
        /// <param name="key"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult TransforoutList(int? index, string key, int pageSize = 10)
        {
            int sum;
            TransferOutBLL bll = new TransferOutBLL();
            List<RelexBarBLL.Models.TransferOutModel> list = bll.GetList(key, null, null, null, pageSize, index == null ? 1 : index.Value, out sum, null);
            ViewData["sum"] = sum;
            return PartialView(list);
        }
        /// <summary>
        /// 更改提现状态
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="status"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public JsonResult TransforoutUpdate(Guid ID, int status, string remark)
        {
            TransferOutBLL bll = new TransferOutBLL();
            enApplyStatus s = status == 1 ? enApplyStatus.Success : enApplyStatus.Faild;
            int i = bll.UpdateStatus(ID, s, remark, -1);
            if (i > 0)
            {
                return RJson("1", "已处理");
            }
            else
            {
                return RJson(i.ToString(), "处理失败");
            }
        }
        /// <summary>
        /// 获得管理员列表
        /// </summary>
        /// <param name="index"></param>
        /// <param name="key"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult AdminList(int? index, string key, int pageSize = 10)
        {
            int sum;
            AdminUserBLL bll = new AdminUserBLL();
            List<AdminUser> list = bll.GetAdminList(index == null ? 1 : index.Value, pageSize, key, out sum);
            ViewData["sum"] = sum;
            return PartialView(list);
        }
        /// <summary>
        /// 修改管理员状态
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public JsonResult UpdateAdminStatus(Guid ID, int status)
        {
            AdminUserBLL bll = new AdminUserBLL();
            AdminUser a = bll.GetAdminByID(ID);
            if (a == null)
            {
                return RJson("-1", "管理员不存在");
            }
            a.Status = status == 1 ? 1 : 0;
            int i = bll.UpdateAdminUser(a);
            if (i > 0)
            {
                return RJson("1", "修改成功");
            }
            else
            {
                return RJson("-2", "修改失败");
            }
        }
        /// <summary>
        /// 修改管理员密码
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public JsonResult UpdateAdminPwd(Guid ID, string pwd)
        {
            AdminUserBLL bll = new AdminUserBLL();
            AdminUser a = bll.GetAdminByID(ID);
            if (a == null)
            {
                return RJson("-1", "管理员不存在");
            }
            if (string.IsNullOrEmpty(pwd))
            {
                return RJson("-3", "密码不能为空");
            }
            a.Psw = CommonClass.EncryptDecrypt.GetMd5Hash(pwd + SysConfigBLL.MD5Key);
            int i = bll.UpdateAdminUser(a);
            if (i > 0)
            {
                return RJson("1", "修改成功");
            }
            else
            {
                return RJson("-2", "修改失败");
            }
        }
        /// <summary>
        /// 添加管理员
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Pwd"></param>
        /// <returns></returns>
        public JsonResult AddAdmin(string Name, string Pwd, Guid RID)
        {

            AdminUserBLL bll = new AdminUserBLL();
            if (bll.Exist(Name))
            {
                return RJson("-1", "用户名已被使用");
            }
            int i = bll.InsertAdminUser(Name, Pwd, RID);
            if (i > 0)
            {
                return RJson("1", "添加成功");
            }
            else
            {
                return RJson("1", "添加失败");
            }
        }
        /// <summary>
        /// 获取配置信息列表
        /// </summary>
        /// <param name="index"></param>
        /// <param name="key"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult SysConfigList(int? index, string key, int pageSize = 10)
        {
            int sum;
            SysConfigBLL bll = new SysConfigBLL();
            List<SysConfig> list = bll.GetAllConfig(key, pageSize, index == null ? 1 : index.Value, out sum);
            ViewData["sum"] = sum;
            return PartialView(list);
        }
        /// <summary>
        /// 修改配置状态
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public JsonResult UpdateSysConfigStatus(int ID, int status)
        {
            SysConfigBLL bll = new SysConfigBLL();
            enStatus s = status == 1 ? enStatus.Enabled : enStatus.Unabled;
            int i = bll.UpdateStatus(ID, s);
            if (i > 0)
            {
                return RJson("1", "修改成功");
            }
            else
            {
                return RJson("-1", "修改失败");
            }
        }

        public JsonResult AddSysConfig(string name, string value, string des)
        {
            SysConfigBLL bll = new SysConfigBLL();
            int i = bll.Insert(name, value, des, 1);
            if (i > 0)
            {
                return RJson("1", "新增成功");
            }
            return RJson("-1", "新增失败");
        }
        public JsonResult EditSysConfig(int id, string value, string des)
        {
            SysConfigBLL bll = new SysConfigBLL();
            int i = bll.Update(id, value, des, null);
            if (i > 0)
            {
                return RJson("1", "修改成功");
            }
            return RJson("-1", "修改失败");
        }
        [Filter.NoFilter]
        public JsonResult DoLogin(string name, string pwd)
        {
            AdminUserBLL bll = new AdminUserBLL();
            AdminUser admin = bll.Login(name, pwd);
            if (admin == null)
            {
                return RJson("-1", "用户名或密码有误");
            }
            if (admin.Status == 0)
            {
                return RJson("-2", "该账户已被禁用");
            }
            Session["admin"] = admin;
            string token = MD5(admin.ID + admin.Psw);
            HttpCookie cookie = new HttpCookie("adminToken", admin.ID + "|" + token);
            Response.Cookies.Add(cookie);
            return RJson("1", "登陆成功");
        }
        public ActionResult LoginOut()
        {
            Session["admin"] = null;
            if (Response.Cookies["adminToken"] != null)
                Response.Cookies["adminToken"].Expires = DateTime.Now.AddDays(-1);
            return Redirect("Login");
        }
        /// <summary>
        /// 商品列表
        /// </summary>
        /// <returns></returns>
        public ActionResult ProductList(int? index, string key, int? Category, int? Taste, int? Status, int pageSize = 10)
        {
            int sum;
            ProductsBLL bll = new ProductsBLL();
            List<RelexBarBLL.Models.ProductListModel> list = bll.GetAllProductList(Category, key, Status, null, pageSize, index == null ? 1 : index.Value, out sum);
            ViewData["sum"] = sum;
            return PartialView(list);
        }
        /// <summary>
        /// 编辑商品
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult EditProduct(Guid? ID)
        {

            ProductsBLL bll = new ProductsBLL();
            ProductList p = ID == null ? null : bll.GetProduct(ID.Value);
            // SelectList sl = new SelectList(new CategoryBLL().GetAllList(), "ID", "Name");
            if (p == null || p.CategoryID != -1)
            {
                return RedirectToAction("EditProductAll", new { id = ID });
            }
            return PartialView(p);
        }
        public ActionResult EditProductAll(Guid? ID)
        {
            ProductsBLL bll = new ProductsBLL();
            ProductList p = ID == null ? null : bll.GetProduct(ID.Value);
            var catelist = new CategoryBLL().GetAllList();
            List<Category> tempcates = new List<Category>();
            foreach (var a in catelist.Where(m => m.HeadID == 0))
            {
                Category c = new RelexBarDLL.Category();
                c.ID = a.ID;
                c.Name = a.Name;
                tempcates.Add(c);
                foreach (var b in catelist.Where(m => m.HeadID == a.ID))
                {
                    Category bc = new Category();
                    bc.ID = b.ID;
                    bc.Name = "|--" + b.Name;
                    tempcates.Add(bc);
                }
            }
            SelectList sl = new SelectList(tempcates, "ID", "Name");
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
        //private void SaveSpecValues(Guid proID, string hfSPValues)
        //{
        //    string spvalue = hfSPValues;

        //    RelexBarBLL.ProductsBLL probll = new RelexBarBLL.ProductsBLL();
        //    probll.DeleteSpec(proID);

        //    Guid? SPID;
        //    string SPName;
        //    string SPDesc;
        //    string Number;
        //    decimal RealPrice;
        //    decimal Price; decimal Stock;
        //    string Remark;
        //    if (!string.IsNullOrEmpty(spvalue))
        //    {
        //        var line = spvalue.Split(new string[] { "$$" }, StringSplitOptions.RemoveEmptyEntries);
        //        foreach (var q in line)
        //        {
        //            var t = q.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
        //            if (t[0] != "0")
        //                SPID = Guid.Parse(t[0]);
        //            else
        //                SPID = null;

        //            SPName = t[1];
        //            SPDesc = t[2];
        //            Number = t[3];
        //            RealPrice = decimal.Parse(t[4]);
        //            Price = decimal.Parse(t[5]);
        //            Stock = decimal.Parse(t[6]);
        //            Remark = t[7];
        //            probll.InsertSpecProduct(SPID, proID, SPName, SPDesc, Number, 0, RealPrice, RelexBarBLL.Common.enPayType.Coin, Price, Stock, Remark);
        //        }
        //    }
        //}
        [ValidateInput(false)]
        public JsonResult ProductSave(Guid? ID, string Name, string Title, string Img, string ImgList, string Descrition, int? OrderID, int Type, decimal DailySalary,
            decimal Price, int Status, int CategoryID = -1)
        {

            #region 判断
            if (string.IsNullOrEmpty(Descrition))
            {
                return RJson("-1", "商品详情不能为空！");
            }
            if (string.IsNullOrEmpty(Img))
            {
                return RJson("-1", "商品首图不能为空！");
            }
            #endregion

            RelexBarBLL.ProductsBLL probll = new RelexBarBLL.ProductsBLL();
            if (OrderID == null) OrderID = 0;
            if (ID == null)//ID不正确，新增
            {
                //int i = probll.Insert(Guid.Empty, Name, Title, Number, CategoryID, Img, ImgList, Descrition, RealPrice
                //    , (RelexBarBLL.Common.enPayType)PriceType,
                //     Price, Stock, OrderID.Value, (RelexBarBLL.Common.enProductType)Type, DateTime.Now, DateTime.Now.AddYears(10));
                //if (i>0)
                //{
                //    return RJson("1", "商品添加成功！");
                //}
                //else
                //{
                //    return RJson("-1", "商品添加失败！");
                //}
                return RJson("-1", "不支持新增");
            }
            else//编辑
            {
                var model = probll.GetProduct(ID.Value);
                model.CategoryID = CategoryID;
                model.Descrition = Descrition;
                model.Img = Img;
                model.ImgList = ImgList;
                model.Type = Type;
                model.UpdateTime = DateTime.Now;
                model.Name = Name;
                model.Title = Title;
                model.DailySalary = DailySalary;
                model.Price = Price;
                if (probll.Update(model) > 0)
                {
                    return RJson("1", "更新成功！");
                }
                else
                {
                    return RJson("-2", "更新失败！");
                }

            }
        }
        [ValidateInput(false)]
        public JsonResult ProductSaveAll(Guid? ID, string Name, int CategoryID, string Title, string Number, int PriceType, decimal RealPrice, decimal Price
            , decimal Stock, string Img, string ImgList, string Descrition, int? OrderID, int Type, int Status, string hfSPValues, decimal? CashDiscount)
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
            if (OrderID == null) OrderID = 0;
            if (ID == null)//ID不正确，新增
            {

                ID = probll.Insert(Guid.Empty, Name, Title, Number, CategoryID, Img, ImgList, Descrition, RealPrice
                    , (RelexBarBLL.Common.enPayType)PriceType, Price, Stock, OrderID.Value, (RelexBarBLL.Common.enProductType)Type, DateTime.Now, DateTime.Now.AddYears(10), CashDiscount.HasValue ? CashDiscount.Value : 1);
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
                model.Status = Status;
                model.Stock = Stock;
                model.Title = Title;
                model.Type = Type;
                model.CashDiscount = CashDiscount.HasValue ? CashDiscount.Value : 1;
                model.UpdateTime = DateTime.Now;

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

        //public ActionResult BirthdayCardList(int? index, string key, int pageSize = 10)
        //{
        //    int sum;
        //    BirthDayCardBLL bll = new BirthDayCardBLL();
        //    List<BirthdayCard> list = bll.GetList(index == null ? 1 : index.Value, pageSize, key, out sum);
        //    ViewData["sum"] = sum;
        //    return PartialView(list);
        //}
        /// <summary>
        /// 新增或者修改生日牌
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Name"></param>
        /// <param name="IMG"></param>
        /// <param name="Remark"></param>
        /// <returns></returns>
        //public JsonResult EditBirthdayCard(int? ID, string Name, string IMG, string Remark)
        //{
        //    BirthDayCardBLL bll = new BirthDayCardBLL();
        //    if (ID == null)
        //    {
        //        int i = bll.Add(Name, IMG, Remark);
        //        if (i > 0)
        //        {
        //            return RJson("1", "新增成功");
        //        }
        //        return RJson("-1", "新增失败");
        //    }
        //    else
        //    {
        //        int i = bll.Update(ID.Value, Name, Remark, IMG, null);
        //        if (i > 0)
        //        {
        //            return RJson("1", "修改成功");
        //        }
        //        return RJson("-1", "修改失败");
        //    }
        //}
        //public JsonResult UpdateBirthdayCardStatus(int id, int status)
        //{
        //    status = status == 1 ? 1 : 0;
        //    BirthDayCardBLL bll = new BirthDayCardBLL();
        //    int i = bll.Update(id, null, null, null, status);
        //    if (i > 0)
        //    {
        //        return RJson("1", "修改成功");
        //    }
        //    else
        //    {
        //        return RJson("-1", "修改失败");
        //    }
        //}
        public ActionResult AdsCardList(int? index, string key, int pageSize = 10)
        {
            int sum;
            AdsListBLL bll = new AdsListBLL();
            List<AdsList> list = bll.GetList(index == null ? 1 : index.Value, pageSize, key, out sum);
            ViewData["sum"] = sum;
            return PartialView(list);
        }
        /// <summary>
        /// 新增或修改广告
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Name"></param>
        /// <param name="Title"></param>
        /// <param name="Img"></param>
        /// <param name="LinkTo"></param>
        /// <param name="Descrition"></param>
        /// <param name="BeginTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="Location"></param>
        /// <returns></returns>
        public JsonResult EditAds(Guid? ID, string Name, string Title, string Img, string LinkTo, string Descrition
            , DateTime? BeginTime, DateTime? EndTime, int? Location)
        {
            AdsListBLL bll = new AdsListBLL();
            if (ID == null)
            {
                if (BeginTime == null || EndTime == null || Location == null)
                {
                    return RJson("-1", "参数不正确");
                }
                int i = bll.Add(Name, Title, Img, LinkTo, Descrition, BeginTime.Value, EndTime.Value, Location.Value);
                if (i > 0)
                {
                    return RJson("1", "新增成功");
                }
                return RJson("-1", "新增失败");
            }
            else
            {
                int i = bll.Update(ID.Value, Name, Title, Img, LinkTo, Descrition, BeginTime, EndTime, Location, null);
                if (i > 0)
                {
                    return RJson("1", "修改成功");
                }
                return RJson("-1", "修改失败");
            }
        }
        //获取广告详情
        public string GetAD(Guid id)
        {
            AdsListBLL bll = new AdsListBLL();
            AdsList ad = bll.GetDetail(id);
            return ad.SerializeObject("yyyy-MM-dd");
        }
        //删除广告
        public JsonResult DeleteAD(Guid id)
        {
            AdsListBLL bll = new AdsListBLL();
            int i = bll.Delete(id);
            if (i > 0)
            {
                return RJson("1", "删除成功");
            }
            else
            {
                return RJson("-1", "删除失败");
            }
        }
        public JsonResult UpdateAdsStatus(Guid id, int status)
        {
            status = status == 1 ? 1 : 0;
            AdsListBLL bll = new AdsListBLL();
            int i = bll.Update(id, null, null, null, null, null, null, null, null, status);
            if (i > 0)
            {
                return RJson("1", "修改成功");
            }
            else
            {
                return RJson("-1", "修改失败");
            }
        }
        //订单列表
        public ActionResult OrderList(string Number, int? status, enOrderType? type, DateTime? beginTime, DateTime? endtime, int index = 1, int pageSize = 10)
        {
            OrdersBLL bll = new OrdersBLL();
            int sum = 0;
            enOrderStatus? Status = (enOrderStatus?)status;
            enOrderType? Type = (enOrderType?)type;
            List<RelexBarBLL.Models.OrderListModel> list = bll.GetOrderList(Number, Status, Type, beginTime, endtime, pageSize, index, out sum);
            @ViewData["sum"] = sum;
            return PartialView(list);
        }
        //导出订单列表
        public void ExcelOrderList(string Number, int? status, enOrderType? type, DateTime? beginTime, DateTime? endtime)
        {
            OrdersBLL bll = new OrdersBLL();
            int sum = 0;
            enOrderStatus? Status = (enOrderStatus?)status;
            List<RelexBarBLL.Models.OrderListModel> list = bll.GetOrderList(Number, Status, type, beginTime, endtime, 999999, 1, out sum);
            if (sum > 0)
            {
                string contents = "<table border=1>";
                contents += @"<tr>
                    <th>订单编号</th>
                    <th>下单时间</th>
                    <th>订单状态</th>
                    <th>订单金额</th>
                    <th>订单总金额</th>
                    <th>商品名称</th>
                    <th>订单类型</th>
                    <th>购买数量</th>
                    <th>收货人信息</th>
                </tr>";
                foreach (var temp in list)
                {
                    contents += @"<tr>
                            <td>" + temp.Number + @"</td>
                            <td>" + temp.CreateTime + @"</td>
                            <td>" + temp.toOrderStatus() + @"</td>
                            <td>" + temp.Price + @"</td>
                            <td>" + temp.TotalPrice + @"</td>
                            <td>" + temp.Name + @"</td>
                            <td>" + temp.toOrderType() + @"</td>
                            <td>" + temp.Count + @"</td>
                            <td>
                                姓名：" + temp.RecName + @"<br />
                                联系电话：" + temp.RecPhone + @"<br />
                                详细地址：" + temp.RecAddress + @"
                            </td>
                        </tr>";
                }
                contents += "</table>";
                ExportFile(contents);
            }
        }

        //已发货
        public JsonResult OrderSend(Guid id)
        {
            try
            {
                OrdersBLL bll = new OrdersBLL();
                int i = bll.OrderSend(id);
                if (i > 0)
                {
                    return RJson(1, "发货成功");
                }
                else
                {
                    return RJson(-1, ((ErrorCode)i).ToString());
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, ex.Message);
            }

        }

        //取消订单
        public JsonResult CancelOrder(Guid id)
        {
            OrdersBLL bll = new OrdersBLL();
            int i = bll.CancelOrder(null, id);
            if (i > 0)
            {
                return RJson(1, "取消成功");
            }
            else
            {
                return RJson(-1, ((Common.ErrorCode)i).ToString());
            }
        }


        public JsonResult GetHolidaysList(int year)
        {
            HolidaysBLL bll = new HolidaysBLL();
            return Json(bll.GetHolidaysByYear(year).SerializeObject("yyyy-MM-dd HH:mm:ss"));
        }
        /*
        [Filter.NoFilter]
        [Route("gzhd/dev/{id}")]
        public ActionResult DevManager(string id)
        {
            if (id != "gzhd6666admin")
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            return View();
        }




        [Filter.NoFilter]
        [Route("gzhd/dev/CUD/{id}")]
        public JsonResult CUD(string id, string sql)
        {
            using (RelexBarEntities entity = new RelexBarEntities())
            {
                try
                {
                    int i = entity.Database.ExecuteSqlCommand(sql, new SqlParameter[] { });
                    return Json(new { code = 1, result = i, msg = "" });
                }
                catch (Exception ex)
                {
                    return Json(new { code = -1, result = 0, msg = ex.Message });
                }
            }
        }
        [Filter.NoFilter]
        [Route("gzhd/dev/Q/{id}")]
        public JsonResult Q(string id, string sql)
        {
            using (RelexBarEntities entity = new RelexBarEntities())
            {
                try
                {

                    using (SqlCommand sqlcomm = new SqlCommand())
                    {
                        entity.Database.Connection.Open();
                        //command.CommandText = sql;

                        SqlDataAdapter adapter = new SqlDataAdapter(sql, entity.Database.Connection as SqlConnection);

                        DataSet ds = new DataSet();
                        adapter.Fill(ds);
                        DataTable schema = ds.Tables[0];

                        string[] head = new string[schema.Columns.Count];
                        List<string[]> list = new List<string[]>();
                        for (int i = 0; i < schema.Columns.Count; i++)
                        {
                            head[i] = schema.Columns[i].ColumnName;
                        }
                        for (int i = 0; i < schema.Rows.Count; i++)
                        {
                            string[] temp = new string[head.Length];
                            for (int j = 0; j < temp.Length; j++)
                            {
                                object tempdata = schema.Rows[i][head[j]];
                                temp[j] = tempdata == null ? null : tempdata.ToString();
                            }
                            list.Add(temp);
                        }
                        sqlcomm.Dispose();
                        entity.Database.Connection.Close();
                        return Json(new { code = 1, head = string.Join(",", head), result = list.SerializeObject(), msg = "" });

                    }
                }
                catch (Exception ex)
                {
                    return Json(new { code = -1, result = 0, msg = ex.Message });
                }
            }
        }
        */
        public JsonResult UpdateHolidaysList(List<RelexBarBLL.Models.HolidaysModel> list, int year)
        {
            HolidaysBLL bll = new HolidaysBLL();
            try
            {
                int i = bll.UpdateHolidays(year, list);
                if (i > 0)
                {
                    return RJson(1, "更新成功");
                }
                else
                {
                    return RJson(-1, "更新失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, "更新失败：" + ex.Message);
            }

        }
        public JsonResult ResetPwd(Guid id)
        {
            UsersBLL bll = new UsersBLL();
            try
            {
                int i = bll.ResetLoginPwd(id);
                if (i > 0)
                {
                    return RJson(1, "密码已重置为：123456");
                }
                else
                {
                    return RJson(-1, "密码重置失败！");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, "密码重置失败:" + ex.Message);
            }
        }
        ////获取兑换券
        //public ActionResult TicketList(int? index, int pageSize = 10)
        //{
        //    int sum;
        //    AdminUserBLL bll = new AdminUserBLL();
        //    List<AdminUser> list = bll.GetAdminList(index == null ? 1 : index.Value, pageSize, out sum);
        //    ViewData["sum"] = sum;
        //    return PartialView(list);
        //}
        public ActionResult UserHelpList(int? index, string key, int pageSize = 10)
        {
            UserHelpBLL bll = new UserHelpBLL();
            int sum = 0;
            List<UserHelp> list = bll.GetList(index == null ? 1 : index.Value, key, pageSize, out sum);
            ViewData["sum"] = sum;
            return PartialView(list);
        }
        public ActionResult EditUserHelp(Guid? id)
        {
            UserHelp uh = null;
            if (id != null)
            {
                uh = new UserHelpBLL().Get(id.Value);
            }
            return PartialView(uh);
        }
        public JsonResult UpdateUserHelpStatus(Guid ID, int status)
        {
            UserHelpBLL bll = new UserHelpBLL();
            enStatus s = enStatus.Enabled;
            if (status == 0)
            {
                s = enStatus.Unabled;
            }
            int i = bll.ChangeStatus(ID, s);
            if (i > 0)
                return RJson("1", "修改成功");
            return RJson("-1", "修改失败");
        }
        [ValidateInput(false)]
        public JsonResult DoEditUserHelp(Guid? ID, string Title, int Type, string Content)
        {
            UserHelpBLL bll = new UserHelpBLL();
            if (ID == null)
            {
                UserHelp uh = new UserHelp();
                uh.ID = Guid.NewGuid();
                uh.Title = Title;
                uh.Content = Content;
                uh.Type = Type;
                uh.Status = 1;
                uh.CreateTime = uh.UpdateTime = DateTime.Now;
                int i = bll.Add(uh);
                if (i > 0)
                {
                    return RJson(1, "新增成功");
                }
                else
                {
                    return RJson(-1, "新增失败");
                }
            }
            else
            {
                int i = bll.Update(ID.Value, Title, Content, Type, null);
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

        public JsonResult DeleteUserHelp(Guid id)
        {
            UserHelpBLL bll = new UserHelpBLL();
            int i = bll.Delete(id);
            if (i > 0)
                return RJson("1", "删除成功");
            return RJson("-1", "删除失败");
        }
        //商家入驻申请列表
        public ActionResult ShopReqList(int? index, enAdminMsgType? type, enAdminMsgResult? result, int pageSize = 10)
        {
            AdminMsgBLL bll = new AdminMsgBLL();
            int sum = 0;

            List<RelexBarBLL.Models.AdminMsgModel> list = bll.GetList(index == null ? 1 : index.Value, pageSize, out sum, type, result);
            ViewData["sum"] = sum;
            return PartialView(list);
        }
        public JsonResult DoShopReq(Guid ID, enAdminMsgResult result)
        {
            AdminMsgBLL bll = new AdminMsgBLL();
            try
            {
                int i = bll.Update(ID, result);
                if (i > 0)
                {
                    return RJson(1, "操作成功");
                }
                else
                {
                    return RJson(-1, "操作失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, "操作失败：" + ex.Message);
            }
        }

        public ActionResult CategoryList()
        {
            CategoryBLL bll = new CategoryBLL();
            return View(bll.GetAllList());
        }
        public JsonResult EditCategory(int? ID, string Name, string Title, string SrcDetail, string Description, int? OrderID, int IsShow, int HeadID)
        {
            CategoryBLL bll = new CategoryBLL();
            try
            {
                if (ID == null)
                {
                    int i = bll.Add(Name, Title, SrcDetail, Description, OrderID.HasValue ? OrderID.Value : 0, IsShow, HeadID);
                    if (i > 0)
                    {
                        return RJson(1, "新增成功");
                    }
                    else
                    {
                        return RJson(-1, "新增失败");
                    }
                }
                else
                {
                    int i = bll.Update(ID.Value, Name, Title, SrcDetail, Description, OrderID.HasValue ? OrderID.Value : 0, IsShow, HeadID);
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
            catch (Exception ex)
            {
                return RJson(-1, ex.Message);
            }
        }
        public JsonResult Category(int ID)
        {
            CategoryBLL bll = new CategoryBLL();
            return Json(bll.GetDetail(ID));
        }
        public JsonResult GetCategoryByHeadID(int ID)
        {
            CategoryBLL bll = new CategoryBLL();
            return Json(bll.GetAllList(null, "", 0).Where(m => m.ID != ID));
        }
        //头条列表/系统消息
        public ActionResult SysMsgList(int? index, string key, int pageSize = 10)
        {
            int sum;
            UserMsgBLL bll = new UserMsgBLL();

            List<UserMsg> list = bll.GetList(index == null ? 1 : index.Value, pageSize, out sum, Guid.Empty, null, key, enMessageType.TouTiao);
            ViewData["sum"] = sum;
            return PartialView(list);
        }
        //编辑头条/系统消息
        [ValidateInput(false)]
        public JsonResult EditSysMsg(Guid? ID, string Title, string Content)
        {
            UserMsgBLL bll = new UserMsgBLL();
            try
            {
                if (ID == null)
                {
                    int i = bll.Add(Title, Content);
                    if (i > 0)
                    {
                        return RJson(1, "新增成功");
                    }
                    else
                    {
                        return RJson(-1, "新增失败");
                    }
                }
                else
                {
                    int i = bll.Update(ID.Value, Title, Content, null);
                    if (i > 0)
                    {
                        return RJson(1, "更新成功");
                    }
                    else
                    {
                        return RJson(-1, "更新失败");
                    }
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, "更新失败：" + ex.Message);
            }

        }
        public JsonResult GetSysMsg(Guid ID)
        {
            UserMsgBLL bll = new UserMsgBLL();
            return Json(bll.Get(ID));
        }

        public JsonResult UpdateSysMsgStatus(Guid id, enStatus status)
        {
            UserMsgBLL bll = new UserMsgBLL();
            try
            {
                int i = bll.Update(id, null, null, (int)status);
                if (i > 0)
                {
                    return RJson(1, "更新成功");
                }
                else
                {
                    return RJson(-1, "更新失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, "更新失败：" + ex.Message);
            }

        }

        public JsonResult DelSysMsg(Guid id)
        {
            int i = new UserMsgBLL().Del(id);
            if (i > 0)
            {
                return RJson(1, "删除成功");
            }
            else
            {
                return RJson(1, "删除失败");
            }
        }
        public ActionResult ShopList(int? index, string key, int pageSize = 10)
        {
            ShopBLL bll = new ShopBLL();
            int sum = 0;
            List<RelexBarBLL.Models.AdminShop> list = bll.GetAdminShopList(index == null ? 1 : index.Value, pageSize, out sum, key);

            return Content(new { sum = sum, data = list }.SerializeObject("yyyy-MM-dd"));
        }
        public ActionResult GetShop(Guid id)
        {
            return Json(new ShopBLL().Get(id));
        }
        public JsonResult Save(Guid ID, string ShopName, int AgentType)
        {
            ShopBLL bll = new ShopBLL();
            int i = bll.UpdateShop(ID, ShopName, AgentType);
            if (i > 0)
            {
                return RJson(1, "修改成功");
            }
            else
            {
                return RJson(-1, "修改失败");
            }
        }
        //修改商家信息
        public JsonResult UpdateShopInfo(string Img, string ShopName, string ChatQQ, string BackImg, string ServicePhone)
        {
            try
            {
                ShopBLL bll = new ShopBLL();
                Shop shop = bll.GetByUID(Guid.Empty);
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
        /// <summary>
        /// 发送通知消息
        /// </summary>
        /// <param name="sendType">1所有人,0，指定人</param>
        /// <param name="recuid"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public JsonResult SendMsg(int sendType, Guid? recuid, string content)
        {
            if (sendType == 0 && recuid == null)
            {
                return RJson(-1, "参数错误");
            }
            if (sendType != 1 && sendType != 0)
            {
                return RJson(-1, "参数错误");
            }
            UserMsgBLL bll = new UserMsgBLL();
            int i = -1;
            if (sendType == 1)
            {
                i = bll.SendMsgToAllUser("", "", content, enMessageType.System);
            }
            else
            {
                i = bll.SendMsg(recuid.Value, "", "", content, enMessageType.System);
            }
            if (i > 0)
            {
                return RJson(1, "发送成功");
            }
            return RJson(-1, "发送失败");

        }

        #region ExportFile

        public void ExportFile(string content, string filename = "")
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = "YWY_" + DateTime.Now.ToString("HHmmssfff") + ".xls";
            }
            Response.Clear();
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.ContentType = "Application/ms-excel";
            //导出文件名称
            Response.AppendHeader("Content-Disposition", "attachment;filename=\"" + filename + "\"");
            Response.Write(content);
            Response.Flush();
            Response.Close();
        }

        public void ExportFile(string filename, byte[] content)
        {
            Response.Clear();
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.ContentType = "Application/ms-excel";
            //导出文件名称
            Response.AppendHeader("Content-Disposition", "attachment;filename=\"" + filename + "\"");
            Response.BinaryWrite(content);
            Response.Flush();
            Response.Close();
        }

        public void WriteFile(string source, string outfilename)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (System.IO.File.Exists(source))
                {
                    byte[] content = System.IO.File.ReadAllBytes(source);
                    ExportFile(outfilename, content);
                }
            }
        }

        #endregion
    }
}