using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;
using System.Data.SqlClient;
using System.Web;

namespace RelexBarBLL
{
    //CommendID为上家id，fid为直推人id
    public partial class UsersBLL : BaseBll
    {
        public decimal GWQBL = 0.1M;//购物券比例
        /// <summary>
        /// 验证是否能创建用户
        /// </summary>
        /// <param name="UID">当前用户，开户的用户</param>
        /// <param name="NextUID">新开用户在此用户下</param>
        /// <returns></returns>
        public bool CanCreateUser(Guid UID, Guid NextUID)
        {
            using (DBContext)
            {
                if (!IsChildren(UID, NextUID, DBContext))
                {
                    return false;
                }
                if (DBContext.Users.Where(m => m.CommendID == NextUID).Count() < 2)
                {
                    return true;
                }
                //if (UID == NextUID && DBContext.Users.Where(m => m.CommendID == UID).Count() < 2)
                //{
                //    return true;
                //}
                //else if (UID != NextUID)
                //{
                //    Users u = DBContext.Users.Where(m => m.ID == NextUID && m.CommendID == UID).FirstOrDefault();
                //    if (u == null)
                //    {
                //        return false;
                //    }
                //    if (DBContext.Users.Where(m => m.CommendID == NextUID).Count() < 2)
                //    {
                //        return true;
                //    }
                //}
                return false;

            }
        }
        /// <summary>
        /// 验证cid是否是fid的子节点
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="cid"></param>
        /// <returns></returns>
        public bool IsChildren(Guid fid, Guid cid, RelexBarEntities entity)
        {
            Users u = entity.Users.Where(m => m.ID == cid).FirstOrDefault();
            if (u == null)
            {
                return false;
            }
            if (u.FID == fid)
            {
                return true;
            }
            if (u.FID == null)
            {
                return true;
            }
            return IsChildren(fid, u.FID.Value, entity);
        }

        public bool CanCreateUser(RelexBarEntities entity, Guid UID, Guid NextUID)
        {
            if (UID == NextUID && entity.Users.Where(m => m.CommendID == UID).Count() < 2)
            {
                return true;
            }
            else if (UID != NextUID)
            {
                //Users u = entity.Users.Where(m => m.ID == NextUID && m.CommendID == UID).FirstOrDefault();
                //if (u == null)
                //{
                //    return false;
                //}
                if (entity.Users.Where(m => m.CommendID == NextUID).Count() < 2)
                {
                    return true;
                }
            }
            return false;
        }

        public List<Users> GetNextUser(Guid UID)
        {
            using (DBContext)
            {
                return DBContext.Users.Where(m => m.CommendID == UID).OrderBy(m => m.CreateTime).ToList();
            }
        }
        /// <summary>
        /// 获取商品信息
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public ProductList GetProductByLevel(int level)
        {
            using (DBContext)
            {
                var q = from a in DBContext.ProductList
                        where a.Level == level && a.Type == 1
                        select a;

                return q.FirstOrDefault();
            }
        }
        /// <summary>
        /// 获取最后那条记录
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public PayRecord GetLastRecord(Guid UID)
        {
            using (DBContext)
            {
                return DBContext.PayRecord.OrderByDescending(m => m.Level).FirstOrDefault(m => m.UID == UID);
            }
        }
        /// <summary>
        /// 支付成功后的区县市代理奖励
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="shop"></param>
        /// <param name="money"></param>
        public void PaySuccessJL(RelexBarEntities entity, Shop shop, decimal money)
        {
            Shop pshop = shop;
            if (pshop == null)
            {
                throw new Exception("商家不存在");
            }
            //国，省，市，区/县
            string areaid = pshop.AreaID;
            //获得区县代理 商家的用户
            List<Users> qUser = entity.Users.Where(u => entity.Shop.Where(s => s.AreaID == areaid && s.AgentType == (int)Common.enShopAgentType.District).Select(s => s.UID).ToList().Contains(u.ID)).ToList();
            //市代理
            string sareaid = areaid.Substring(0, areaid.LastIndexOf(','));
            List<Users> sUser = entity.Users.Where(u => entity.Shop.Where(s => s.AgentType == (int)Common.enShopAgentType.City && s.AreaID.StartsWith(areaid)).Select(s => s.UID).ToList().Contains(u.ID)).ToList();
            decimal payPrice = money;
            decimal qjl = payPrice * 0.01M;//区县奖励
            decimal sjl = payPrice * 0.02M;//市代理
            for (int i = 0; i < qUser.Count; i++)
            {
                qUser[i].Score += qjl;
                PayList pl = new PayList();
                pl.UID = qUser[i].ID;
                pl.InOut = (int)Common.enPayInOutType.In;
                pl.PayType = (int)Common.enPayType.Point;
                pl.FromTo = (int)Common.enPayFrom.ShopAgent;
                pl.Val = qjl;
                pl.Remark = "区/县代理2%收入";
                pl.Status = 1;
                pl.CreateTime = pl.UpdateTime = DateTime.Now;
                pl.PriceType = (int)Common.enPriceType.Score;
                entity.PayList.Add(pl);
            }
            for (int i = 0; i < sUser.Count; i++)
            {
                sUser[i].Score = sjl;
                PayList pl = new PayList();
                pl.UID = sUser[i].ID;
                pl.InOut = (int)Common.enPayInOutType.In;
                pl.PayType = (int)Common.enPayType.Point;
                pl.FromTo = (int)Common.enPayFrom.ShopAgent;
                pl.Val = qjl;
                pl.Remark = "市代理1%收入";
                pl.Status = 1;
                pl.CreateTime = pl.UpdateTime = DateTime.Now;
                pl.PriceType = (int)Common.enPriceType.Score;
                entity.PayList.Add(pl);
            }


        }
        /// <summary>
        /// 支付购物券给商家
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="ShopID"></param>
        /// <param name="Money"></param>
        /// <param name="VID">验证码id</param>
        /// <returns></returns>
        public int PayToShop(Guid UID, Guid ShopID, decimal Money, Guid VID)
        {
            using (DBContext)
            {
                DateTime d = DateTime.Now.AddMinutes(-5);
                VerifyCodes v = DBContext.VerifyCodes.Where(m => m.ID == VID && m.CreateTime >= d).FirstOrDefault();
                if (v == null)
                {
                    throw new Exception("二维码已过期");
                }
                Users user = DBContext.Users.Where(m => m.ID == UID).FirstOrDefault();
                if (user == null)
                {
                    throw new Exception("用户不存在");
                }
                if (user.ShoppingVoucher < Money)
                {
                    throw new Exception("购物券不足");
                }
                Users shop = DBContext.Users.Where(m => m.ID == ShopID && m.UserType == (int)Common.enUserType.Shop).FirstOrDefault();
                if (shop == null)
                {
                    throw new Exception("商家不存在");
                }
                user.ShoppingVoucher -= Money;
                shop.Score += Money;
                PayList pl = new PayList();
                pl.UID = UID;
                pl.InOut = (int)enPayInOutType.Out;
                pl.PayType = (int)enPayListType.ShoppingVoucher;
                pl.FromTo = (int)enPayFrom.OutLinePay;
                pl.Val = Money;
                pl.Remark = "线下扫码支付购物券";
                pl.Status = 1;
                pl.CreateTime = pl.UpdateTime = DateTime.Now;
                pl.PriceType = (int)enPriceType.ShoppingVoucher;
                DBContext.PayList.Add(pl);
                PayList pl2 = new PayList();
                pl2.UID = ShopID;
                pl2.InOut = (int)enPayInOutType.In;
                pl2.PayType = (int)enPayListType.OutLinePay;
                pl2.FromTo = (int)enPayFrom.OutLinePay;
                pl2.Val = Money;
                pl2.Remark = "线下扫码收款";
                pl2.Status = 1;
                pl2.CreateTime = pl2.UpdateTime = DateTime.Now;
                pl2.PriceType = (int)enPriceType.Score;
                DBContext.PayList.Add(pl2);
                PaySuccessJL(DBContext, DBContext.Shop.Where(m => m.UID == shop.ID).FirstOrDefault(), Money);
                return DBContext.SaveChanges();
            }

        }
        /// <summary>
        /// 修改姓名
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public int CName(Guid UID, string Name)
        {
            using (DBContext)
            {
                Users user = DBContext.Users.Where(m => m.ID == UID).FirstOrDefault();
                if (user == null)
                {
                    throw new Exception("用户不存在");
                }
                user.TrueName = Name;
                return DBContext.SaveChanges();
            }
        }
        /// <summary>
        /// 转让首购券
        /// </summary>
        /// <returns></returns>
        public int ZRSGQ(Guid UID, string recPhone, string recName, decimal money)
        {
            using (DBContext)
            {
                Users u = DBContext.Users.Where(m => m.ID == UID).FirstOrDefault();
                if (u == null)
                {
                    throw new Exception("用户不存在");
                }
                if (u.Balance < money)
                {
                    throw new Exception("首购券不足");
                }
                Users recu = DBContext.Users.Where(m => m.Name == recPhone && m.TrueName == recName).FirstOrDefault();
                if (recu == null)
                {
                    throw new Exception("用户不存在");
                }
                u.Balance -= money;
                recu.Balance += money;

                Logs log = new Logs();//可提现金额

                Recharge r = new Recharge();
                r.ID = Guid.NewGuid();
                r.FromUID = u.ID;
                r.ToID = recu.ID;
                r.RechType = (int)enRechType.SGQ;
                r.Val = -money;
                r.ComVal = 0;
                r.Status = 1;
                r.CreateTime = r.UpdateTime = DateTime.Now;
                DBContext.Recharge.Add(r);
                Recharge r2 = new Recharge();
                r2.ID = Guid.NewGuid();
                r2.FromUID = recu.ID;
                r2.ToID = u.ID;
                r2.RechType = (int)enRechType.SGQ;
                r2.Val = money;
                r2.ComVal = 0;
                r2.Status = 1;
                r2.CreateTime = r2.UpdateTime = DateTime.Now;
                DBContext.Recharge.Add(r2);
                InsertLog(DBContext, enLogType.User, "用户UID:" + UID + "给UID:" + recu.ID + "转让首购券：￥" + money + ",转出后剩余：" + u.Balance, u.ID);
                InsertLog(DBContext, enLogType.User, "用户UID:" + UID + "收到UID:" + recu.ID + "首购券：￥" + money + ",转入后剩余：" + recu.Balance, recu.ID);
                PayList pl = new PayList();
                pl.UID = UID;
                pl.InOut = (int)enPayInOutType.Out;
                pl.PayType = (int)enPayListType.SGQ;
                pl.FromTo = (int)enPayFrom.Exchange;
                pl.Val = money;
                pl.Remark = "转给" + recu.Name + "￥:" + money + "首购券";
                pl.Status = 1;
                pl.CreateTime = pl.UpdateTime = DateTime.Now;
                DBContext.PayList.Add(pl);
                PayList pl2 = new PayList();
                pl2.UID = recu.ID;
                pl2.InOut = (int)enPayInOutType.In;
                pl2.PayType = (int)enPayListType.SGQ;
                pl2.FromTo = (int)enPayFrom.Exchange;
                pl2.Val = money;
                pl2.Remark = "收到" + u.Name + "￥:" + money + "首购券";
                pl2.Status = 1;
                pl2.CreateTime = pl2.UpdateTime = DateTime.Now;
                DBContext.PayList.Add(pl2);
                DBContext.Logs.Add(log);
                return DBContext.SaveChanges();
            }
        }

        /// <summary>
        /// 转让易物券
        /// </summary>
        /// <returns></returns>
        public int ExchangeJF(Guid UID, string recPhone, string recName, decimal money)
        {
            using (DBContext)
            {
                Users u = DBContext.Users.Where(m => m.ID == UID).FirstOrDefault();
                if (u == null)
                {
                    throw new Exception("用户不存在");
                }
                var payRecord = DBContext.PayRecord.Where(m => m.UID == UID).ToList();
                var payDetails = DBContext.PayListDetail.Where(m => m.UID == UID).ToList();
                decimal maintainMoney = 0;
                if (payRecord.Count > 0)
                {
                    maintainMoney = payRecord.Sum(m => m.MaxMoney);
                    if (payDetails.Count > 0)
                    {
                        maintainMoney -= payDetails.Sum(m => m.Price);
                    }
                }
                if (maintainMoney < money)
                {
                    throw new Exception("易物券不足");
                }
                Users recu = DBContext.Users.Where(m => m.Name == recPhone && m.TrueName == recName).FirstOrDefault();
                if (recu == null)
                {
                    throw new Exception("用户不存在");
                }
                decimal? _transtotal = money, _transigle = 0;
                foreach (PayRecord temp in payRecord.OrderByDescending(m => m.CreateTime).Where(m => m.Status == (int)enStatus.Enabled))
                {
                    _transigle = payDetails.Where(m => m.FromPRID == temp.ID).Sum(m => (decimal?)m.Price);//是否已全部转出？
                    if (_transigle < temp.MaxMoney)
                    {
                        PayListDetail pd = new PayListDetail();
                        pd.ID = Guid.NewGuid();
                        pd.UID = temp.UID;
                        pd.Price = (_transtotal > temp.MaxMoney - _transigle.Value)
                            ? (temp.MaxMoney - _transigle.Value) : _transtotal.Value;
                        pd.Remark = "转出易物券：" + pd.Price;
                        pd.CreateTime = DateTime.Now;
                        pd.Level = 1;
                        pd.Type = 1;
                        pd.FromUID = temp.UID;
                        pd.FromPRID = temp.ID;
                        DBContext.PayListDetail.Add(pd);

                        if (pd.Price == temp.MaxMoney - _transigle.Value)
                            temp.Status = (int)enStatus.Unabled;//额度已释放完
                        _transtotal -= pd.Price;

                        if (_transtotal <= 0)
                        {
                            break;
                        }
                    }
                }
                decimal actMoney = money * 0.9M;//实际到账

                Recharge r = new Recharge();
                r.ID = Guid.NewGuid();
                r.FromUID = u.ID;
                r.ToID = recu.ID;
                r.RechType = (int)enRechType.SGQ;
                r.Val = -money;
                r.ComVal = 0;
                r.Status = 1;
                r.CreateTime = r.UpdateTime = DateTime.Now;
                DBContext.Recharge.Add(r);
                Recharge r2 = new Recharge();
                r2.ID = Guid.NewGuid();
                r2.FromUID = recu.ID;
                r2.ToID = u.ID;
                r2.RechType = (int)enRechType.SGQ;
                r2.Val = actMoney;
                r2.ComVal = 0;
                r2.Status = 1;
                r2.CreateTime = r2.UpdateTime = DateTime.Now;
                DBContext.Recharge.Add(r2);
                //InsertLog(DBContext, enLogType.User, "用户UID:" + UID + "给UID:" + recu.ID + "转让易物券：￥" + money + ",转出后剩余：" + u.Balance, u.ID);
                //InsertLog(DBContext, enLogType.User, "用户UID:" + UID + "收到UID:" + recu.ID + "易物券：￥" + money + ",转入后剩余：" + recu.Balance, recu.ID);
                PayList pl = new PayList();
                pl.UID = UID;
                pl.InOut = (int)enPayInOutType.Out;
                pl.PayType = (int)enPayListType.SGQ;
                pl.FromTo = (int)enPayFrom.Exchange;
                pl.Val = money;
                pl.Remark = "转给" + recu.Name + "￥:" + money + "易物券";
                pl.Status = 1;
                pl.CreateTime = pl.UpdateTime = DateTime.Now;
                DBContext.PayList.Add(pl);
                PayList pl2 = new PayList();
                pl2.UID = recu.ID;
                pl2.InOut = (int)enPayInOutType.In;
                pl2.PayType = (int)enPayListType.SGQ;
                pl2.FromTo = (int)enPayFrom.Exchange;
                pl2.Val = actMoney;
                pl2.Remark = "收到" + u.Name + "￥:" + actMoney + "易物券";
                pl2.Status = 1;
                pl2.CreateTime = pl2.UpdateTime = DateTime.Now;
                DBContext.PayList.Add(pl2);
                PayRecord prRec = new PayRecord();
                prRec.ID = Guid.NewGuid();
                prRec.UID = recu.ID;
                prRec.MaxMoney = actMoney;//转账手续费10%
                prRec.LocalMoney = actMoney / 300;
                prRec.Level = 0;
                prRec.IsBuyProduct = 0;
                prRec.Status = 1;
                prRec.CreateTime = DateTime.Now;
                DBContext.PayRecord.Add(prRec);
                return DBContext.SaveChanges();
            }
        }

        public int CLoginPwd(string phone, string pwd)
        {
            using (DBContext)
            {
                Users user = DBContext.Users.Where(m => m.Name == phone).FirstOrDefault();
                if (user == null)
                {
                    throw new Exception("账号不存在");
                }
                user.Psw = MD5(pwd);
                return DBContext.SaveChanges();
            }
        }

        /// <summary>
        /// 获得丝路积分
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public decimal GetSiLuJiFen(Guid UID)
        {
            using (DBContext)
            {
                decimal? sum = DBContext.PayListDetail.Where(m => m.UID == UID).Sum(m => (decimal?)m.Price);
                decimal? MaxMoney = DBContext.PayRecord.Where(m => m.UID == UID).Sum(m => (decimal?)m.MaxMoney);

                decimal result = (MaxMoney == null ? 0 : MaxMoney.Value) - (sum == null ? 0 : sum.Value);
                return result < 0 ? 0 : result;
            }
        }
        /// <summary>
        /// 获得丝路积分
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public decimal GetSiLuJiFen2(Guid UID, out decimal allJf)
        {
            using (DBContext)
            {
                decimal? sum = DBContext.PayListDetail.Where(m => m.UID == UID).Sum(m => (decimal?)m.Price);
                decimal? MaxMoney = DBContext.PayRecord.Where(m => m.UID == UID).Sum(m => (decimal?)m.MaxMoney);
                allJf = (MaxMoney == null ? 0 : MaxMoney.Value);
                return allJf - (sum == null ? 0 : sum.Value);
            }
        }
        /// <summary>
        /// 回馈积分总数
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="allJf"></param>
        /// <returns></returns>
        public decimal GetHuikui(Guid UID)
        {
            using (DBContext)
            {
                decimal? sum = DBContext.PayListDetail.Where(m => m.UID == UID && !m.Remark.Contains("转出易物券")).Sum(m => (decimal?)m.Price);
                return (sum == null ? 0 : sum.Value) * 0.3M;
            }
        }

        public PayRecord GetNewPayRecordByUID(Guid UID)
        {
            using (DBContext)
            {
                //return DBContext.PayRecord.Where(m => m.UID == UID).OrderByDescending(m => m.CreateTime).FirstOrDefault();
                return DBContext.PayRecord.Where(m => m.UID == UID).OrderByDescending(m => m.Level).FirstOrDefault();
            }
        }
        public int ResetLoginPwd(Guid UID)
        {
            using (DBContext)
            {
                Users u = DBContext.Users.Where(m => m.ID == UID).FirstOrDefault();
                if (u == null)
                {
                    throw new Exception("用户不存在");
                }
                u.Psw = MD5("123456");
                return DBContext.SaveChanges();
            }
        }
        /// <summary>
        /// 修改用户
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Name"></param>
        /// <param name="HeadImg1"></param>
        /// <param name="Sex"></param>
        /// <param name="AddScore"></param>
        /// <returns></returns>
        public int UpdateUser(Guid ID, string Name, string TrueName, string HeadImg1, int LV, int Sex
            , decimal? AddScore, decimal? AddBalance, decimal? AddYWQ, decimal? AddShoppingVoucher)
        {
            using (DBContext)
            {
                Users u = DBContext.Users.Where(m => m.ID == ID).FirstOrDefault();
                if (u == null)
                {
                    throw new Exception("用户不存在");
                }
                u.Name = Name;
                u.TrueName = TrueName;
                u.HeadImg1 = HeadImg1;
                u.Sex = Sex;
                u.LV = LV;
                if (AddScore != null)
                {
                    u.Score += AddScore.Value;
                    Logs scorelog = new Logs();//可提现金额
                    scorelog.LogType = (int)enLogType.Admin;
                    scorelog.Page = "/Admin/DoEditUser";
                    scorelog.Remark = "管理员给账户：" + u.Name + ",UID:" + u.ID + "添加可提现金额：￥" + AddScore + "新增后金额：" + u.Score;
                    scorelog.Ip = HttpContext.Current.Request.UserHostAddress;
                    scorelog.CreateTime = scorelog.UpdateTime = DateTime.Now;
                    DBContext.Logs.Add(scorelog);
                }
                if (AddBalance != null)
                {
                    u.Balance += AddBalance.Value;
                    Logs sgqlog = new Logs();//首购券
                    sgqlog.LogType = (int)enLogType.Admin;
                    sgqlog.Page = "/Admin/DoEditUser";
                    sgqlog.Remark = "管理员给账户：" + u.Name + ",UID:" + u.ID + "添加行票券：￥" + AddBalance + "新增后行票券：" + u.Balance;
                    sgqlog.Ip = HttpContext.Current.Request.UserHostAddress;
                    sgqlog.CreateTime = sgqlog.UpdateTime = DateTime.Now;
                    DBContext.Logs.Add(sgqlog);
                }
                if (AddYWQ != null)
                {
                    var q = from a in DBContext.PayRecord
                            where a.UID == u.ID && a.Status == (int)enStatus.Enabled
                            orderby a.CreateTime descending
                            select a;
                    var ywqModel = q.FirstOrDefault();
                    if (ywqModel == null)
                    {
                        throw new Exception("用户不存在正在释放中的易物券");
                    }

                    ywqModel.MaxMoney += AddYWQ.Value;

                    Logs sgqlog = new Logs();//首购券
                    sgqlog.LogType = (int)enLogType.Admin;
                    sgqlog.Page = "/Admin/DoEditUser";
                    sgqlog.Remark = "管理员给账户：" + u.Name + ",UID:" + u.ID + "添加易物券：￥" + AddYWQ;
                    sgqlog.Ip = HttpContext.Current.Request.UserHostAddress;
                    sgqlog.CreateTime = sgqlog.UpdateTime = DateTime.Now;
                    DBContext.Logs.Add(sgqlog);
                }
                if (AddShoppingVoucher != null)
                {
                    u.ShoppingVoucher += AddShoppingVoucher.Value;
                    Logs sgqlog = new Logs();//首购券
                    sgqlog.LogType = (int)enLogType.Admin;
                    sgqlog.Page = "/Admin/DoEditUser";
                    sgqlog.Remark = "管理员给账户：" + u.Name + ",UID:" + u.ID + "添加行票消费券：￥" + AddShoppingVoucher + "新增后行票消费券：" + u.ShoppingVoucher;
                    sgqlog.Ip = HttpContext.Current.Request.UserHostAddress;
                    sgqlog.CreateTime = sgqlog.UpdateTime = DateTime.Now;
                    DBContext.Logs.Add(sgqlog);
                }

                u.UpdateTime = DateTime.Now;
                return DBContext.SaveChanges();
            }
        }

        //每天定时任务
        public void TimedTaskRun(DateTime? nowdate = null)
        {
            try
            {
                if (nowdate == null)
                {
                    nowdate = DateTime.Now;
                }
                using (DBContext)
                {

                    List<Users> alluser = DBContext.Users.Where(m => m.Status == (int)enStatus.Enabled).ToList();
                    for (int i = 0; i < alluser.Count; i++)
                    {
                        TimedTask(DBContext, alluser[i], nowdate);
                    }
                    DBContext.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        public void CommunityRewardsTask(DateTime? nowdate = null)
        {
            try
            {
                if (nowdate == null)
                {
                    nowdate = DateTime.Now;
                }
                using (DBContext)
                {
                    //只计算未锁定的用户
                    List<Users> alluser = DBContext.Users.Where(m => m.Status == (int)enStatus.Enabled).ToList();
                    for (int i = 0; i < alluser.Count; i++)
                    {
                        CommunityRewards(DBContext, alluser[i], nowdate.Value);
                    }
                    DBContext.SaveChanges();
                }
            }
            catch (Exception)
            {

                throw;
            }

        }
        /// <summary>
        /// 小区奖励
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="user"></param>
        public void CommunityRewards(RelexBarEntities entity, Users user, DateTime now)
        {
            //    List<Guid> PRList = new List<Guid>();
            //    PayRecord pr = entity.PayRecord.Where(m => m.UID == user.ID && m.Status == (int)enStatus.Enabled).OrderBy(m => m.CreateTime).FirstOrDefault();
            //    if (pr == null)
            //    {
            //        return;
            //    }
            //    decimal jlbl = 0.06M;

            //    if (entity.Users.Count(m => m.FID == user.ID) < 2)//直推会员数至少要两个
            //    {
            //        return;
            //    }
            //    List<Users> childs = entity.Users.Where(m => m.CommendID == user.ID).OrderBy(m => m.CreateTime).ToList();//获得一级用户
            //    if (childs.Count == 0)
            //    {
            //        return;
            //    }
            //    //if (user.Name == "15602408988")
            //    //{
            //    //    ;
            //    //}
            //    Guid? leftUID = null;
            //    Guid? rightUID = null;
            //    //今日左右两边总额
            //    decimal leftSum = 0;
            //    decimal rightSum = 0;
            //    if (childs.Count >= 1)
            //    {
            //        leftUID = childs[0].ID;
            //        leftSum = GetTuanDuiSumPrice(entity, childs[0].ID);
            //    }
            //    if (childs.Count >= 2)
            //    {
            //        rightUID = childs[1].ID;
            //        rightSum = GetTuanDuiSumPrice(entity, childs[1].ID);
            //    }
            //    ///////////查询(最近一次)昨日两边总额
            //    decimal? yleftSum = entity.RecordsOfConsumption.Where(m => m.UID == user.ID && m.ChildUID == leftUID).OrderByDescending(m => m.CreateTime).Select(m => (decimal?)m.SumPrice).FirstOrDefault();
            //    decimal? yrightSum = rightUID == Guid.Empty ? 0 : entity.RecordsOfConsumption.Where(m => m.UID == user.ID && m.ChildUID == rightUID).OrderByDescending(m => m.CreateTime).Select(m => (decimal?)m.SumPrice).FirstOrDefault();

            //    RecordsOfConsumption roc = new RecordsOfConsumption();
            //    roc.ID = Guid.NewGuid();
            //    roc.UID = user.ID;
            //    roc.ChildUID = leftUID.Value;
            //    roc.SumPrice = leftSum;
            //    roc.CreateTime = roc.UpdateTime = now;
            //    roc.Status = 0;
            //    entity.RecordsOfConsumption.Add(roc);
            //    if (rightUID == null)
            //    {
            //        return;
            //    }
            //    RecordsOfConsumption roc1 = new RecordsOfConsumption();
            //    roc1.ID = Guid.NewGuid();
            //    roc1.UID = user.ID;
            //    roc1.ChildUID = rightUID.Value;
            //    roc1.SumPrice = rightSum;
            //    roc1.CreateTime = roc1.UpdateTime = now;
            //    roc1.Status = 0;
            //    entity.RecordsOfConsumption.Add(roc1);

            //    //如果昨天sum未空（第一次初始化状态）//计算收益
            //    if (yleftSum == null && yrightSum == null)
            //    {
            //        //////////计算收益begin/////////////
            //        if (leftSum - rightSum != 0)//左右不相等
            //        {
            //            //decimal ce = Math.Abs(leftSum - rightSum);
            //            decimal ce = leftSum < rightSum ? leftSum : rightSum;//以小的区域计算释放才正确
            //            if (ce == 0)//如果为0，则不计算
            //                return;
            //            decimal jl = ce * jlbl;
            //            decimal realJL = 0;
            //            RealShouYi(entity, user, jl, out realJL, "左区" + leftSum + "，右区" + rightSum + "，加速：", "", now, enPayListType.CommunityRewards);
            //            user.Score += realJL;
            //            roc.Status = 1;
            //            roc1.Status = 1;
            //        }
            //        ///////////计算收益end////////////////////
            //        return;
            //    }
            //    else if (yleftSum == null || yrightSum == null)
            //    {//其中一个收益为null
            //        return;
            //    }

            //    decimal yLeftSum = yleftSum == null ? 0 : yleftSum.Value;
            //    decimal yRightSum = yrightSum == null ? 0 : yrightSum.Value;
            //    decimal realLeft = leftSum - yLeftSum;//今日实际左业绩
            //    decimal realRight = rightSum - yRightSum;//今日实际右业绩

            //    if (realLeft == 0 && realRight == 0)//如果左右都没有变化，则不计算大小区奖
            //    {
            //        roc.Status = 1;
            //        roc1.Status = 1;
            //        return;
            //    }
            //    //昨日大小区奖比对后剩余
            //    decimal maintainY = Math.Abs(yLeftSum - yRightSum);
            //    if (yLeftSum > yRightSum)
            //    {
            //        realLeft += maintainY;
            //    }
            //    else
            //    {
            //        realRight += maintainY;
            //    }
            //    if (realLeft - realRight == 0)//左右区相等，则不变
            //    {
            //        return;
            //    }

            //    decimal Chae = realLeft < realRight ? realLeft : realRight;//以小的区域计算释放才正确
            //    decimal Jl = Chae * jlbl;
            //    decimal realJL1 = 0;
            //    RealShouYi(entity, user, Jl, out realJL1, "左区" + realLeft + "，右区" + realRight + "，加速：", "", now, enPayListType.CommunityRewards);
            //    user.Score += realJL1 * (1 - GWQBL);
            //    user.TotalScore += realJL1 * (1 - GWQBL);
            //    user.ShoppingVoucher += realJL1 * GWQBL;//购物券
            //    InsertShouRuPayList(entity, user.ID, now, realJL1);

            //    entity.Database.ExecuteSqlCommand("update RecordsOfConsumption set Status=1 where UID='" + user.ID + "' and childUID='" + leftUID + "'");
            //    entity.Database.ExecuteSqlCommand("update RecordsOfConsumption set Status=1 where UID='" + user.ID + "' and childUID='" + rightUID + "'");
        }
        /// <summary>
        /// 计算收益使用，可计算能得到的真实收益
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="user"></param>
        /// <param name="shouyi"></param>
        /// <param name="realshouyi"></param>
        public void RealShouYi(RelexBarEntities entity, Users user, decimal shouyi, out decimal realshouyi, string remarkbegin, string remarkend, DateTime now, enPayListType type)
        {
            realshouyi = 0;
            List<Guid> PRList = new List<Guid>();
            bool flag = false;
            do
            {
                PayRecord pr = entity.PayRecord.Where(m => m.UID == user.ID && m.Status == (int)enStatus.Enabled && !PRList.Contains(m.ID)).OrderBy(m => m.CreateTime).FirstOrDefault();
                if (pr == null)
                {
                    return;
                }
                PRList.Add(pr.ID);
                decimal? tylje = entity.PayListDetail.Where(m => m.UID == user.ID && m.FromPRID == pr.ID).Sum(m => (decimal?)m.Price);//当前pr已领金额
                decimal ylje = tylje == null ? 0 : tylje.Value;
                decimal this_sy = 0;
                if (pr.MaxMoney - shouyi >= ylje)
                {
                    this_sy = shouyi;
                    realshouyi += shouyi;
                    flag = false;

                }
                else
                {
                    this_sy = pr.MaxMoney - ylje;
                    realshouyi += this_sy;
                    shouyi -= this_sy;
                    flag = true;
                    pr.Status = 0;
                }
                if (this_sy != 0)
                {
                    PayListDetail pl = new PayListDetail();
                    pl.ID = Guid.NewGuid();
                    pl.UID = user.ID;
                    pl.Price = this_sy;
                    pl.Remark = remarkbegin + this_sy + remarkend;
                    pl.CreateTime = now;
                    pl.Type = (int)type;
                    pl.FromPRID = pr.ID;
                    entity.PayListDetail.Add(pl);
                }
            } while (flag);
        }
        public void InsertShouRuPayList(RelexBarEntities entity, Guid UID, DateTime time, decimal shouru)
        {
            DateTime date = DateTime.Parse(time.ToString("yyyy-MM-dd"));//当前时间
            DateTime end = date.AddDays(1);
            PayList pl = entity.PayList.Where(m => m.CreateTime >= date && m.CreateTime < end && m.UID == UID && m.InOut == (int)Common.enPayInOutType.In && m.FromTo == (int)Common.enPayFrom.ShouRu)
                .OrderByDescending(m => m.CreateTime).FirstOrDefault();
            if (pl == null)
            {
                PayList payList = new PayList();
                payList.UID = UID;
                payList.InOut = (int)Common.enPayInOutType.In;
                payList.PayType = (int)Common.enPayType.Point;
                payList.FromTo = (int)Common.enPayFrom.ShouRu;
                payList.Val = shouru;
                payList.Remark = "收入";
                payList.Status = 1;
                payList.CreateTime = payList.UpdateTime = time;
                entity.PayList.Add(payList);
            }
            else
            {
                pl.Val += shouru;
                pl.UpdateTime = time;
            }
        }
        public void TimedTask(RelexBarEntities entity, Users user, DateTime? nowdate = null)
        {
            try
            {
                if (nowdate == null)
                {
                    nowdate = DateTime.Now;
                }

                Guid UID = user.ID;
                ////////////////////////////////////////////每日固定工资begin///////////////////////////////////////
                //获取当前用户的产品
                List<PayRecord> payRecord = entity.PayRecord.Where(m => m.UID == user.ID && m.Status == (int)enStatus.Enabled).ToList();
                if (payRecord.Count() == 0)
                {
                    return;
                }
                decimal? total;
                decimal totalPrice = 0;
                int pdCount = 0;
                foreach (PayRecord temp in payRecord)
                {
                    pdCount = entity.PayListDetail.Count(m => m.FromPRID == temp.ID && m.Remark.Contains("每日返易物券"));//总共返了多少天？
                    total = entity.PayListDetail.Where(m => m.FromPRID == temp.ID).Sum(m => (decimal?)m.Price);
                    if (!total.HasValue)
                        total = 0;
                    if (total < temp.MaxMoney)
                    {
                        PayListDetail pd = new PayListDetail();
                        pd.ID = Guid.NewGuid();
                        pd.UID = temp.UID;
                        //pd.Price = (temp.LocalMoney > temp.MaxMoney - total.Value) ? (temp.MaxMoney - total.Value) : temp.LocalMoney;
                        decimal everyDayMoney = (temp.MaxMoney - total.Value) / (300 - pdCount);
                        everyDayMoney = everyDayMoney > 0.01M ? everyDayMoney : 0.01M;//最小0.01元
                        pd.Price = (everyDayMoney > temp.MaxMoney - total.Value) ? (temp.MaxMoney - total.Value) : everyDayMoney;//按照剩余金额除以剩余天数来结算每日
                        pd.Remark = "每日返易物券：" + pd.Price.ToString("0.##");
                        pd.CreateTime = DateTime.Now;
                        pd.Level = 1;
                        pd.Type = 1;
                        pd.FromUID = temp.UID;
                        pd.FromPRID = temp.ID;
                        entity.PayListDetail.Add(pd);

                        if (pd.Price == temp.MaxMoney - total.Value)
                        {
                            temp.Status = (int)enStatus.Unabled;
                        }
                        totalPrice += pd.Price;
                    }
                }
                if (totalPrice > 0)
                {
                    PayList pl = new PayList();
                    pl.UID = UID;
                    pl.InOut = (int)enPayInOutType.In;
                    pl.PayType = (int)enPayType.Coin;
                    pl.FromTo = (int)enPayFrom.ShouRu;
                    pl.Val = totalPrice * 0.5M;
                    pl.Remark = "每日返易物券：" + pl.Val.ToString("0.##");//3成扣出来成为回馈券
                    pl.Status = (int)enStatus.Enabled;
                    pl.CreateTime = pl.UpdateTime = DateTime.Now;
                    pl.PriceType = (int)enPriceType.Balance;
                    entity.PayList.Add(pl);

                    PayList pl2 = new PayList();
                    pl2.UID = UID;
                    pl2.InOut = (int)enPayInOutType.In;
                    pl2.PayType = (int)enPayType.Point;
                    pl2.FromTo = (int)enPayFrom.ShouRu;
                    pl2.Val = totalPrice * 0.1M;
                    pl2.Remark = "每日返消费券：" + pl2.Val.ToString("0.##");//1成扣出来成为消费券
                    pl2.Status = (int)enStatus.Enabled;
                    pl2.CreateTime = pl2.UpdateTime = DateTime.Now;
                    pl2.PriceType = (int)enPriceType.ShoppingVoucher;
                    entity.PayList.Add(pl2);

                    user.Balance += totalPrice * 0.5M;//50%进入行票
                    user.ShoppingVoucher += totalPrice * 0.1M;//10% 去到消费券
                    /*
                        1，关于易物劵释放：易物劵除以300天每日释放出来，
                            其中50%到“今日行票”，30%到“回馈易物劵”、
                            10%到“行票消费劵”、10%手续费直接扣除。
                            例如：30000易物劵，按300天计，每天释放出来100，
                            其中50进到“今日行票”，30进入“回馈易物劵”，
                            10进入“行票消费劵”，10手续费直接扣除不用显示。
                    */
                }
                //user.Balance += totalPrice * 0.5M;
                //user.ShoppingVoucher += totalPrice * 0.1M;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取我的下家用户
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public List<Models.UserLevel> GetNextUsers(Guid UID)
        {
            using (DBContext)
            {
                var q = DBContext.Users.Where(m => m.FID == UID).Join(DBContext.ProductList, u => u.LV, p => p.Level, (u, p) => new Models.UserLevel
                {
                    ID = u.ID,
                    Name = u.Name,
                    HeadImg = u.HeadImg1,
                    LevelName = p.Name,
                    Level = u.LV,
                    TrueName = u.TrueName,
                    CommendID = u.CommendID,
                    CreateTime = u.CreateTime,
                });
                return q.ToList();
            }
        }
        /// <summary>
        /// 获取我的下家用户-递归
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public List<Models.UserLevel> GetNextUsersRecursion(Guid UID, int deep)
        {
            if (deep <= 0)
                return new List<Models.UserLevel>();
            deep--;

            List<Models.UserLevel> list;
            using (DBContext)
            {
                var q = from a in DBContext.Users
                        where a.FID == UID
                        orderby a.CreateTime descending
                        select new Models.UserLevel
                        {
                            ID = a.ID,
                            Name = a.Name,
                            HeadImg = a.HeadImg1,
                            LevelName = "",
                            Level = a.LV,
                            Fid = a.FID,
                            TrueName = a.TrueName,
                            CommendID = a.CommendID,
                            CreateTime = a.CreateTime,
                        };

                list = q.ToList();//获得我的下家
                List<Models.UserLevel> nextlist = new List<Models.UserLevel>();
                for (int i = 0; i < list.Count; i++)
                {
                    nextlist.AddRange(GetNextUsersRecursion(list[i].ID, deep));
                }
                list.AddRange(nextlist);
            }
            return list;
        }
        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="oldpsw"></param>
        /// <param name="newpsw"></param>
        /// <returns></returns>
        public int ChangeLoginPsw(Guid UID, string oldpsw, string newpsw)
        {
            using (DBContext)
            {
                oldpsw = MD5(oldpsw);
                var q = DBContext.Users.FirstOrDefault(m => m.ID == UID && m.Psw == oldpsw);
                if (q != null)
                {
                    q.Psw = MD5(newpsw);
                    q.UpdateTime = DateTime.Now;
                    var result = DBContext.SaveChanges();
                    logBll.InsertLog(string.Format("用户：{0},修改登录密码成功", q.Name), enLogType.User);
                    return result;
                }
                return 0;
            }
        }

        public void InsertLog(RelexBarEntities entity, enLogType type, string remark, Guid? UID)
        {
            Logs log = new Logs();
            log.LogType = (int)type;
            log.Page = HttpContext.Current.Request.Path;
            log.Remark = remark;
            log.Ip = HttpContext.Current.Request.UserHostAddress;
            log.UID = UID;
            log.CreateTime = log.UpdateTime = DateTime.Now;
            entity.Logs.Add(log);

        }
        /// <summary>
        /// 计算团队奖励，每次开户时(CreateAccount)调用 //递归方法
        /// <param name="UID">开户人，上家</param>
        /// <param name="nextyeji">下一级的团队金额 ，第一次调用给0</param>
        /// <param name="xsje">销售金额(当前产品卖价)</param>
        /// </summary>
        public void TuanDuiJiangLi(RelexBarEntities entity, Guid UID, decimal nextyeji, decimal xsje)
        {
            Users user = entity.Users.Where(m => m.ID == UID).FirstOrDefault();
            //获得我的上家
            Users u = entity.Users.Where(m => m.ID == user.FID).FirstOrDefault();
            if (user == null)
            {
                throw new Exception("用户不存在，开户失败");
            }
            decimal myTuanDuiPrice = GetTuanDuiSumPrice(entity, UID);//团队金额
            int t = (int)(myTuanDuiPrice / 1e6M);
            if (t == (int)nextyeji / 1e6M || t == 0)
            {//平级不奖励跳过,不到100w跳过
                if (u != null)
                {
                    TuanDuiJiangLi(entity, u.ID, myTuanDuiPrice, xsje);//
                }
                return;
            }
            //取我的下级个数
            int nextCount = entity.Users.Where(m => m.FID == UID).Count();
            if (nextCount < 3)
            {
                if (u != null)
                {
                    TuanDuiJiangLi(entity, u.ID, myTuanDuiPrice, xsje);
                }
                return;//此用户下级数量不够
            }
            decimal jl = 0;
            if (t > 10)
            {
                jl = xsje * 0.1M;
            }
            else
            {
                jl = xsje * t / 100;
            }

            user.Score += jl;
            user.TotalScore += jl;
            user.UpdateTime = DateTime.Now;
            PayListDetail pd = new PayListDetail();
            pd.ID = Guid.NewGuid();
            pd.Price = jl;
            pd.Remark = "部门业绩奖，提成比例：" + t + "%";
            pd.UID = UID;
            pd.Type = (int)Common.enPayListType.TuanDui;
            pd.CreateTime = DateTime.Now;
            DBContext.PayListDetail.Add(pd);
            DateTime date = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));//当前时间
            DateTime end = date.AddDays(1);
            PayList p = entity.PayList.Where(m => m.CreateTime >= date && m.CreateTime < end && m.UID == UID && m.InOut == (int)Common.enPayInOutType.In && m.FromTo == (int)Common.enPayFrom.ShouRu).OrderByDescending(m => m.CreateTime).FirstOrDefault();
            if (p == null)
            {
                PayList payList = new PayList();
                payList.UID = UID;
                payList.InOut = (int)Common.enPayInOutType.In;
                payList.PayType = (int)Common.enPayType.Point;
                payList.FromTo = (int)Common.enPayFrom.ShouRu;
                payList.Val = jl;
                payList.Remark = "收入";
                payList.Status = 1;
                payList.CreateTime = DateTime.Now;
                payList.UpdateTime = DateTime.Now;
                entity.PayList.Add(payList);
            }
            else
            {
                p.Val += jl;
                p.UpdateTime = DateTime.Now;
            }
            if (u != null)
            {
                TuanDuiJiangLi(entity, u.ID, myTuanDuiPrice, xsje);
            }
        }
        /// <summary>
        /// 获得团队所有消费
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public decimal GetTuanDuiSumPrice(Guid UID)
        {
            return GetMySumPrice(UID) + GetNextSumPrice(UID);
        }
        public decimal GetTuanDuiSumPrice(RelexBarEntities entity, Guid UID)
        {
            return GetMySumPrice(entity, UID) + GetNextSumPrice(entity, UID);
        }
        /// <summary>
        /// 获取我的消费金额
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public decimal GetMySumPrice(Guid UID)
        {
            using (DBContext)
            {
                return DBContext.Database.SqlQuery<decimal>("select ISNULL(SUM(p.price),0) from PayRecord r left join ProductList p on r.Level=p.Level where UID=@uid", new SqlParameter[] { new SqlParameter("uid", UID) }).FirstOrDefault();
            }
        }
        public decimal GetMySumPrice(RelexBarEntities entity, Guid UID)
        {
            return entity.Database.SqlQuery<decimal>("select ISNULL(SUM(p.price),0) from PayRecord r left join ProductList p on r.Level=p.Level where UID=@uid", new SqlParameter[] { new SqlParameter("uid", UID) }).FirstOrDefault();

        }
        /// <summary>
        /// 获取除开我以外所有下家消费金额
        /// </summary>
        /// <returns></returns>
        public decimal GetNextSumPrice(Guid UID)
        {
            decimal price = 0;
            using (DBContext)
            {
                //获取下家数量
                List<Guid> nextuid = DBContext.Users.Where(m => m.FID == UID).Select(m => m.ID).ToList();
                if (nextuid.Count == 0)
                {
                    return 0;
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("select ISNULL(SUM(pl.Price),0) from PayRecord pr left join ProductList pl on pr.Level=pl.Level");
                    sb.Append(" where pr.UID in(select ID from Users u where u.CommendID=@uid)");
                    price += DBContext.Database.SqlQuery<decimal>(sb.ToString(), new SqlParameter[] { new SqlParameter("uid", UID) }).FirstOrDefault();
                    for (int i = 0; i < nextuid.Count; i++)
                    {
                        price += GetNextSumPrice(nextuid[i]);
                    }
                    return price;
                }
            }
        }
        public decimal GetNextSumPrice(RelexBarEntities entity, Guid UID)
        {
            decimal price = 0;

            //获取下家数量
            List<Guid> nextuid = entity.Users.Where(m => m.CommendID == UID).Select(m => m.ID).ToList();
            if (nextuid.Count == 0)
            {
                return 0;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select ISNULL(SUM(pl.Price),0) from PayRecord pr left join ProductList pl on pr.Level=pl.Level");
                sb.Append(" where pr.UID in(select ID from Users u where u.CommendID=@uid)");
                price += entity.Database.SqlQuery<decimal>(sb.ToString(), new SqlParameter[] { new SqlParameter("uid", UID) }).FirstOrDefault();
                for (int i = 0; i < nextuid.Count; i++)
                {
                    price += GetNextSumPrice(entity, nextuid[i]);
                }
                return price;
            }

        }

        /// <summary>
        /// 通过名称获得用户，插入的时候保证在数据库是唯一的
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Users GetUserByName(string name)
        {
            using (DBContext)
            {
                return DBContext.Users.Where(m => m.Name == name).FirstOrDefault();
            }
        }
        #region mvc新增方法
        /// <summary>
        /// 通过微信登陆注册
        /// </summary>
        /// <param name="Unionid"></param>
        /// <param name="Openid"></param>
        /// <param name="Nickname"></param>
        /// <param name="Headimgurl"></param>
        /// <returns></returns>
        //public int InsertWxUser(string Unionid, string Openid, string Nickname, string Headimgurl)
        //{
        //    using (DBContext)
        //    {
        //        if (DBContext.Users.FirstOrDefault(m => (!string.IsNullOrEmpty(Unionid) && m.WX_UnionID == Unionid)) != null)
        //            return (int)ErrorCode.账号已被注册;

        //        Users model = new Users()
        //        {
        //            Name = string.IsNullOrEmpty(Nickname) ? "_" + Guid.NewGuid().ToString().Substring(0, 8) : Nickname,
        //            Psw = "",
        //            UserType = (int)enUserType.User,
        //            Phone = "",
        //            FID = Guid.Empty,
        //            HeadImg1 = Headimgurl,
        //            //以下信息自动生成
        //            ID = Guid.NewGuid(),
        //            Status = (int)enStatus.Enabled,
        //            CreateTime = DateTime.Now,
        //            UpdateTime = DateTime.Now,
        //            TrueName = "wx_" + new Random().Next(100000, 999999),
        //            WX_UnionID = Unionid,
        //            WX_OpenID = Openid,
        //            WxName = Nickname,
        //            PayPsw = "",
        //        };
        //        DBContext.Users.Add(model);

        //        try
        //        {
        //            int result = DBContext.SaveChanges();
        //            if (result > 0)
        //            {
        //                DBContext.SaveChanges();

        //                logBll.InsertLog(string.Format("注册用户成功：微信注册,openid:{0}", Unionid), enLogType.User);
        //            }
        //            return result;
        //        }
        //        catch (Exception ex)
        //        {
        //            logBll.InsertLog(string.Format("注册用户失败：微信注册,openid:{0}", Unionid, ex), enLogType.User);
        //            return 0;
        //        }
        //    }
        //}
        /// <summary>
        /// 查询用户
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sum"></param>
        /// <returns></returns>
        public List<Models.AdminUsers> GetUsersSearch(string key, int pageSize, int pageIndex, out int sum)
        {
            using (DBContext)
            {
                //var q = DBContext.Users.Where(m => m.ID != Guid.Empty).Join(DBContext.Users, U => U.FID, F => F.ID, (U, F) => new Models.AdminUsers() {
                //    ID = U.ID,
                //    Name = U.Name,
                //    CardNumber = U.CardNumber,
                //    TrueName=U.TrueName,
                //    Sex=U.Sex,
                //    LV=U.LV,
                //    Score=U.Score,
                //    TotalScore=U.TotalScore,
                //    Balance=U.Balance,
                //    UserType=U.UserType,
                //    FID=U.FID,
                //    RealCheck=U.RealCheck,
                //    C_index=U.C_index,
                //    Status=U.Status,
                //    CreateTime=U.CreateTime,
                //    UpdateTime=U.UpdateTime,
                //    ShoppingVoucher=U.ShoppingVoucher,
                //    PrePhone=F.Name,
                //    PreName=F.TrueName

                //});
                sum = DBContext.Database.SqlQuery<int>("select COUNT(ID) from Users").FirstOrDefault();
                string sql = "select * from(select ROW_NUMBER() over(order by u.createtime) rownumber,"
+ "u.*, f.Name PrePhone, f.TrueName PreName from Users u left join Users f on u.FID = f.ID ";
                if (!string.IsNullOrEmpty(key))
                {
                    sql += "where u.Name like @key";
                    // q = q.Where(m => m.Name.Contains(key));
                }
                else
                {
                    sql += "where (1=1 or u.Name like @key)";
                }
                sql += ") as temp where temp.rownumber >" + (pageIndex - 1) * pageSize + " and temp.rownumber<=" + pageIndex * pageSize;
                return DBContext.Database.SqlQuery<Models.AdminUsers>(sql, new SqlParameter[] {
                    new SqlParameter("@key","%"+key+"%")
                }).ToList();
            }
        }
        //public List<Users> GetUsersSearch(string key, int pageSize, int pageIndex, out int sum)
        //{
        //    using (DBContext)
        //    {
        //        var q = DBContext.Users.Where(m => m.ID != Guid.Empty);
        //        if (!string.IsNullOrEmpty(key))
        //        {
        //            q = q.Where(m => m.Name.Contains(key));
        //        }
        //        return GetPagedList(q.OrderByDescending(m => m.CreateTime), pageSize, pageIndex, out sum);
        //    }
        //}
        /// <summary>
        /// 修改用户性别
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="sex"></param>
        /// <returns></returns>
        public int ChangeSex(Guid UID, int sex)
        {
            using (DBContext)
            {
                Users u = DBContext.Users.Where(m => m.ID == UID).FirstOrDefault();
                if (u != null)
                {
                    u.Sex = sex;
                    u.UpdateTime = DateTime.Now;
                }
                return DBContext.SaveChanges();
            }
        }
        /// <summary>
        /// 修改用户头像
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="headimg"></param>
        /// <returns></returns>
        public int UpdateHead(Guid UID, string headimg)
        {
            using (DBContext)
            {
                Users u = DBContext.Users.Where(m => m.ID == UID).FirstOrDefault();
                if (u == null) { return -1; }
                u.HeadImg1 = headimg;
                return DBContext.SaveChanges();
            }
        }

        public int GetTotalYSCount()
        {
            using (DBContext)
            {
                return DBContext.Users.Count(m => m.LV == (int)enUserLV.牙商);
            }
        }
        public int GetTotalUserCount()
        {
            using (DBContext)
            {
                return DBContext.Users.Count();
            }
        }

        /// <summary>
        /// 更改用户状态
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int ChangeUserStatus(Guid uid, enStatus status)
        {
            using (DBContext)
            {
                Users user = DBContext.Users.Where(m => m.ID == uid).FirstOrDefault();
                if (user != null)
                {
                    user.Status = (int)status;
                    user.UpdateTime = DateTime.Now;
                }
                return DBContext.SaveChanges();
            }
        }
        #endregion
        public Users GetUser(string name, string psw)
        {
            using (DBContext)
            {
                var q = DBContext.Users.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    q = q.Where(m => m.Name == name);
                }
                if (!string.IsNullOrWhiteSpace(psw))
                {
                    psw = MD5(psw);
                    q = q.Where(m => m.Psw == psw);
                }

                return q.FirstOrDefault();
            }
        }
        public Users GetUser(string name)
        {
            return GetUser(name, string.Empty);
        }

        public Users GetUserById(Guid id)
        {
            using (DBContext)
            {
                return DBContext.Users.FirstOrDefault(m => m.ID == id);
            }
        }
        public Users GetUserById(string id)
        {
            return GetUserById(Guid.Parse(id));
        }
        public Users GetUserByPhone(string phone)
        {
            using (DBContext)
            {
                return DBContext.Users.FirstOrDefault(m => m.Phone == phone);
            }
        }

        public List<Users> GetUsersByFId(Guid fid)
        {
            using (DBContext)
            {
                return DBContext.Users.Where(m => m.FID == fid && m.UserType == (int)enUserType.User && m.Status == (int)enStatus.Enabled).ToList();
            }
        }
        public List<Users> GetUsersByFId(string fid)
        {
            return GetUsersByFId(Guid.Parse(fid));
        }

        public int GetUsersCountByFId(Guid fid)
        {
            using (DBContext)
            {
                return DBContext.Users.Count(m => m.FID == fid && m.UserType == (int)enUserType.User);
            }
        }

        public List<Users> GetShopsList()
        {
            using (DBContext)
            {
                return DBContext.Users.Where(m => m.UserType == (int)enUserType.Shop && m.Status == (int)enStatus.Enabled).ToList();
            }
        }
        public List<Users> GetShopsList(enShopType shoptype)
        {
            using (DBContext)
            {
                return DBContext.Users.Where(m => m.UserType == (int)enUserType.Shop && m.LV == (int)shoptype && m.Status == (int)enStatus.Enabled).ToList();
            }
        }

        public Users GetShopsByCardID(string cardid)
        {
            using (DBContext)
            {
                return DBContext.Users.FirstOrDefault(m => m.CardNumber == cardid && m.Status == (int)enStatus.Enabled);
            }
        }

        public Users GetShopsDetails(Guid SID)
        {
            using (DBContext)
            {
                return DBContext.Users.FirstOrDefault(m => m.UserType == (int)enUserType.Shop && m.ID == SID && m.Status == (int)enStatus.Enabled);
            }
        }

        public List<Users> GetUsersSearch(string name, string truename, string phone, string wxname, string cardnumber)
        {
            using (DBContext)
            {
                var q = DBContext.Users.Where(m => m.UserType == (int)enUserType.User);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    q = q.Where(m => m.Name == name);
                }
                if (!string.IsNullOrWhiteSpace(truename))
                {
                    q = q.Where(m => m.TrueName == truename);
                }
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    q = q.Where(m => m.Phone == phone);
                }
                if (!string.IsNullOrWhiteSpace(wxname))
                {
                    q = q.Where(m => m.WxName == wxname);
                }
                if (!string.IsNullOrWhiteSpace(cardnumber))
                {
                    q = q.Where(m => m.CardNumber == cardnumber);
                }

                return q.ToList();
            }
        }

        public dynamic GetUsersSearch(string key, enUserType? usertype, int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                var q = from u1 in DBContext.Users
                        join u2 in DBContext.Users on u1.FID equals u2.ID into temp
                        from tt in temp.DefaultIfEmpty()
                        select new
                        {
                            ID = u1.ID,
                            Name = u1.Name,
                            CardNumber = u1.CardNumber,
                            TrueName = u1.TrueName,
                            WxName = u1.WxName,
                            Phone = u1.Phone,
                            CredID = u1.CredID,
                            CredImg1 = u1.CredImg1,
                            CredImg2 = u1.CredImg2,
                            LV = u1.LV,
                            Score = u1.Score,
                            TotalScore = u1.TotalScore,
                            Balance = u1.Balance,
                            UserType = u1.UserType,
                            AreaID = u1.AreaID,
                            RealCheck = u1.RealCheck,
                            Status = u1.Status,
                            Address = u1.Address,
                            Descrition = u1.Descrition,
                            HeadImg1 = u1.HeadImg1,
                            CreateTime = u1.CreateTime,
                            UpdateTime = u1.UpdateTime,
                            FID = u1.FID,
                            FuserName = tt != null ? (tt.Phone + "[" + tt.TrueName + "]") : "",
                            FuserPhone = tt != null ? tt.Phone : "",
                        };

                if (!string.IsNullOrWhiteSpace(key))
                {
                    q = q.Where(m => m.Name == key || m.TrueName.Contains(key) || m.Phone == key || m.CardNumber == key);
                }
                if (usertype.HasValue)
                {
                    q = q.Where(m => m.UserType == (int)usertype.Value);
                }

                return GetPagedList(q.OrderByDescending(m => m.CreateTime), pagesize, pageinex, out count);
            }
        }

        /// <summary>
        /// 获取实名制申请列表
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <param name="pagesize"></param>
        /// <param name="pageinex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public dynamic GetRealChecklist(Guid id, string key, int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                var q = from u1 in DBContext.Users
                        select new
                        {
                            ID = u1.ID,
                            Name = u1.Name,
                            CardNumber = u1.CardNumber,
                            TrueName = u1.TrueName,
                            Phone = u1.Phone,
                            CredID = u1.CredID,
                            CredImg1 = string.IsNullOrEmpty(u1.CredImg1) ? "" : u1.CredImg1,
                            CredImg2 = string.IsNullOrEmpty(u1.CredImg2) ? "" : u1.CredImg2,
                            UserType = u1.UserType,
                            RealCheck = u1.RealCheck,
                            Status = u1.Status,
                            CreateTime = u1.CreateTime,
                        };

                if (id != Guid.Empty)
                {
                    q = q.Where(m => m.ID == id);
                }
                if (!string.IsNullOrWhiteSpace(key))
                {
                    q = q.Where(m => m.Name == key || m.TrueName.Contains(key) || m.Phone.Contains(key)
                      || m.CardNumber == key || m.CredID == key);
                }

                return GetPagedList(q.OrderByDescending(m => m.CreateTime), pagesize, pageinex, out count);
            }
        }

        public List<Users> GetUsersSearchAnd(string name, string truename, string phone, string wxname, string cardnumber)
        {
            using (DBContext)
            {
                var q = DBContext.Users.Where(m => m.UserType == (int)enUserType.User);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    q = q.Where(m => m.Name == name);
                }
                if (!string.IsNullOrWhiteSpace(truename))
                {
                    q = q.Where(m => m.TrueName == truename);
                }
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    q = q.Where(m => m.Phone == phone);
                }
                if (!string.IsNullOrWhiteSpace(wxname))
                {
                    q = q.Where(m => m.WxName == wxname);
                }
                if (!string.IsNullOrWhiteSpace(cardnumber))
                {
                    q = q.Where(m => m.CardNumber == cardnumber);
                }

                return q.ToList();
            }
        }

        /// <summary>
        /// 登陆，可用手机号、账号、微信等登陆
        /// </summary>
        /// <param name="name"></param>
        /// <param name="psw"></param>
        /// <returns></returns>
        public Users Login(string name, string psw)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            //logBll.InsertLog(string.Format("用户{0}尝试登陆中...", name), enLogType.Login);
            using (DBContext)
            {
                psw = MD5(psw);
                //var q = DBContext.Users.Where(m => ((m.Name == name || m.CardNumber == name || m.Phone == name) && m.Psw == psw)
                //|| m.WxName == name);
                var q = DBContext.Users.Where(m => m.Name == name && m.Psw == psw);
                var m2 = q.FirstOrDefault();

                if (m2 != null)
                    logBll.InsertLog(string.Format("用户{0}-{1}-{2}登录成功...", m2.ID, m2.Phone, m2.CardNumber), enLogType.Login);

                return m2;
            }
        }

        public int ChangeLoginPsw(string phone, string oldpsw, string newpsw)
        {
            using (DBContext)
            {
                oldpsw = MD5(oldpsw);
                var q = DBContext.Users.FirstOrDefault(m => m.Phone == phone && m.Psw == oldpsw);
                if (q != null)
                {
                    q.Psw = MD5(newpsw);
                    q.UpdateTime = DateTime.Now;
                    var result = DBContext.SaveChanges();
                    logBll.InsertLog(string.Format("用户：{0},修改登录密码成功", q.Name), enLogType.User);
                    return result;
                }
                return 0;
            }
        }

        public int ResetWXName(Guid UID, string NewWX)
        {
            using (DBContext)
            {
                var u = DBContext.Users.FirstOrDefault(m => m.ID == UID);
                if (u != null)
                {
                    if (string.IsNullOrEmpty(NewWX) || DBContext.Users.FirstOrDefault(m => m.WxName == NewWX) == null)
                        u.WxName = NewWX;
                    else
                    {
                        return (int)ErrorCode.微信已被注册;
                    }
                    return DBContext.SaveChanges();
                }
                return (int)ErrorCode.账号不存在;
            }
        }

        public int ChangeLoginPsw(string phone, string newpsw)
        {
            using (DBContext)
            {
                var q = DBContext.Users.FirstOrDefault(m => m.Phone == phone);
                if (q != null)
                {
                    q.Psw = MD5(newpsw);
                    q.UpdateTime = DateTime.Now;
                    var result = DBContext.SaveChanges();
                    logBll.InsertLog(string.Format("用户：{0},修改登录密码成功", q.Name), enLogType.User);
                    return result;
                }
                return 0;
            }
        }

        public int ChangePayPsw(Guid id, string oldpsw, string newpsw)
        {
            using (DBContext)
            {

                oldpsw = MD5(oldpsw);
                var q = DBContext.Users.FirstOrDefault(m => m.ID == id);
                if (q != null && (q.PayPsw == oldpsw || string.IsNullOrEmpty(q.PayPsw)))
                {
                    q.PayPsw = MD5(newpsw);
                    q.UpdateTime = DateTime.Now;
                    var result = DBContext.SaveChanges();
                    logBll.InsertLog(string.Format("用户：{0},修改支付密码成功", q.Name), enLogType.User);
                    return result;
                }
                return 0;
            }
        }

        /// <summary>
        /// 校验支付密码是否正确
        /// </summary>
        /// <param name="id"></param>
        /// <param name="psw"></param>
        /// <returns></returns>
        public int CheckPay(Guid id, string psw)
        {
            using (DBContext)
            {
                psw = MD5(psw);
                var u = DBContext.Users.FirstOrDefault(m => m.ID == id);
                if (u == null)
                {
                    return (int)ErrorCode.账号不存在;
                }
                if (string.IsNullOrEmpty(u.PayPsw))
                {
                    return (int)ErrorCode.密码尚未设置;
                }
                if (u.PayPsw != psw)
                {
                    return (int)ErrorCode.密码不正确;
                }
                return 1;
            }
        }

        public int InsertUser(string name, string psw, string truename, string CredID, enUserType usertype, Guid Fid)
        {
            if (string.IsNullOrWhiteSpace(truename))
            {
                //?不正确
                return (int)ErrorCode.姓名不正确;
            }
            //密码不能少于6位数
            if ((string.IsNullOrWhiteSpace(psw) || psw.Length < 6))
            {
                //?不正确
                return (int)ErrorCode.密码格式不正确;
            }
            //手机号码不能为空，且手机号码必须11位数
            if (!IsPhone(name))
            {
                //?不正确
                return (int)ErrorCode.手机不正确;
            }
            if (string.IsNullOrWhiteSpace(CredID))
            {
                //?不正确
                return (int)ErrorCode.姓名不正确;
            }

            using (DBContext)
            {
                if (DBContext.Users.FirstOrDefault(m => m.Name == name) != null)//用户名或者手机号重复？
                    return (int)ErrorCode.账号已被注册;
                if (!SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "CANMOREACCOUNT").Value.Contains(CredID + ",")
                    && DBContext.Users.FirstOrDefault(m => m.CredID == CredID) != null)//用户名或者手机号重复？
                    return (int)ErrorCode.身份证已被注册;

                Users model = new Users()
                {
                    Name = name,
                    Psw = MD5(psw),
                    UserType = (int)usertype,
                    Phone = name,
                    FID = Fid,
                    CredID = CredID,
                    CommendID = Fid,
                    TrueName = truename,

                    //以下信息自动生成
                    ID = Guid.NewGuid(),
                    Status = (int)enStatus.Enabled,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    CardNumber = "",
                    WxName = "",
                    PayPsw = "",
                    LV = 0,
                };
                DBContext.Users.Add(model);

                try
                {
                    int result = DBContext.SaveChanges();
                    if (result > 0)
                    {
                        model.CardNumber = FunCardNum(model.C_index);
                        DBContext.SaveChanges();

                        logBll.InsertLog(string.Format("注册用户成功：{0},手机号{1},推荐人ID{2}", name, truename, Fid), enLogType.User);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    logBll.InsertLog(string.Format("注册用户失败：{0},手机号{1},推荐人ID{2},错误：{3}", name, truename, Fid, ex), enLogType.User);
                    return 0;
                }
            }
        }

        /// <summary>
        /// 身份证是否可以注册多个账号
        /// </summary>
        /// <param name="CardID"></param>
        /// <returns></returns>
        private bool CanMoreAccount(string CardID)
        {
            var temp = SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == SysConfigBLL.CANMOREACCOUNT);
            if (temp == null)
                return false;
            return temp.Value.Contains(CardID + ",");
        }

        public int InsertUser_Admin(string name, string psw, string phone, string wxid, enUserType usertype, Guid Fid
    , int cartlv, decimal score, decimal balance, string address, string desc, string headimg, string truename)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                //?不正确
                return (int)ErrorCode.姓名不正确;
            }
            //密码不能少于6位数
            if (string.IsNullOrWhiteSpace(psw) || psw.Length < 6)
            {
                //?不正确
                return (int)ErrorCode.密码格式不正确;
            }
            //手机号码不能为空，且手机号码必须11位数
            if (!IsPhone(phone))
            {
                //?不正确
                return (int)ErrorCode.手机不正确;
            }

            using (DBContext)
            {
                if (DBContext.Users.FirstOrDefault(m => m.Phone == phone || m.Name == name || (!string.IsNullOrEmpty(wxid) && m.WxName == wxid)) != null)//用户名或者手机号重复？
                    return (int)ErrorCode.账号已被注册;

                Users model = new Users()
                {
                    Name = name,
                    Psw = MD5(psw),
                    UserType = (int)usertype,
                    Phone = phone,
                    FID = Fid,
                    LV = (int)cartlv,
                    Score = score,
                    TotalScore = score,
                    Balance = balance,
                    Address = address,
                    Descrition = desc,
                    HeadImg1 = headimg,
                    TrueName = truename,
                    WxName = wxid,

                    //以下信息自动生成
                    ID = Guid.NewGuid(),
                    Status = (int)enStatus.Enabled,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    CardNumber = "",
                    PayPsw = "",
                };
                DBContext.Users.Add(model);

                try
                {
                    int result = DBContext.SaveChanges();
                    if (result > 0)
                    {
                        model.CardNumber = FunCardNum(model.C_index);
                        DBContext.SaveChanges();

                        logBll.InsertLog(string.Format("注册用户成功：{0},手机号{1},推荐人ID{2}", name, phone, Fid), enLogType.User);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    logBll.InsertLog(string.Format("注册用户失败：{0},手机号{1},推荐人ID{2},错误：{3}", name, phone, Fid, ex), enLogType.User);
                    return 0;
                }
            }
        }

        public string FunCardNum(int index)
        {
            return "QC" + DateTime.Now.Year + index.ToString().PadLeft(8 - index.ToString().Length, '0');
        }

        /// <summary>
        /// 申请实名验证
        /// </summary>
        /// <returns></returns>
        public int ApplyRealCheck(Guid id, string realname, string cid, string img1, string img2)
        {
            using (DBContext)
            {
                var user = DBContext.Users.FirstOrDefault(m => m.ID == id);
                if (user == null && user.RealCheck != 0)//已经申请过，或者账号不存在
                    return 0;

                user.TrueName = realname;
                user.CredID = cid;
                user.CredImg1 = img1;
                user.CredImg2 = img2;
                user.RealCheck = (int)enRealCheckStatus.审核中;

                logBll.InsertLog("用户" + user.ID + "正在申请实名验证", enLogType.User);
                return DBContext.SaveChanges();
            }
        }

        /// <summary>
        /// 通过实名验证
        /// </summary>
        /// <returns></returns>
        public int AcceptRealCheck(Guid id, enRealCheckStatus State, string remark)
        {
            using (DBContext)
            {
                var user = DBContext.Users.FirstOrDefault(m => m.ID == id);
                if (user == null)//已经申请过，或者账号不存在
                    return (int)ErrorCode.账号不存在;
                if (user.RealCheck != (int)enRealCheckStatus.审核中)
                    return (int)ErrorCode.状态异常或已处理;

                user.RealCheck = (int)State;

                UserMsgBLL msgbll = new UserMsgBLL();
                if (State == enRealCheckStatus.不通过)
                {
                    msgbll.Insert(user.ID, Guid.Empty, "实名验证不通过", "您的实名制认证不通过，原因：" + remark, enMessageType.System, "", "实名验证不通过");
                }
                else
                {
                    msgbll.Insert(user.ID, Guid.Empty, "实名验证通过", "您的实名制认证已通过，您现在可以进行提现操作了。", enMessageType.System, "", "实名验证通过");
                }

                user.UpdateTime = DateTime.Now;

                logBll.InsertLog("用户" + user.ID + "实名验证" + State, enLogType.User);
                return DBContext.SaveChanges();
            }
        }

        public int ChangePhone(Guid id, string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return 0;
            }
            using (DBContext)
            {
                var u = DBContext.Users.FirstOrDefault(m => m.ID == id);
                if (u == null && DBContext.Users.FirstOrDefault(m => m.Phone == phone) != null)
                    return 0;
                u.Phone = phone;
                var result = DBContext.SaveChanges();
                logBll.InsertLog(string.Format("用户：{0},修改手机号码为{1}", u.Name, u.Phone), enLogType.User);
                return result;
            }
        }

        public int UpdateUser(Users model)
        {
            using (DBContext)
            {
                DBContext.Users.Attach(model);
                DBContext.Entry<Users>(model).State = System.Data.Entity.EntityState.Modified;
                return DBContext.SaveChanges();
            }
        }

        public int DeleteUser(string name)
        {
            using (DBContext)
            {
                var q = DBContext.Users.FirstOrDefault(m => m.Name == name);
                if (q != null)
                {
                    q.Status = (int)enStatus.Unabled;//不可用
                    return DBContext.SaveChanges();
                }
                return 0;
            }
        }

        /// <summary>
        /// 是否购买过牙商
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public OtherPayServiceLog HadBuyYashang(Guid id)
        {
            using (DBContext)
            {
                var o = DBContext.OtherPayServiceLog.FirstOrDefault(m => m.UID == id
                && m.OrderType == (int)enPayFrom.UpdateUserLV
                && m.Status == (int)enStatus.Enabled);
                return o;
            }
        }

        /// <summary>
        /// 是否要牙商提货
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public OtherPayServiceLog CanYashangTihuo(Guid id)
        {
            using (DBContext)
            {
                //已购买，且尚未提货
                var o = DBContext.OtherPayServiceLog.FirstOrDefault(m => m.UID == id
                && m.OrderType == (int)enPayFrom.UpdateUserLV
                && m.Status == (int)enStatus.Enabled && m.OrderNumber == "");
                return o;
            }
        }
    }
}
