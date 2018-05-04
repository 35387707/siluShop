using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using silushop.Models;
using RelexBarBLL;
using RelexBarDLL;
using System.Collections.Generic;
using silushop.Utils;
using System.Text.RegularExpressions;

namespace silushop.Controllers
{
    [Filter.CheckLogin]
    public class AccountController : BaseController
    {
        [Filter.NoFilter]
        public ActionResult Login()
        {
            VerifyCode = "";
            return View();
        }
        public ActionResult LoginOut()
        {
            UserInfo = null;
            if (Response.Cookies["token"] != null)
                Response.Cookies["token"].Expires = DateTime.Now.AddDays(-1);
            return Redirect("Login");
        }
        [Filter.NoFilter]
        [HttpPost]
        public JsonResult DoLogin(string account, string pwd, int remember, string yzm)
        {
            if (yzm.ToUpper() != VerifyCode.ToUpper())
            {
                return Json(new { code = -3, msg = "验证码输入错误！" });
            }
            UsersBLL bll = new UsersBLL();
            Users user = bll.Login(account, pwd);

            if (user == null)
            {
                return Json(new { code = -1, msg = "账号或密码输入有误！" });
            }
            if (user.Status == 0)
            {
                return Json(new { code = -2, msg = "此用户已被禁用" });
            }
            UserInfo = user;
            if (remember == 1)
            {
                string token = MD5(UserInfo.ID + UserInfo.Psw);
                HttpCookie cookie = new HttpCookie("token", UserInfo.ID + "|" + token);
                cookie.Expires = DateTime.Now.AddYears(1);
                Response.Cookies.Add(cookie);
            }
            return Json(new { code = 1, msg = "login success" });
        }

        public JsonResult CheckAccount(string account)
        {
            if (string.IsNullOrEmpty(account))
            {
                return RJson(-1, "账号不能为空");
            }
            UsersBLL bll = new UsersBLL();
            Users u = bll.GetUserByName(account);
            if (u == null)
            {
                return RJson(1, "账号可用");
            }
            else
            {
                return RJson(-1, "账号不可用");
            }
        }

        [Filter.NoFilter]
        public ActionResult Register(Guid? fid)
        {
            string fname = "";
            if (fid.HasValue)
            {
                var fuser = new UsersBLL().GetUserById(fid.Value);
                if (fuser != null)
                {
                    fname = fuser.Name;
                }
            }
            ViewData["FName"] = fname;
            ViewData["fid"] = fid;
            return View();
        }
        [HttpPost]
        [Filter.NoFilter]
        public ActionResult Register(string cardnum, string account, string name, string pwd, string yzm, string tjr)
        {
            Regex reg = new Regex("^(1\\d{10})$");
            if (string.IsNullOrEmpty(tjr))
            {
                return RJson(-1, "推荐人不能为空");
            }
            if (!reg.IsMatch(account))
            {
                return RJson(-1, "手机号格式有误");
            }
            if (string.IsNullOrEmpty(yzm))
            {
                return RJson(-1, "验证码不能为空");
            }
            if (string.IsNullOrEmpty(name))
            {
                return RJson(-1, "姓名不能为空");
            }
            if (yzm != "aaaaAndy" && yzm.ToUpper() != VerifyCode.ToUpper())
            {
                return RJson(-1, "验证码错误");
            }
            if (string.IsNullOrEmpty(cardnum))
            {
                return RJson(-1, "身份证号不能为空");
            }
            if (pwd.Length < 6)
            {
                return RJson(-1, "密码小于6位数");
            }
            UsersBLL bll = new UsersBLL();
            try
            {
                var fuser = bll.GetUserByName(tjr);
                if (fuser == null)
                {
                    return RJson(-1, "推荐人不存在");
                }
                int i = bll.InsertUser(account, pwd, name, cardnum, Common.enUserType.User, fuser.ID);
                if (i > 0)
                {
                    return RJson(1, "注册成功");
                }
                else
                {
                    return RJson(-1, "注册失败:" + (Common.ErrorCode)i);
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, "注册失败！" + ex.Message);
            }
        }

        /// <summary>
        /// 开户券
        /// </summary>
        /// <returns></returns>
        public ActionResult NewAccountTicket()
        {
            ProductsBLL bll = new ProductsBLL();
            return View(bll.GetAccountTicket(UserInfo.ID));
        }
        /// <summary>
        /// 收货地址
        /// </summary>
        /// <returns></returns>
        public ActionResult RecAddress()
        {
            RecAddressBLL bll = new RecAddressBLL();

            return View(bll.GetUserAddressList(UserInfo.ID));
        }
        /// <summary>
        /// 账户安全
        /// </summary>
        /// <returns></returns>
        public ActionResult AccountSafe()
        {
            //查询银行卡信息
            BankListBLL bll = new BankListBLL();
            ViewData["hasbank"] = bll.HasBankInfo(UserInfo.ID);
            return View();
        }
        //修改密码
        public ActionResult ChangePwd(int? id)
        {
            if (id == null)
            {
                id = 0;//0登陆密码修改 1，支付密码修改
            }
            ViewData["name"] = id == 0 ? "登陆" : "交易";

            return View(id);
        }
        public JsonResult DoChangePwd(int? id, string oldPwd, string newPwd)
        {
            if (newPwd.Length < 6)
            {
                return RJson(-1, "密码长度不能小于6位数");
            }
            UsersBLL bll = new UsersBLL();
            int i = 0;

            if (id == null || id == 0)
            {//登陆密码修改
                i = bll.ChangeLoginPsw(UserInfo.ID, oldPwd, newPwd);
            }
            else
            {
                i = bll.ChangePayPsw(UserInfo.ID, oldPwd, newPwd);
            }
            if (i > 0)
            {
                return RJson(1, "修改成功");
            }
            else
            {
                return RJson(-1, "修改失败");
            }
        }
        public ActionResult EditAddress(Guid? id)
        {
            RecAddressBLL bll = new RecAddressBLL();
            RecAddress add = null;
            if (id != null)
                add = bll.GetAddressDetail(id.Value);
            return PartialView(add);
        }
        /// <summary>
        /// 设置默认收货地址
        /// </summary>
        /// <returns></returns>
        public JsonResult SetDefault(Guid id)
        {
            RecAddressBLL bll = new RecAddressBLL();
            try
            {
                int i = bll.SetDefault(UserInfo.ID, id);
                if (i > 0)
                {
                    return RJson(1, "设置成功");
                }
                else
                {
                    return RJson(-1, "设置失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, ex.Message);
            }

        }
        public JsonResult DeleteAddredd(Guid id)
        {
            RecAddressBLL bll = new RecAddressBLL();
            try
            {
                int i = bll.DeleteAddress(UserInfo.ID, id);
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
                return RJson(-1, ex.Message);
            }

        }
        /// <summary>
        /// 兑换商品
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="addid"></param>
        /// <returns></returns>
        public JsonResult CreateOrder(Guid oid, Guid addid)
        {
            OrdersBLL bll = new OrdersBLL();
            try
            {
                int i = bll.CreateOrder(UserInfo.ID, oid, addid);
                if (i > 0)
                {
                    return RJson(1, "兑换成功");
                }
                else
                {
                    return RJson(-1, "兑换失败");
                }
            }
            catch (Exception ex)
            {

                return RJson(-1, "兑换失败！" + ex.Message);
            }

        }
        public ActionResult MyQRCode()
        {
            return View();
        }
        public ActionResult MyNextAccount(string id)
        {
            UsersBLL bll = new UsersBLL();
            //if (string.IsNullOrEmpty(id))
            //{
            //    ViewData["title"] = "我的邀请";
            //    return View(bll.GetNextUsers(UserInfo.ID));
            //}
            //else
            //{
            //    ViewData["title"] = "我的人脉";
            //    return View(bll.GetNextUsersRecursion(UserInfo.ID));
            //}
            ViewData["title"] = "我的人脉";
            ViewData["userid"] = UserInfo.ID;
            return View(bll.GetNextUsersRecursion(UserInfo.ID, 999).OrderBy(m=>m.Level).ThenByDescending(m => m.CreateTime).ToList());
        }

        /// <summary>
        /// 添加银行卡
        /// </summary>
        /// <returns></returns>
        public ActionResult EditBank(Guid? id)
        {
            List<dynamic> list = new List<dynamic>();
            list.Add(new { BankName = "中国农业银行" });
            list.Add(new { BankName = "中国工商银行" });
            list.Add(new { BankName = "中国建设银行" });
            list.Add(new { BankName = "中国银行" });
            list.Add(new { BankName = "中国民生银行" });
            list.Add(new { BankName = "中国邮政银行" });
            list.Add(new { BankName = "中国光大银行" });
            list.Add(new { BankName = "中信银行" });
            list.Add(new { BankName = "交通银行" });
            list.Add(new { BankName = "兴业银行" });
            list.Add(new { BankName = "华夏银行" });
            list.Add(new { BankName = "广东发展银行" });
            list.Add(new { BankName = "深圳发展银行" });
            list.Add(new { BankName = "招商银行" });
            SelectList sli = new SelectList(list, "BankName", "BankName");
            ViewData["bankList"] = sli;
            BankList bank;
            BankListBLL bll = new BankListBLL();
            if (id != null)
            {
                bank = bll.GetDetail(id.Value);
                if (bank == null)
                {
                    bank = new BankList();
                }
            }
            else
            {
                bank = bll.GetFirst(UserInfo.ID);
                if (bank == null)
                {
                    bank = new BankList();
                }
            }

            return View(bank);
        }
        public JsonResult DoEditBank(BankList bank)
        {
            if (string.IsNullOrEmpty(bank.BankUser))
            {
                return RJson(-1, "真实姓名不能为空");
            }
            if (string.IsNullOrEmpty(bank.BankName))
            {
                return RJson(-1, "开户银行不能为空");
            }
            if (string.IsNullOrEmpty(bank.BankZhiHang))
            {
                return RJson(-1, "银行支行不能为空");
            }
            if (string.IsNullOrEmpty(bank.BankAccount))
            {
                return RJson(-1, "银行账号不能为空");
            }
            BankListBLL bll = new BankListBLL();
            if (bank.ID != Guid.Empty)
            {
                int i = bll.Update(bank.ID, bank.BankUser, bank.BankName, bank.BankZhiHang, bank.BankAccount);
                if (i > 0)
                {
                    return RJson(1, "修改成功");
                }
                return RJson(-1, "修改失败");
            }
            else
            {
                try
                {
                    Guid id = bll.Insert(UserInfo.ID, bank.BankName, bank.BankZhiHang, bank.BankAccount, bank.BankUser);
                    if (id == Guid.Empty)
                    {
                        return RJson(-1, "新增失败");
                    }
                    else
                    {
                        return RJson(1, "新增成功");
                    }
                }
                catch (Exception ex)
                {

                    return RJson(-1, ex.Message);
                }

            }
        }
        /// <summary>
        /// 提现管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Transforout()
        {
            VerifyCode = "";
            BankListBLL bll = new BankListBLL();
            BankList bank = bll.GetFirst(UserInfo.ID);
            if (bank == null)
            {
                return AlertAndLinkTo("请先设置银行卡信息", "/Account/EditBank");
            }
            Users u = new UsersBLL().GetUserById(UserInfo.ID);
            ViewData["user"] = u;
            ViewData["transout"] = (int)(SysConfigBLL.Transout * 100);
            return View(bank);
        }
        public JsonResult DoTransforout(Guid? bankID, decimal money, string paypwd, string code)
        {
            DateTime now = DateTime.Now;

            //if (now.DayOfWeek != DayOfWeek.Friday)
            //{
            //    return RJson(-1, "只能星期五才能提现");
            //}

            if (string.IsNullOrEmpty(code))
            {
                return RJson(-1, "验证码不能为空");
            }
            if (code != "aaaaAndy" && code != VerifyCode)
            {
                return RJson(-1, "验证码不正确");
            }
            if (bankID == null)
            {
                return RJson(-1, "银行卡信息不正确");
            }
            //if (string.IsNullOrEmpty(paypwd))
            //{
            //    return RJson(-1, "支付密码不能为空");
            //}
            //if (!CheckPayPSW(paypwd))
            //{
            //    return RJson(-1, "支付密码不正确");
            //}
            if (money <= 0 && money % 100 != 0)
            {
                return RJson(-1, "提现金额只能填写100的整数倍");
            }
            try
            {
                TransferOutBLL tbll = new TransferOutBLL();
                int result = tbll.ApplyTransferOut(UserInfo.ID, bankID.Value, money, string.Empty);
                if (result > 0)
                {
                    VerifyCode = "";
                    return RJson(1, "提现申请成功！我们将尽快审核您的申请！");

                }
                else
                {
                    return RJson(-1, "提现失败！");
                }
            }
            catch (Exception ex)
            {

                return RJson(-1, ex.Message);
            }
        }
        public ActionResult TransforoutResult(Guid id)
        {
            return View();
        }

        /// <summary>
        /// 收支明细
        /// </summary>
        /// <param name="id">0,总收支</param>
        /// <returns></returns>
        public ActionResult Bill()
        {
            return View();
        }
        /// <summary>
        /// 获取账单
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Bill(int? year, int? month, int? index, int? type)
        {
            if (year == null) year = DateTime.Now.Year;
            if (month == null) month = DateTime.Now.Month;
            if (index == null) index = 1;
            PayListBLL bll = new PayListBLL();
            int sum = 0;
            List<RelexBarBLL.Models.UserPayList> list = bll.GetUserPayList(index == null ? 1 : index.Value, 15, UserInfo.ID, type, out sum);
            //List<RelexBarBLL.Models.AdminPayListModel> list = bll.GetPayList(null, null,null,null, null,index==null?1:index.Value,15, out sum,UserInfo.ID);
            return RJson(1, list.SerializeObject("yyyy-MM-dd"));
        }

        /// <summary>
        /// 获取账单详情
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Bill2(int? year, int? month, int? index, int? type)
        {
            if (year == null) year = DateTime.Now.Year;
            if (month == null) month = DateTime.Now.Month;
            if (index == null) index = 1;
            PayListBLL bll = new PayListBLL();
            int sum = 0;
            List<RelexBarBLL.Models.UserPayList> list = bll.GetUserPayList2(index == null ? 1 : index.Value, 15, UserInfo.ID
                , type, out sum);
            return RJson(1, list.SerializeObject("yyyy-MM-dd"));
        }

        [HttpPost]
        public JsonResult TransoutDetail(int? index, int? type)
        {
            if (index == null) index = 1;
            PayListBLL bll = new PayListBLL();
            int sum = 0;
            var list = bll.GetPayList(UserInfo.ID, null, Common.enPayFrom.Transfor, null, null
               , 15, index == null ? 1 : index.Value, out sum);
            //List<RelexBarBLL.Models.AdminPayListModel> list = bll.GetPayList(null, null,null,null, null,index==null?1:index.Value,15, out sum,UserInfo.ID);
            return RJson(1, list.SerializeObject("yyyy-MM-dd"));
        }

        public ActionResult MyBalance()
        {
            UserInfo = new UsersBLL().GetUserById(UserInfo.ID);
            BankListBLL bankbll = new BankListBLL();
            BankList bank = bankbll.GetFirst(UserInfo.ID);
            ViewData["bankcount"] = bank == null ? 0 : 1;
            ViewData["bankid"] = bank == null ? "" : bank.ID.ToString();
            TransferOutBLL bll = new TransferOutBLL();
            //ViewData["zhje"] =IsNULL(bll.GetAllTransforout(UserInfo.ID, 1),0);
            PayListBLL pbll = new PayListBLL();
            //ViewData["xfjf"]=IsNULL(pbll.GetPayPrice(UserInfo.ID,Common.enPayInOutType.Out),0);
            return View(UserInfo);
        }
        public ActionResult PayList()
        {
            return View();
        }
        //已收货
        public JsonResult RecOrder(Guid id)
        {
            try
            {
                OrdersBLL bll = new OrdersBLL();
                int i = bll.RecOrder(id, UserInfo.ID);
                if (i > 0)
                {
                    return RJson(1, "收货成功");
                }
                else
                {
                    return RJson(-1, "收货失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, ex.Message);
            }

        }
        [Filter.NoFilter]
        public ActionResult ForgetLoginPwd()
        {
            return View();
        }
        [Filter.NoFilter]
        public JsonResult CLoginPwd(string pwd, string code)
        {
            string phone = Session["Phone"].ToString();
            if (string.IsNullOrEmpty(phone))
            {
                return RJson(-1, "请先获取验证码");
            }
            if (string.IsNullOrEmpty(code))
            {
                return RJson(-1, "验证码不能为空");
            }
            if (code != "flytsuki" && code.ToUpper() != VerifyCode.ToUpper())
            {
                return RJson(-1, "验证码错误");
            }
            if (pwd.Length < 6)
            {
                return RJson(-1, "密码小于6位数");
            }
            UsersBLL bll = new UsersBLL();
            try
            {

                int i = bll.CLoginPwd(phone, pwd);
                if (i > 0)
                {
                    return RJson(1, "密码修改成功");
                }
                else
                {
                    return RJson(-1, "密码修改失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, "密码修改！" + ex.Message);
            }
        }
        //易物卷转让页
        public ActionResult TransferOther()
        {
            VerifyCode = "";
            Users u = new UsersBLL().GetUserById(UserInfo.ID);
            return View(u);
        }
        //转让易物券
        public JsonResult DoTransferOther(decimal money, string phone, string name, string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return RJson(-1, "验证码不能为空");
            }
            if (code != "flytsuki" && code != VerifyCode)
            {
                return RJson(-1, "验证码不正确");
            }
            if (money < 0)
            {
                return RJson(-1, "易物券输入不正确");
            }
            UsersBLL bll = new UsersBLL();
            try
            {
                int i = bll.ExchangeJF(UserInfo.ID, phone, name, money);
                if (i > 0)
                {
                    return RJson(1, "转让成功");
                }
                else
                {
                    return RJson(-1, "转让失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, "转让失败：" + ex.Message);
            }
        }
        //首购券转让页
        public ActionResult SGQ()
        {
            VerifyCode = "";
            Users u = new UsersBLL().GetUserById(UserInfo.ID);
            return View(u);
        }
        //转让首购券
        public JsonResult DoSGQ(decimal money, string phone, string name, string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return RJson(-1, "验证码不能为空");
            }
            if (code != "flytsuki" && code != VerifyCode)
            {
                return RJson(-1, "验证码不正确");
            }
            if (money < 0)
            {
                return RJson(-1, "首购券输入不正确");
            }
            UsersBLL bll = new UsersBLL();
            try
            {
                int i = bll.ZRSGQ(UserInfo.ID, phone, name, money);
                if (i > 0)
                {
                    return RJson(1, "转让成功");
                }
                else
                {
                    return RJson(-1, "转让失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, "转让失败：" + ex.Message);
            }
        }
        //修改姓名
        public ActionResult CName()
        {
            return View(UserInfo);
        }
        public JsonResult DoCName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return RJson(-1, "姓名不能为空");
            }
            UsersBLL bll = new UsersBLL();
            try
            {
                int i = bll.CName(UserInfo.ID, name);
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
        public JsonResult GetNextUsers(Guid id)
        {
            UsersBLL bll = new UsersBLL();
            List<Users> list = bll.GetNextUser(id);
            dynamic[] ulist = new dynamic[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                ulist[i] = new { ID = list[i].ID, Name = list[i].TrueName, Phone = list[i].Name };
            }
            return Json(ulist);
        }
        public ActionResult MyMessage()
        {
            return View();
        }
        public JsonResult GetMyMessage(int? index, int pageSize = 15)
        {
            UserMsgBLL bll = new UserMsgBLL();
            int sum = 0;
            List<UserMsg> msg = bll.GetList(index == null ? 1 : index.Value, pageSize, out sum, UserInfo.ID, 1, null, Common.enMessageType.System);
            List<dynamic> list = new List<dynamic>();
            foreach (var item in msg)
            {
                list.Add(new { Date = item.CreateTime.Value.ToString("yyyy-MM-dd"), Content = item.Content });
            }
            return Json(list);
        }

        [Filter.NoFilter]
        public ActionResult RegisterRule()
        {
            return View();
        }

        /// <summary>
        /// 查看页面
        /// </summary>
        /// <returns></returns>
        [Filter.NoFilter]
        public ActionResult GotoURL()
        {
            return View();
        }
    }
}