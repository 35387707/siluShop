using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;
using System.Data.SqlClient;
using System.Data.Entity.Validation;

namespace RelexBarBLL
{
    public partial class OrdersBLL : BaseBll
    {
        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="OID"></param>
        /// <returns></returns>
        public int CancelOrder(Guid? UID, Guid OID)
        {
            using (DBContext)
            {
                OrderList ol;
                if (UID == null)
                {
                    ol = DBContext.OrderList.Where(m => m.ID == OID).FirstOrDefault();
                }
                else
                {
                    ol = DBContext.OrderList.Where(m => m.ID == OID && m.UID == UID).FirstOrDefault();
                }

                if (ol == null)
                {
                    return (int)ErrorCode.订单不存在;
                }
                if (ol.Status > (int)enOrderStatus.Payed || ol.Status == (int)enOrderStatus.Cancel)
                {
                    return (int)ErrorCode.订单异常;
                }
                if (ol.LocalPrice.HasValue)
                {
                    Users u = DBContext.Users.Where(m => m.ID == ol.UID).FirstOrDefault();
                    if (u != null)
                    {
                        u.ShoppingVoucher += ol.LocalPrice.Value;
                        PayList pl = new PayList();
                        pl.UID = UID;
                        pl.InOut = (int)Common.enPayInOutType.In;
                        pl.PayType = (int)Common.enPayListType.ShoppingVoucher;
                        pl.FromTo = (int)Common.enPayFrom.pay;
                        pl.Val = ol.LocalPrice.Value;
                        pl.Status = 1;
                        pl.CreateTime = pl.UpdateTime = DateTime.Now;
                        pl.PriceType = (int)Common.enPriceType.ShoppingVoucher;
                        pl.Remark = "订单取消，退还本地支付订单50%购物券";
                        DBContext.PayList.Add(pl);
                    }

                }
                List<OrderProductList> opl = DBContext.OrderProductList.Where(m => m.OrderID == OID).ToList();
                for (int i = 0; i < opl.Count; i++)
                {
                    OrderProductList op = opl[i];
                    if (op.SpecID != null)
                    {
                        TrueProduct t = DBContext.TrueProduct.Where(m => m.SPID == op.SpecID).FirstOrDefault();
                        if (t == null)
                        {
                            return (int)ErrorCode.商品不存在;
                        }
                        t.Stock += op.Count;
                        op.Status = (int)enOrderStatus.Cancel;
                    }
                    else
                    {
                        ProductList p = DBContext.ProductList.Where(m => m.ID == op.ProductID).FirstOrDefault();
                        if (p == null)
                        {
                            return (int)ErrorCode.商品不存在;
                        }
                        p.Stock += op.Count;
                        op.Status = (int)enOrderStatus.Cancel;
                    }
                }
                ol.Status = (int)enOrderStatus.Cancel;
                return DBContext.SaveChanges();

            }
        }
        /// <summary>
        /// 本地支付
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="OID"></param>
        /// <returns>本地支付的购物券金额</returns>
        public decimal? LocalPay(Guid UID, Guid OID)
        {
            using (DBContext)
            {
                OrderList o = DBContext.OrderList.Where(m => m.ID == OID && m.UID == UID).FirstOrDefault();
                o.Payment = Common.enPayment.LOCAL.ToString();
                Users u = DBContext.Users.Where(m => m.ID == UID).FirstOrDefault();
                if (u == null)
                {
                    throw new Exception("用户不存在");
                }
                if (o == null)
                {
                    throw new Exception("订单不存在");
                }

                decimal localprice = o.Cash.Value;
                decimal maintainScore = 0;
                var payRecord = DBContext.PayRecord.Where(m => m.UID == UID).ToList();
                var payDetails = DBContext.PayListDetail.Where(m => m.UID == UID).ToList();
                if (payRecord.Count > 0)
                {
                    maintainScore = payRecord.Sum(m => m.MaxMoney);
                    if (payDetails.Count > 0)
                    {
                        maintainScore -= payDetails.Sum(m => m.Price);
                    }
                }
                if (maintainScore < localprice)
                {
                    throw new Exception("易物券不足");
                }

                decimal? _transtotal = localprice, _transigle = 0;
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
                        pd.Remark = "消费易物券：" + pd.Price;
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

                o.LocalPrice = localprice;
                o.Status = (int)enOrderStatus.Payed;

                PayList pl = new PayList();
                pl.UID = UID;
                pl.InOut = (int)Common.enPayInOutType.Out;
                pl.PayType = (int)Common.enPayListType.ShoppingVoucher;
                pl.FromTo = (int)Common.enPayFrom.pay;
                pl.Val = localprice;
                pl.Status = 1;
                pl.CreateTime = pl.UpdateTime = DateTime.Now;
                pl.PriceType = (int)Common.enPriceType.ShoppingVoucher;
                pl.Remark = "本地支付订单" + (SysConfigBLL.LocalPay * 100) + "易物券";
                DBContext.PayList.Add(pl);
                int i = DBContext.SaveChanges();
                if (i > 0)
                {
                    return localprice;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 订单评论
        /// </summary>
        /// <param name="oplid"></param>
        /// <param name="score"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public int AddOrderComment(Guid UID, Guid oplid, int score, string comment)
        {
            using (DBContext)
            {
                OrderProductList opl = DBContext.OrderProductList.Where(m => m.ID == oplid).FirstOrDefault();
                Guid? tUID = DBContext.OrderList.Where(m => m.ID == opl.OrderID && m.UID == UID).Select(m => m.ID).FirstOrDefault();
                if (tUID == null)
                {
                    throw new Exception("订单不存在");
                }
                if (opl == null)
                {
                    throw new Exception("订单不存在");
                }
                if (opl.Score != null)
                {
                    throw new Exception("不能重复评论");
                }
                if (score > 5 || score < 1)
                {
                    throw new Exception("评分范围不正确");
                }
                opl.Score = score;
                opl.Comment = comment;
                return DBContext.SaveChanges();
            }
        }
        public string[] ShopIndexData(Guid ShopID)
        {
            using (DBContext)
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("select (select CONVERT(varchar,Count(id)) from OrderProductList");
                sql.Append(" where ShopID='" + ShopID + "' and Status=1)+','+");
                sql.Append("(select CONVERT(varchar,Count(id)) from OrderProductList");
                sql.Append(" where ShopID='" + ShopID + "' and Status=0)+','+");
                sql.Append("(select CONVERT(varchar,Count(id)) from OrderProductList");
                sql.Append(" where ShopID='" + ShopID + "' and Status=4)+','+");
                sql.Append("(select CONVERT(varchar,COUNT(id)) from ProductList where ShopID='" + ShopID + "')");
                var data = DBContext.Database.SqlQuery<string>(sql.ToString()).First();
                return data.Split(',');
            }
        }
        /// <summary>
        /// 通过订单号获得订单
        /// </summary>
        /// <param name="Number"></param>
        /// <returns></returns>
        public OrderList GetByNumber(string Number)
        {
            using (DBContext)
            {
                return DBContext.OrderList.Where(m => m.Number == Number).FirstOrDefault();
            }
        }
        public OrderList GetDetail(Guid ID, Guid? UID = null)
        {
            using (DBContext)
            {
                if (UID != null)
                {
                    return DBContext.OrderList.FirstOrDefault(m => m.ID == ID && m.UID == UID.Value);
                }
                return DBContext.OrderList.FirstOrDefault(m => m.ID == ID);
            }
        }
        /// <summary>
        /// 本地支付
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="OID"></param>
        /// <returns></returns>
        /*
        public int LocalPay(Guid UID,Guid OID) {
            using (DBContext) {
                Users user = DBContext.Users.Where(m => m.ID == UID).FirstOrDefault();
                if (user == null) {
                    throw new Exception("用户不存在");
                }
                OrderList order = DBContext.OrderList.Where(m => m.ID == OID).FirstOrDefault();
                if (order==null) {
                    throw new Exception("订单不存在");
                }
                if (user.ShoppingVoucher<order.Price) {
                    throw new Exception("购物券不足");
                }
                user.ShoppingVoucher -= order.Price;
                PayList paylist = new PayList();
                paylist.UID = user.ID;
                paylist.InOut = (int)Common.enPayInOutType.Out;
                paylist.PayType = (int)Common.enPayType.Point;
                paylist.FromTo = (int)Common.enPayFrom.OnLinePay;
                paylist.Val = order.Price;
                paylist.Remark = "订单支付:" + order.Number;
                paylist.Status = 1;
                paylist.CreateTime = paylist.UpdateTime= DateTime.Now;
                
                DBContext.PayList.Add(paylist);

                order.Status = (int)Common.enOrderStatus.Payed;
                List<OrderProductList> opflist = DBContext.OrderProductList.Where(m => m.OrderID == order.ID).ToList();
                for (int i = 0; i < opflist.Count; i++)
                {
                    opflist[i].Status = (int)Common.enOrderStatus.Payed;
                    DBContext.Database.ExecuteSqlCommand("update ProductList set Payed+=1 where ID='"+opflist[i].ProductID+"'");
                }
                List<ShopT> ShopList = (from s in opflist
                                          group s by new { s.ShopID } into t
                                          select new ShopT
                                          {
                                              ShopID = t.Key.ShopID,
                                              Price = t.Sum(s => s.Price*s.Count)
                                          }
                                        ).ToList();
                for (int i = 0; i < ShopList.Count; i++)
                {
                    if (ShopList[i].ShopID!=null&&ShopList[i].ShopID!=Guid.Empty) {
                        Guid shopid = ShopList[i].ShopID.Value;
                        Users shop = DBContext.Users.Where(m=>m.ID==DBContext.Shop.Where(s=>s.ID== shopid).Select(s=>s.UID).FirstOrDefault()).FirstOrDefault();
                        if (shop==null) {
                            throw new Exception("商家不存在");
                        }
                        PayList pl = new PayList();
                        pl.UID = shop.ID;
                        pl.InOut = (int)Common.enPayInOutType.In;
                        pl.PayType = (int)Common.enPayType.Point;
                        pl.FromTo = (int)Common.enPayFrom.Shop;
                        pl.Val =ShopList[i].Price;
                        pl.Remark = "店铺收入，订单号:" + order.Number;
                        pl.Status = 1;
                        pl.CreateTime = pl.UpdateTime = DateTime.Now;
                        DBContext.PayList.Add(pl);
                        shop.Score += ShopList[i].Price;
                    }
                }
                return DBContext.SaveChanges();
            }
        }
        */
        class ShopT
        {
            public Guid? ShopID { get; set; }
            public decimal Price { get; set; }
        }
        /// <summary>
        /// 支付成功后的区县市代理奖励
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="order"></param>
        public void PaySuccessJL(RelexBarEntities entity, OrderList o, OrderProductList order)
        {
            //ProductList p = entity.ProductList.Where(m => m.ID == order.ProductID).FirstOrDefault();
            //if (p == null)
            //{
            //    throw new Exception("商品不存在");
            //}
            //TrueProduct tp = null;
            //if (order.SpecID != null)
            //{
            //    tp = entity.TrueProduct.Where(m => m.SPID == order.SpecID).FirstOrDefault();
            //}
            //Shop pshop = entity.Shop.Where(m => m.ID == p.ShopID).FirstOrDefault();
            //if (pshop == null)
            //{
            //    throw new Exception("商家不存在");
            //}
            ////国，省，市，区/县
            //string areaid = pshop.AreaID;
            ////获得区县代理 商家的用户
            //List<Users> qUser = entity.Users.Where(u => entity.Shop.Where(s => s.AreaID == areaid && s.AgentType == (int)Common.enShopAgentType.District).Select(s => s.UID).ToList().Contains(u.ID)).ToList();
            ////市代理
            //string sareaid = areaid.Substring(0, areaid.LastIndexOf(','));
            //List<Users> sUser = entity.Users.Where(u => entity.Shop.Where(s => s.AgentType == (int)Common.enShopAgentType.City && s.AreaID.StartsWith(areaid)).Select(s => s.UID).ToList().Contains(u.ID)).ToList();
            //decimal payPrice = 0;
            //if (o.Payment == Common.enPayment.LOCAL.ToString())
            //{//如果是本地支付
            //    payPrice = o.Price;
            //}
            //else
            //{
            //    payPrice = o.Cash.Value;//现金
            //}
            //decimal qjl = payPrice * 0.01M;//区县奖励
            //decimal sjl = payPrice * 0.02M;//市代理
            //for (int i = 0; i < qUser.Count; i++)
            //{
            //    qUser[i].Score += qjl;
            //    PayList pl = new PayList();
            //    pl.UID = qUser[i].ID;
            //    pl.InOut = (int)Common.enPayInOutType.In;
            //    pl.PayType = (int)Common.enPayType.Point;
            //    pl.FromTo = (int)Common.enPayFrom.ShopAgent;
            //    pl.Val = qjl;
            //    pl.Remark = "区/县代理2%收入";
            //    pl.Status = 1;
            //    pl.CreateTime = pl.UpdateTime = DateTime.Now;
            //    pl.PriceType = (int)Common.enPriceType.Score;
            //    entity.PayList.Add(pl);
            //}
            //for (int i = 0; i < sUser.Count; i++)
            //{
            //    sUser[i].Score = sjl;
            //    PayList pl = new PayList();
            //    pl.UID = sUser[i].ID;
            //    pl.InOut = (int)Common.enPayInOutType.In;
            //    pl.PayType = (int)Common.enPayType.Point;
            //    pl.FromTo = (int)Common.enPayFrom.ShopAgent;
            //    pl.Val = qjl;
            //    pl.Remark = "市代理1%收入";
            //    pl.Status = 1;
            //    pl.CreateTime = pl.UpdateTime = DateTime.Now;
            //    pl.PriceType = (int)Common.enPriceType.Score;
            //    entity.PayList.Add(pl);
            //}
        }
        //支付成功后卖家上一级，二级奖励
        public void PaySuccessShopPreJL(RelexBarEntities entity, OrderList o, OrderProductList order)
        {
            //Users shop = entity.Users.Where(m => m.ID == entity.Shop.Where(s => s.ID == order.ShopID).Select(m1 => m1.UID).FirstOrDefault()).FirstOrDefault();
            //if (shop != null && shop.FID != null)
            //{
            //    //获取商家的上一级用户
            //    Users preu1 = entity.Users.Where(m => m.ID == shop.FID).FirstOrDefault();
            //    if (preu1 != null)
            //    {
            //        decimal payPrice = 0;
            //        if (o.Payment == Common.enPayment.LOCAL.ToString())
            //        {
            //            payPrice = o.Price;
            //        }
            //        else
            //        {
            //            payPrice = o.Cash.Value;//现金
            //        }
            //        decimal jl = payPrice * 0.02M;
            //        preu1.Score += jl;
            //        PayList pl = new PayList();
            //        pl.UID = preu1.ID;
            //        pl.InOut = (int)Common.enPayInOutType.In;
            //        pl.PayType = (int)Common.enPayType.Point;
            //        pl.FromTo = (int)Common.enPayFrom.NextShop;
            //        pl.Val = jl;
            //        pl.Remark = "下一级商家出售商品2%奖励";
            //        pl.Status = 1;
            //        pl.CreateTime = pl.UpdateTime = DateTime.Now;
            //        pl.PriceType = (int)Common.enPriceType.Score;
            //        entity.PayList.Add(pl);
            //        if (preu1.FID != null)
            //        {
            //            Users preu2 = entity.Users.Where(m => m.ID == preu1.FID).FirstOrDefault();
            //            if (preu2 != null)
            //            {
            //                decimal jl2 = payPrice * 0.01M;
            //                preu2.Score += jl2;
            //                PayList pl2 = new PayList();
            //                pl2.UID = preu2.ID;
            //                pl2.InOut = (int)Common.enPayInOutType.In;
            //                pl2.PayType = (int)Common.enPayType.Point;
            //                pl2.FromTo = (int)Common.enPayFrom.NextShop;
            //                pl2.Val = jl2;
            //                pl2.Remark = "下二级商家出售商品1%奖励";
            //                pl2.Status = 1;
            //                pl2.CreateTime = pl2.UpdateTime = DateTime.Now;
            //                pl2.PriceType = (int)Common.enPriceType.Score;
            //                entity.PayList.Add(pl);
            //            }
            //        }

            //    }
            //}
        }
        //支付成功后购买人上级，二级奖励，自己可获得两倍积分的易物券
        public void PaySuccessUserPreJL(RelexBarEntities entity, OrderList o, Users user)
        {
            if (user.FID != null)
            {
                decimal payPrice = 0;
                if (o.Payment == Common.enPayment.LOCAL.ToString())
                {
                    payPrice = o.Cash.Value;//现金 ;
                }
                else
                {
                    payPrice = o.Price;//现金
                    //如果是现金支付，则获得两倍积分
                    PayRecord pl = new PayRecord();
                    pl.ID = Guid.NewGuid();
                    pl.UID = user.ID;
                    pl.Status = (int)Common.enPayInOutType.In;
                    pl.MaxMoney = payPrice * 2;
                    pl.LocalMoney = payPrice * 2 / 300;//300天返还
                    pl.IsBuyProduct = 0;
                    pl.Status = 1;
                    pl.CreateTime = DateTime.Now;
                    pl.OrderID = o.ID;
                    entity.PayRecord.Add(pl);
                }

                Users preu = entity.Users.Where(m => m.ID == user.FID).FirstOrDefault();//找到上级
                if (preu != null)
                {
                    decimal jl = payPrice * 0.02M;
                    preu.Balance += jl;
                    PayList pl = new PayList();
                    pl.UID = preu.ID;
                    pl.InOut = (int)Common.enPayInOutType.In;
                    pl.PayType = (int)Common.enPayType.Coin;
                    pl.FromTo = (int)Common.enPayFrom.ShopAgent;
                    pl.Val = jl;
                    pl.Remark = "下一级用户购买商品2%奖励";
                    pl.Status = 1;
                    pl.CreateTime = pl.UpdateTime = DateTime.Now;
                    pl.PriceType = (int)Common.enPriceType.Balance;
                    entity.PayList.Add(pl);

                    Users preu2 = null;
                    if (preu.FID != null)
                    {
                        preu2 = entity.Users.Where(m => m.ID == preu.FID).FirstOrDefault();
                        if (preu2 != null && preu2.LV > (int)enUserLV.普通用户)
                        {
                            decimal jl2 = payPrice * 0.01M;
                            preu2.Balance += jl;
                            PayList pl2 = new PayList();
                            pl2.UID = preu2.ID;
                            pl2.InOut = (int)Common.enPayInOutType.In;
                            pl2.PayType = (int)Common.enPayType.Coin;
                            pl2.FromTo = (int)Common.enPayFrom.ShopAgent;
                            pl2.Val = jl2;
                            pl2.Remark = "下二级用户购买商品1%奖励";
                            pl2.Status = 1;
                            pl2.CreateTime = pl2.UpdateTime = DateTime.Now;
                            pl2.PriceType = (int)Common.enPriceType.Balance;
                            entity.PayList.Add(pl2);
                        }
                    }
                    //行商奖励
                    var hangshang = GetFirstGudong(entity, user.FID.Value, enUserLV.行商);
                    if (hangshang != null)
                    {
                        if ((preu2 == null || hangshang.ID != preu2.ID) && hangshang.ID != preu.ID)
                        {
                            decimal jl2 = payPrice * 0.01M;
                            hangshang.Balance += jl2;//可获取1%奖励
                            PayList pl2 = new PayList();
                            pl2.UID = hangshang.ID;
                            pl2.InOut = (int)Common.enPayInOutType.In;
                            pl2.PayType = (int)Common.enPayType.Coin;
                            pl2.FromTo = (int)Common.enPayFrom.ShopAgent;
                            pl2.Val = jl2;
                            pl2.Remark = "下级用户购买商品，行商得1%奖励";
                            pl2.Status = 1;
                            pl2.CreateTime = pl2.UpdateTime = DateTime.Now;
                            pl2.PriceType = (int)Common.enPriceType.Balance;
                            entity.PayList.Add(pl2);
                        }
                    }

                    ///股东奖励
                    var gudong = GetFirstGudong(entity, user.FID.Value, enUserLV.股东);
                    if (gudong != null)
                    {
                        decimal jl2 = payPrice * 0.01M;
                        if (preu2 != null && gudong.ID == preu2.ID)
                        {
                            preu2.Balance += jl2;//可获取1%奖励
                        }
                        else if (gudong.ID == preu.ID)
                        {
                            preu.Balance += jl2;//可获取1%奖励
                        }
                        else
                        {
                            gudong.Balance += jl2;//可获取1%奖励
                        }
                        PayList pl2 = new PayList();
                        pl2.UID = gudong.ID;
                        pl2.InOut = (int)Common.enPayInOutType.In;
                        pl2.PayType = (int)Common.enPayType.Coin;
                        pl2.FromTo = (int)Common.enPayFrom.ShopAgent;
                        pl2.Val = jl2;
                        pl2.Remark = "下级用户购买商品，股东得1%奖励";
                        pl2.Status = 1;
                        pl2.CreateTime = pl2.UpdateTime = DateTime.Now;
                        pl2.PriceType = (int)Common.enPriceType.Balance;
                        entity.PayList.Add(pl2);
                    }
                }
            }
        }

        /// <summary>
        /// 支付成功成为牙商
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public int PayYaShangs(Guid pid, Guid uid, string payment)
        {
            using (DBContext)
            {
                var product = DBContext.ProductList.FirstOrDefault(m => m.ID == pid);
                if (product == null)
                {
                    return (int)ErrorCode.商品不存在;
                }
                if (product.Status != (int)Common.enProductType.Virtual)//不是牙商产品
                {
                    return (int)ErrorCode.商品不存在;
                }
                var user = DBContext.Users.FirstOrDefault(m => m.ID == uid);
                if (user == null || user.Status != (int)enStatus.Enabled)
                {
                    return (int)ErrorCode.账号不可用;
                }
                if (user.LV >= (int)enUserLV.牙商)
                {
                    return (int)ErrorCode.账户已经是牙商;
                }

                PayList pl = new PayList();
                pl.CID = Guid.NewGuid();
                pl.UID = user.ID;
                pl.InOut = (int)enPayInOutType.Out;
                pl.PayType = (int)enPayType.Coin;
                pl.FromTo = (int)enPayFrom.UpdateUserLV;
                pl.Val = product.Price;

                if (payment == enPayment.LOCAL.ToString())
                {
                    if (user.Balance < product.Price)
                    {
                        return (int)ErrorCode.账户余额不足;
                    }
                    user.Balance -= product.Price;

                    pl.Remark = "升级为牙商，消费" + product.Price + "首购券。";
                }
                else
                    pl.Remark = "升级为牙商，消费" + product.Price + "元。";

                user.LV = (int)enUserLV.牙商;
                pl.Status = (int)enStatus.Enabled;
                pl.CreateTime = pl.UpdateTime = DateTime.Now;
                pl.PriceType = (int)enPriceType.Balance;
                DBContext.PayList.Add(pl);

                UpdateYaShangReward(DBContext, user);

                return DBContext.SaveChanges();
            }
        }

        /// <summary>
        /// 成为牙商之后，给其他人的奖励，上级拿100，上上级如果是牙商则拿20，并将所有下级放归平台
        /// </summary>
        private void UpdateYaShangReward(RelexBarEntities entity, Users updateUser)
        {
            decimal firstJL = 100, firstJL2 = 50, secJL = 20, gudongJl = 5, hangshangJL = 5;//各级别奖励

            var fuser = entity.Users.FirstOrDefault(m => m.ID == updateUser.FID);
            if (fuser == null)
                return;

            PayList pl = new PayList();
            pl.CID = Guid.NewGuid();
            pl.UID = fuser.ID;
            pl.InOut = (int)enPayInOutType.In;
            pl.PayType = (int)enPayType.Coin;
            pl.FromTo = (int)enPayFrom.Reward;
            pl.Val = (fuser.LV == (int)enUserLV.普通用户 ? firstJL2 : firstJL) + (fuser.LV == (int)enUserLV.股东 ? gudongJl : 0);
            pl.Remark = "下级升级为牙商，可得" + pl.Val + "行票。";
            pl.Status = (int)enStatus.Enabled;
            pl.CreateTime = pl.UpdateTime = DateTime.Now;
            pl.PriceType = (int)enPriceType.Balance;
            entity.PayList.Add(pl);
            fuser.Balance += pl.Val;//可获取100元行票

            var ffuser = entity.Users.FirstOrDefault(m => m.ID == fuser.FID);//上级的上级
            if (ffuser != null && ffuser.LV >= (int)enUserLV.牙商)
            {
                PayList pl2 = new PayList();
                pl2.CID = Guid.NewGuid();
                pl2.UID = ffuser.ID;
                pl2.InOut = (int)enPayInOutType.In;
                pl2.PayType = (int)enPayType.Coin;
                pl2.FromTo = (int)enPayFrom.Reward;
                pl2.Val = secJL + (ffuser.LV == (int)enUserLV.股东 ? gudongJl : 0);
                pl2.Remark = "下级升级为牙商，可得" + pl2.Val + "行票。";
                pl2.Status = (int)enStatus.Enabled;
                pl2.CreateTime = pl2.UpdateTime = DateTime.Now;
                pl2.PriceType = (int)enPriceType.Balance;
                entity.PayList.Add(pl2);

                ffuser.Balance += pl2.Val;//可获取100元行票，如果是股东号
            }
            //行商号奖励
            var hangshang = GetFirstGudong(entity, updateUser.FID.Value, enUserLV.行商);
            if (hangshang != null)
            {
                if ((ffuser == null || hangshang.ID != ffuser.ID) && hangshang.ID != fuser.ID)
                {
                    PayList pl2 = new PayList();
                    pl2.CID = Guid.NewGuid();
                    pl2.UID = hangshang.ID;
                    pl2.InOut = (int)enPayInOutType.In;
                    pl2.PayType = (int)enPayType.Coin;
                    pl2.FromTo = (int)enPayFrom.Reward;
                    pl2.Val = hangshangJL;
                    pl2.Remark = "下级升级为牙商，可得" + pl2.Val + "行票。";
                    pl2.Status = (int)enStatus.Enabled;
                    pl2.CreateTime = pl2.UpdateTime = DateTime.Now;
                    pl2.PriceType = (int)enPriceType.Balance;
                    entity.PayList.Add(pl2);
                    hangshang.Balance += pl2.Val;//可获取100元行票，如果是行商
                }
            }

            //股东号奖励
            var gudong = GetFirstGudong(entity, updateUser.FID.Value, enUserLV.股东);
            if (gudong != null)
            {
                if ((ffuser == null || gudong.ID != ffuser.ID) && gudong.ID != fuser.ID)
                {
                    PayList pl2 = new PayList();
                    pl2.CID = Guid.NewGuid();
                    pl2.UID = gudong.ID;
                    pl2.InOut = (int)enPayInOutType.In;
                    pl2.PayType = (int)enPayType.Coin;
                    pl2.FromTo = (int)enPayFrom.Reward;
                    pl2.Val = gudongJl;
                    pl2.Remark = "下级升级为牙商，可得" + pl2.Val + "行票。";
                    pl2.Status = (int)enStatus.Enabled;
                    pl2.CreateTime = pl2.UpdateTime = DateTime.Now;
                    pl2.PriceType = (int)enPriceType.Balance;
                    entity.PayList.Add(pl2);
                    gudong.Balance += pl2.Val;//可获取100元行票，如果是股东号
                }

                //所有账号转到股东号之下
                entity.Database.ExecuteSqlCommand("update users set Fid ={0} where Fid={1}", gudong.ID, updateUser.ID);
            }
        }

        /// <summary>
        /// 获取第一个股东账号
        /// </summary>
        /// <param name="fuser"></param>
        private Users GetFirstGudong(RelexBarEntities entity, Guid fid, enUserLV lv)
        {
            var fuser = entity.Users.FirstOrDefault(m => m.ID == fid);
            if (fuser == null)
                return null;
            if (fuser.LV == (int)lv)
            {
                return fuser;
            }
            if (fuser.FID.HasValue)
                return GetFirstGudong(entity, fuser.FID.Value, lv);
            else
                return null;
        }

        /// <summary>
        /// 支付成功回调
        /// </summary>
        /// <param name="Number">订单编号</param>
        /// <param name="PayNumber">第三方流水</param>
        /// <param name="payment">支付方式</param>
        /// <param name="price">订单金额(第三方收到的)</param>
        /// <returns></returns>
        public int PaySuccess(string Number, string PayNumber, decimal price)
        {
            using (DBContext)
            {
                OrderList order = DBContext.OrderList.Where(m => m.Number == Number).FirstOrDefault();
                if (order == null)
                {
                    return -1;
                }
                List<OrderProductList> opflist = DBContext.OrderProductList.Where(m => m.OrderID == order.ID).ToList();
                if (order.Payment != Common.enPayment.LOCAL.ToString())
                {
                    order.Payment = Common.enPayment.ALI.ToString();
                }
                order.PayNumber = PayNumber;
                order.Status = (int)Common.enOrderStatus.Payed;
                for (int i = 0; i < opflist.Count; i++)
                {
                    opflist[i].Status = (int)Common.enOrderStatus.Payed;
                    try
                    {
                        PaySuccessJL(DBContext, order, opflist[i]);
                    }
                    catch (Exception ex)
                    {
                        OtherPayServiceLog log = new OtherPayServiceLog();
                        log.ID = Guid.NewGuid();
                        log.UpdateTime = log.CreateTime = DateTime.Now;
                        log.Page = string.Empty;
                        log.Payment = order.Payment;
                        log.PayPrice = price;
                        log.PayNumber = order.PayNumber;
                        log.OrderNumber = order.Number;
                        log.ReqStr = "";
                        log.RespStr = "";
                        log.Remark = "订单支付结果,支付成功,订单状态更改失败,区县市代理奖励计算失败：" + order.Number + "ex:" + ex.Message;
                        DBContext.OtherPayServiceLog.Add(log);
                    }
                    try
                    {
                        PaySuccessShopPreJL(DBContext, order, opflist[i]);
                    }
                    catch (Exception ex)
                    {
                        OtherPayServiceLog log = new OtherPayServiceLog();
                        log.UpdateTime = log.CreateTime = DateTime.Now;
                        log.ID = Guid.NewGuid();
                        log.Page = string.Empty;
                        log.Payment = order.Payment;
                        log.PayPrice = price;
                        log.PayNumber = order.PayNumber;
                        log.OrderNumber = order.Number;
                        log.ReqStr = "";
                        log.RespStr = "";
                        log.Remark = "订单支付结果,支付成功,订单状态更改失败,商家上级奖励计算失败：" + order.Number + "ex:" + ex.Message;
                        DBContext.OtherPayServiceLog.Add(log);
                    }
                }
                PaySuccessUserPreJL(DBContext, order, DBContext.Users.Where(m => m.ID == order.UID).FirstOrDefault());
                List<ShopT> ShopList = (from s in opflist
                                        group s by new { s.ShopID } into t
                                        select new ShopT
                                        {
                                            ShopID = t.Key.ShopID,
                                            Price = t.Sum(s => s.Price * s.Count)
                                        }
                                        ).ToList();
                for (int i = 0; i < ShopList.Count; i++)
                {
                    // if (ShopList[i].ShopID != null && ShopList[i].ShopID != Guid.Empty)
                    if (ShopList[i].ShopID != null)
                    {
                        Guid shopid = ShopList[i].ShopID.Value;
                        Users shop = DBContext.Users.Where(m => m.ID == DBContext.Shop.Where(s => s.ID == shopid).Select(s => s.UID).FirstOrDefault()).FirstOrDefault();
                        if (shop == null)
                        {
                            throw new Exception("商家不存在");
                        }
                        PayList pl = new PayList();
                        pl.UID = shop.ID;
                        pl.InOut = (int)Common.enPayInOutType.In;
                        pl.PayType = (int)Common.enPayType.Point;
                        pl.FromTo = (int)Common.enPayFrom.Shop;
                        pl.Val = ShopList[i].Price;
                        pl.Remark = "店铺收入，订单号:" + order.Number;
                        pl.Status = 1;
                        pl.CreateTime = pl.UpdateTime = DateTime.Now;
                        DBContext.PayList.Add(pl);
                        shop.Score += ShopList[i].Price;
                    }
                }

                int j = DBContext.SaveChanges();
                PayListBLL paybll = new PayListBLL();
                if (j > 0)
                {
                    OtherPayServiceLog log = new OtherPayServiceLog();
                    log.ID = Guid.NewGuid();
                    log.Page = "PaySuccess";
                    log.Payment = order.Payment;
                    log.PayPrice = price;
                    log.PayNumber = PayNumber;
                    log.UID = order.UID;
                    log.TOID = Guid.Empty;
                    log.OrderType = 2;
                    log.ReqStr = "";
                    log.RespStr = "";
                    log.Remark = "";
                    log.Status = 1;
                    log.OrderNumber = order.Number;
                    log.CreateTime = DateTime.Now;
                    log.UpdateTime = DateTime.Now;
                    DBContext.OtherPayServiceLog.Add(log);

                    PayList model = new PayList();
                    model.CID = order.ID;
                    model.UID = order.UID;
                    model.InOut = (int)enPayInOutType.Out;
                    model.PayType = (int)enPayListType.ALi;
                    model.FromTo = (int)enPayFrom.OnLinePay;
                    model.Val = price;
                    model.Remark = "支付成功,订单号：" + order.Number;
                    model.Status = (int)enStatus.Enabled;
                    model.CreateTime = model.UpdateTime = DateTime.Now;
                    model.PriceType = (int)enPriceType.RMB;
                    DBContext.PayList.Add(model);
                    try
                    {
                        DBContext.SaveChanges();
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                string ff = (string.Format("Class: {0}, Property: {1}, Error: {2}", validationErrors.Entry.Entity.GetType().FullName,
                                validationError.PropertyName,
                                validationError.ErrorMessage));
                            }
                        }
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                else
                {
                    new LogsBLL().InsertServiceLog(order.Payment, price, order.PayNumber, order.Number, "", "", "订单支付结果,支付成功,订单状态更改失败,订单号：" + order.Number);
                    //PayList model = new PayList();
                    //model.CID = order.ID;
                    //model.UID = order.UID;
                    //model.InOut = (int)enPayInOutType.Out;
                    //model.PayType = (int)enPayListType.ALi;
                    //model.FromTo = (int)enPayFrom.OnLinePay;
                    //model.Val = price;
                    //model.Remark = "订单支付结果,支付成功,订单状态更改失败,订单号：" + order.Number;
                    //model.Status = (int)enStatus.Enabled;
                    //model.CreateTime = model.UpdateTime = DateTime.Now;
                    //model.PriceType = (int)enPriceType.RMB;
                    //DBContext.PayList.Add(model);
                    //DBContext.SaveChanges();
                }
                return j;
            }
        }
        /// <summary>
        /// 获取订单状态
        /// </summary>
        /// <param name="Number"></param>
        /// <returns></returns>
        public int GetOrderStatus(string Number)
        {
            using (DBContext)
            {
                OrderList order = DBContext.OrderList.Where(m => m.Number == Number).FirstOrDefault();
                if (order == null)
                {
                    return (int)ErrorCode.订单不存在;
                }
                return order.Status.Value;
            }
        }
        /// <summary>
        /// 更新订单
        /// </summary>
        /// <param name="OID"></param>
        /// <param name="list"></param>
        /// <param name="recaddress"></param>
        /// <returns></returns>
        public int UpdateOrder(Guid OID, List<dynamic> list, Guid? recaddress, string notes = "")
        {
            using (DBContext)
            {
                OrderList o = DBContext.OrderList.Where(m => m.ID == OID && m.Status == (int)enOrderStatus.Order).FirstOrDefault();
                if (o == null) { throw new Exception("订单不存在"); }
                if (o.Status != (int)enOrderStatus.Order)
                {
                    throw new Exception("订单异常");
                }
                if (recaddress != null)
                {
                    RecAddress recadd = DBContext.RecAddress.Where(m => m.ID == recaddress).FirstOrDefault();
                    if (recaddress == null) { throw new Exception("收货地址不存在"); }

                    o.RecID = recaddress;
                    o.RecName = recadd.TrueName;
                    o.RecAreaID = recadd.AreaID;
                    o.RecAddress = recadd.Address.Replace(",", "");
                    o.RecPhone = recadd.Phone;
                    o.RecSex = recadd.Sex;
                }
                if (!string.IsNullOrEmpty(notes))
                {
                    o.Notes = notes;
                }
                decimal sumPrice = 0;
                decimal sumCash = 0;
                for (int i = 0; i < list.Count; i++)
                {

                    Guid tempid = list[i].id;
                    OrderProductList opl = DBContext.OrderProductList.Where(m => m.ID == tempid && m.OrderID == OID).FirstOrDefault();
                    if (opl == null)
                    {
                        throw new Exception("订单不存在");
                    }
                    int tempcount = list[i].count;
                    sumPrice += opl.Price * tempcount;
                    sumCash += opl.Cash.Value * tempcount;
                    if (opl.Count != tempcount)
                    {
                        if (opl.SpecID != null)
                        {
                            TrueProduct tp = DBContext.TrueProduct.Where(m => m.SPID == opl.SpecID).FirstOrDefault();


                            if (tp.Stock < (tempcount - opl.Count))
                            {
                                throw new Exception("商品数量不足");
                            }
                            tp.Stock += (opl.Count - tempcount);
                            tp.UpdateTime = DateTime.Now;
                        }
                        else
                        {
                            ProductList p = DBContext.ProductList.Where(m => m.ID == opl.ProductID).FirstOrDefault();
                            if (p.Stock < (tempcount - opl.Count))
                            {
                                throw new Exception("商品数量不足");
                            }
                            p.Stock += (opl.Count - tempcount);
                            p.UpdateTime = DateTime.Now;
                        }
                        opl.Count = tempcount;

                    }


                }
                o.Price = sumPrice;
                o.Cash = sumCash;
                return DBContext.SaveChanges();
            }
        }
        //删除订单里的商品
        public int DeleteProduct(Guid id)
        {
            using (DBContext)
            {
                OrderProductList opl = DBContext.OrderProductList.Where(m => m.ID == id && m.Status == (int)enOrderStatus.Order).FirstOrDefault();
                if (opl == null)
                {
                    throw new Exception("订单不存在或已支付");
                }
                if (opl.SpecID != null)
                {
                    TrueProduct tp = DBContext.TrueProduct.Where(m => m.SPID == opl.SpecID).FirstOrDefault();
                    if (tp == null) { throw new Exception("该规格未找到"); };
                    tp.Stock += opl.Count;
                }
                else
                {
                    ProductList p = DBContext.ProductList.Where(m => m.ID == opl.ProductID).FirstOrDefault();
                    if (p == null) { throw new Exception("该商品未找到"); };
                    p.Stock += opl.Count;

                }
                OrderList ol = DBContext.OrderList.Where(m => m.ID == opl.OrderID).FirstOrDefault();
                if (ol == null) { throw new Exception("订单不存在"); }
                if (ol.Status != (int)enOrderStatus.Order)
                {
                    throw new Exception("订单状态不正确");
                }

                int count = DBContext.OrderProductList.Where(m => m.OrderID == ol.ID).Count();
                if (count < 2)
                {
                    DBContext.OrderList.Remove(ol);
                }
                DBContext.OrderProductList.Remove(opl);

                return DBContext.SaveChanges();
            }
        }
        public List<OrderProductList> GetorderProductList(Guid[] OIDS)
        {
            using (DBContext)
            {
                return DBContext.OrderProductList.Where(m => OIDS.Contains(m.OrderID)).ToList();
            }
        }
        /// <summary>
        /// 获取我的订单商品
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public List<OrderProductList> GetOrderProListByUID(Guid UID, int pageIndex, int PageSize, out int sum, int? Status = null)
        {
            using (DBContext)
            {
                var q = DBContext.OrderProductList.Where(m => DBContext.OrderList.Where(u => u.UID == UID).Select(m2 => m2.ID).Contains(m.OrderID));
                if (Status != null)
                {
                    q = q.Where(m => m.Status == Status.Value);
                }
                return GetPagedList(q.OrderByDescending(m => m.CreateTime), PageSize, pageIndex, out sum);
            }
        }
        public List<OrderList> GetOrderListByUID(Guid UID, int pageIndex, int pageSize, out int sum, int? Status = null)
        {
            using (DBContext)
            {
                var q = DBContext.OrderList.Where(m => m.UID == UID);
                if (Status != null)
                {
                    q = q.Where(m => m.Status == Status.Value);
                }
                return GetPagedList(q.OrderByDescending(m => m.CreateTime), pageSize, pageIndex, out sum);
            }
        }
        public RecAddress GetRecAddress(Guid OID)
        {
            using (DBContext)
            {
                return DBContext.RecAddress.Where(m => m.ID == DBContext.OrderList.Where(o => o.ID == OID).Select(o => o.RecID).FirstOrDefault()).FirstOrDefault();
            }
        }
        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="OID"></param>
        /// <param name="UID"></param>
        /// <returns></returns>
        public OrderList GetOrderListByID(Guid OID, Guid? UID = null)
        {
            using (DBContext)
            {
                var q = DBContext.OrderList.Where(m => m.ID == OID);
                if (UID != null)
                {
                    q = q.Where(m => m.UID == UID);
                }
                return q.FirstOrDefault();
            }
        }
        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="PID"></param>
        /// <param name="SPDesc"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int Insert(Guid UID, Guid PID, string SPDesc, int count, out Guid OID)
        {
            OID = Guid.NewGuid();
            using (DBContext)
            {
                Users user = DBContext.Users.Where(m => m.ID == UID).FirstOrDefault();//
                if (user == null)
                {
                    return (int)ErrorCode.账号不存在;
                }
                ProductList p = DBContext.ProductList.Where(m => m.ID == PID).FirstOrDefault();
                if (p == null)
                {
                    return (int)ErrorCode.商品不存在;
                }
                int j = DBContext.TrueProduct.Where(m => m.ProductID == PID).Count();//查询商品是否有规格
                if (j > 0 && string.IsNullOrEmpty(SPDesc))
                {
                    return (int)ErrorCode.商品不存在;
                }
                TrueProduct tp = DBContext.TrueProduct.Where(m => m.ProductID == PID && m.SPDesc.Contains(SPDesc)).FirstOrDefault();

                if (j > 0 && tp == null)
                {
                    return (int)ErrorCode.商品不存在;
                }
                OrderList ol = new OrderList();
                ol.ID = OID;
                ol.Number = Common.GetOrderNumer();
                ol.UID = UID;
                ol.PriceType = (int)enPayType.Coin;
                ol.OrderType = (int)enOrderType.OnLine;
                ol.Status = (int)enOrderStatus.Order;
                ol.CreateTime = DateTime.Now;
                ol.UpdateTime = DateTime.Now;
                ol.RecAreaID = "-1";
                ol.RecAddress = "-1";
                ol.Price = 0;
                ol.Fee = 0;
                DBContext.OrderList.Add(ol);
                OrderProductList opl = new OrderProductList();
                opl.ID = Guid.NewGuid();
                opl.OrderID = ol.ID;
                opl.OrderNumber = ol.Number;
                opl.ProductID = PID;
                opl.Number = p.Number;
                opl.Name = p.Name;
                opl.CategoryID = p.CategoryID;
                opl.Img = p.Img;
                opl.ShopID = p.ShopID;
                opl.PriceType = (int)enPayType.Coin;
                opl.CashDiscount = p.CashDiscount;
                if (tp != null)
                {
                    if (tp.Stock < opl.Count)
                    {
                        return (int)ErrorCode.商品数量不足;
                    }
                    tp.Stock -= count;
                    opl.Price = tp.Price;//单价
                    opl.Cash = tp.RealPrice * p.CashDiscount;
                    opl.SpecID = tp.SPID;
                    opl.SPName = tp.SPName;
                    opl.SPDesc = tp.SPDesc;
                    opl.SPRemark = tp.Remark;
                }
                else
                {
                    if (p.Stock < opl.Count)
                    {
                        return (int)ErrorCode.商品数量不足;
                    }
                    p.Stock -= count;
                    opl.Price = p.Price;//单价
                    opl.Cash = p.RealPrice * p.CashDiscount;
                }
                opl.Count = count;
                ol.Price += opl.Price * count;
                ol.Cash = opl.Cash * count;
                opl.Type = p.Type;
                opl.CreateTime = DateTime.Now;
                opl.UpdateTime = DateTime.Now;

                opl.Status = (int)enOrderStatus.Order;
                DBContext.OrderProductList.Add(opl);
                return DBContext.SaveChanges();
            }
        }
        /// <summary>
        /// 把用户购物车里的商品添加到订单
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="CIDS">购物车id</param>
        /// <returns></returns>
        public int AddOrderByCar(Guid UID, Guid[] CIDS, out Guid OID)
        {
            OID = Guid.NewGuid();
            using (DBContext)
            {
                Users user = DBContext.Users.Where(m => m.ID == UID).FirstOrDefault();//
                if (user == null)
                {
                    return (int)ErrorCode.账号不存在;
                }
                List<Cart> carlist = DBContext.Cart.Where(m => m.UID == UID && CIDS.Contains(m.ID) && m.IsBuy == 0).ToList();
                if (carlist.Count != CIDS.Length)
                {
                    throw new Exception("购物车ID不正确");
                }
                //创建订单begin
                OrderList ol = new OrderList();
                ol.ID = OID;
                ol.Number = Common.GetOrderNumer();
                ol.UID = UID;
                ol.PriceType = (int)enPayType.Coin;
                ol.OrderType = (int)enOrderType.OnLine;
                ol.Status = (int)enOrderStatus.Order;
                ol.CreateTime = DateTime.Now;
                ol.UpdateTime = DateTime.Now;
                ol.RecAreaID = "-1";
                ol.RecAddress = "-1";
                ol.Price = 0;
                ol.Cash = 0;
                ol.Fee = 0;
                DBContext.OrderList.Add(ol);
                //创建订单end
                for (int i = 0; i < carlist.Count; i++)
                {
                    carlist[i].IsBuy = 1;
                    Guid tempid = carlist[i].ProductID.Value;//商品id
                    ProductList p = DBContext.ProductList.Where(m => m.ID == tempid).FirstOrDefault();

                    if (p == null) { return (int)ErrorCode.商品不存在; }

                    OrderProductList opl = new OrderProductList();
                    opl.ID = Guid.NewGuid();
                    opl.OrderID = ol.ID;
                    opl.OrderNumber = ol.Number;
                    opl.ProductID = carlist[i].ProductID;
                    opl.Number = p.Number;
                    opl.Name = p.Name;
                    opl.CategoryID = p.CategoryID;
                    opl.Img = p.Img;
                    opl.PriceType = (int)enPayType.Coin;
                    opl.CashDiscount = p.CashDiscount;
                    if (carlist[i].TrueProductID != null)
                    {
                        Guid tempid2 = carlist[i].TrueProductID.Value;
                        TrueProduct tp = DBContext.TrueProduct.Where(m => m.SPID == tempid2).FirstOrDefault();
                        if (tp == null)
                        {
                            return (int)ErrorCode.商品不存在;
                        }
                        if (tp.Stock < carlist[i].Count)
                        {
                            return (int)ErrorCode.商品数量不足;
                        }
                        tp.Stock -= carlist[i].Count;
                        opl.Price = tp.Price;//单价
                        opl.Cash = tp.RealPrice * p.CashDiscount;
                        opl.SpecID = tp.SPID;
                        opl.SPName = tp.SPName;
                        opl.SPDesc = tp.SPDesc;
                        opl.SPRemark = tp.Remark;
                    }
                    else
                    {
                        if (p.Stock < carlist[i].Count)
                        {
                            return (int)ErrorCode.商品数量不足;
                        }
                        p.Stock -= carlist[i].Count;
                        opl.Price = p.Price;
                        opl.Cash = p.RealPrice * p.CashDiscount;
                    }
                    opl.ShopName = p.ShopID == null ? "" : new ShopBLL().GetShopName(p.ShopID.Value);
                    opl.Count = carlist[i].Count;
                    ol.Price += opl.Price * opl.Count;
                    ol.Cash += opl.Cash * opl.Count;
                    opl.Type = p.Type;
                    opl.CreateTime = DateTime.Now;
                    opl.UpdateTime = DateTime.Now;
                    opl.ShopID = p.ShopID;
                    opl.Status = (int)enOrderStatus.Order;
                    DBContext.OrderProductList.Add(opl);
                }
                DBContext.Cart.RemoveRange(carlist);//删掉购物车中已购买的商品

                return DBContext.SaveChanges();
            }

        }
        /// <summary>
        /// 已收货
        /// </summary>
        /// <param name="OID"></param>
        /// <param name="UID"></param>
        /// <returns></returns>
        public int RecOrder(Guid OPID, Guid? UID = null)
        {
            using (DBContext)
            {
                OrderProductList opl = DBContext.OrderProductList.Where(m => m.ID == OPID && m.Status == (int)enOrderStatus.Sended).FirstOrDefault();
                if (opl == null)
                {
                    throw new Exception("订单不存在");
                }

                var qo = DBContext.OrderList.Where(m => m.ID == opl.OrderID && m.Status == (int)enOrderStatus.Sended);

                OrderList o = qo.FirstOrDefault();
                if (o == null)
                {
                    throw new Exception("状态异常或已处理");
                }
                opl.Status = (int)enOrderStatus.Recieved;
                opl.UpdateTime = DateTime.Now;
                int count = DBContext.OrderProductList.Where(m => m.OrderID == opl.OrderID && m.Status == (int)enOrderStatus.Sended).Count();//查询已发货状态的还有多少
                if (count == 1)
                {
                    o.Status = (int)enOrderStatus.Completed;
                    o.UpdateTime = DateTime.Now;
                    List<OrderProductList> list = DBContext.OrderProductList.Where(m => m.OrderID == o.ID).ToList();
                    foreach (var item in list)
                    {
                        item.Status = (int)enOrderStatus.Completed;
                        item.UpdateTime = DateTime.Now;
                    }
                }
                return DBContext.SaveChanges();
            }
        }
        /// <summary>
        /// 已发收货
        /// </summary>
        /// <param name="OID"></param>
        /// <param name="UID"></param>
        /// <returns></returns>
        public int OrderSend(Guid OPID, Guid? UID = null)
        {
            using (DBContext)
            {
                OrderProductList opl = DBContext.OrderProductList.Where(m => m.ID == OPID && m.Status == (int)enOrderStatus.Payed).FirstOrDefault();
                if (opl == null)
                {
                    throw new Exception("订单不存在");
                }

                var qo = DBContext.OrderList.Where(m => m.ID == opl.OrderID && m.Status == (int)enOrderStatus.Payed);

                OrderList o = qo.FirstOrDefault();
                if (o == null)
                {
                    throw new Exception("状态异常或已处理");
                }
                opl.Status = (int)enOrderStatus.Sended;
                opl.UpdateTime = DateTime.Now;
                int count = DBContext.OrderProductList.Where(m => m.OrderID == opl.OrderID && m.Status == (int)enOrderStatus.Payed).Count();//查询已发货状态的还有多少
                if (count == 1)
                {
                    o.Status = (int)enOrderStatus.Sended;
                    o.UpdateTime = DateTime.Now;
                }
                return DBContext.SaveChanges();
            }
        }
        /// <summary>
        /// 后台获取订单
        /// </summary>
        /// <param name="Number"></param>
        /// <param name="status"></param>
        /// <param name="type"></param>
        /// <param name="beginTime"></param>
        /// <param name="endtime"></param>
        /// <param name="pagesize"></param>
        /// <param name="pageinex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<Models.OrderListModel> GetOrderList(string Number, enOrderStatus? status, enOrderType? type, DateTime? beginTime, DateTime? endtime, int pagesize, int pageinex, out int count, Guid? ShopID = null)
        {
            using (DBContext)
            {
                string countsql = "select Count(opl.ID) from OrderProductList opl left join OrderList o on opl.OrderID = o.ID where 1=1";
                StringBuilder sql = new StringBuilder();
                sql.Append("select ROW_NUMBER() over(order by o.createtime desc) num,o.ID,opl.ID OPID,o.Number,o.CreateTime,opl.Status,o.Price TotalPrice,opl.Price,opl.Name");
                sql.Append(", opl.Img, c.Name as CateName, o.RecName, o.RecPhone, o.RecAddress,opl.Type,opl.Count,o.OrderType");
                sql.Append(" from OrderProductList opl left join OrderList o on opl.OrderID = o.ID");
                sql.Append(" left join Category c on opl.CategoryID = c.ID where 1=1 ");
                StringBuilder wheresql = new StringBuilder();
                if (ShopID != null)
                {
                    wheresql.Append(" and opl.ShopID='" + ShopID + "'");
                }
                if (!string.IsNullOrEmpty(Number))
                {
                    wheresql.Append(" and o.Number like @number");
                }
                else
                {
                    wheresql.Append(" and (1=1 or o.Number like @number)");
                }
                if (status != null)
                {
                    wheresql.Append(" and opl.status=" + (int)status.Value);
                }
                if (type != null)
                {
                    wheresql.Append(" and opl.Type=" + (int)type.Value);
                }
                if (beginTime != null)
                {
                    wheresql.Append(" and o.Createtime>=CONVERT(date,'" + beginTime.Value.ToString("yyyy-MM-dd") + "')");
                }
                if (endtime != null)
                {
                    wheresql.Append(" and o.Createtime<=CONVERT(date,'" + endtime.Value.AddDays(1).ToString("yyyy-MM-dd") + "')");
                }
                count = DBContext.Database.SqlQuery<int>(countsql + wheresql.ToString(), new SqlParameter[] {
                    new SqlParameter("number","%"+Number+"%")
                }).FirstOrDefault();
                return DBContext.Database.SqlQuery<Models.OrderListModel>("select * from (" + sql.ToString() + wheresql.ToString() + ") as temp where temp.num > " + ((pageinex - 1) * pagesize) + " and temp.num <=" + (pageinex * pagesize), new SqlParameter[] {
                    new SqlParameter("number","%"+Number+"%")
                }).ToList();
            }
        }
        /// <summary>
        /// 下单方法
        /// </summary>
        /// <param name="UID">用户id</param>
        /// <param name="PID">商品id</param>
        /// <param name="AddID">收货地址id</param>
        /// <returns></returns>
        public int CreateOrder(Guid UID, Guid OID, Guid AddID)
        {
            using (DBContext)
            {
                var o = DBContext.OtherPayServiceLog.FirstOrDefault(m => m.ID == OID
                && m.OrderType == (int)enPayFrom.UpdateUserLV
                && m.Status == (int)enStatus.Enabled && m.OrderNumber == "");
                if (o == null)
                {
                    throw new Exception("订单不存在或者已被提货。");
                }
                ProductList p = DBContext.ProductList.Where(m => m.ID == o.TOID).FirstOrDefault();
                if (p == null)
                {
                    throw new Exception("商品不存在");
                }
                //PayRecord pr = DBContext.PayRecord.FirstOrDefault(m => m.UID == UID && m.Level == p.Level && m.IsBuyProduct == 0);
                //if (pr == null)
                //{
                //    throw new Exception("没有可兑换的商品");
                //}
                //if (DateTime.Now > pr.CreateTime.AddMonths(1))
                //{
                //    throw new Exception("该兑换券已超过时间限制");
                //}
                RecAddress address = DBContext.RecAddress.Where(m => m.ID == AddID && m.UID == UID).FirstOrDefault();
                if (address == null)
                {
                    throw new Exception("收货地址不正确");
                }
                OrderList order = new OrderList();
                order.ID = Guid.NewGuid();
                order.Number = Common.GetOrderNumer();
                order.UID = UID;
                order.RecName = address.TrueName;
                order.RecAreaID = address.AreaID;
                order.RecAddress = address.Address.Replace(",", "");
                order.RecPhone = address.Phone;
                order.RecSex = address.Sex;
                order.Payment = Common.enPayment.LOCAL.ToString();
                order.PriceType = (int)Common.enPayType.Point;
                order.Price = p.Price;
                order.Fee = 0;
                order.OrderType = (int)Common.enOrderType.OnLine;
                order.Status = (int)Common.enOrderStatus.Payed;
                order.CreateTime = DateTime.Now;
                order.UpdateTime = DateTime.Now;

                OrderProductList opl = new OrderProductList();
                opl.ID = Guid.NewGuid();
                opl.OrderID = order.ID;
                opl.OrderNumber = order.Number;
                opl.ProductID = p.ID;
                opl.Number = p.Number;
                opl.Name = p.Name;
                opl.CategoryID = p.CategoryID;
                opl.Img = p.Img;
                opl.PriceType = (int)Common.enPayType.Point;
                opl.Price = p.Price;
                opl.Count = 1;
                opl.Type = p.Type;
                opl.Status = (int)Common.enOrderStatus.Payed;
                opl.CreateTime = DateTime.Now;
                opl.UpdateTime = DateTime.Now;
                try
                {
                    o.OrderNumber = opl.OrderID.ToString();
                    DBContext.OrderList.Add(order);
                    DBContext.OrderProductList.Add(opl);
                    return DBContext.SaveChanges();
                }
                catch (Exception)
                {

                    throw;
                }

            }
        }

        public List<OrderList> GetOrderList(Guid userid, int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                return GetPagedList(DBContext.OrderList.Where(m => m.UID == userid), pagesize, pageinex, out count);
            }
        }

        public List<vw_Orders> GetOrderList(Guid? userid, string key, enOrderStatus? status, enOrderType? type, DateTime? beginTime, DateTime? endtime
            , enPayment? Payment, enPayType? PriceType, decimal? Price
            , int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                var q = DBContext.vw_Orders.Where(m => m.CategoryID != 1);

                if (userid.HasValue)
                {
                    q = q.Where(m => m.UID == userid);
                }
                if (!string.IsNullOrEmpty(key))
                {
                    q = q.Where(m => m.Number == key || m.proName.Contains(key) || m.proNumber == key || m.Uphone == key || m.UTrueName.Contains(key)
                     || m.Remark.Contains(key) || m.RecName.Contains(key) || m.RecPhone == key);
                }
                if (Price.HasValue)
                {
                    q = q.Where(m => m.Price >= Price.Value);
                }
                if (status.HasValue)
                {
                    q = q.Where(m => m.Status == (int)status.Value);
                }
                if (type.HasValue)
                {
                    q = q.Where(m => m.OrderType == (int)type.Value);
                }
                if (beginTime.HasValue)
                {
                    q = q.Where(m => m.CreateTime >= beginTime.Value);
                }
                if (endtime.HasValue)
                {
                    q = q.Where(m => m.CreateTime <= endtime.Value);
                }
                if (Payment.HasValue)
                {
                    q = q.Where(m => m.Payment == Payment.Value.ToString());
                }
                if (PriceType.HasValue)
                {
                    q = q.Where(m => m.PriceType == (int)PriceType.Value);
                }
                return GetPagedList(q.OrderByDescending(m => m.CreateTime), pagesize, pageinex, out count);
            }
        }

        public List<OrderList> GetAllList()
        {
            using (DBContext)
            {
                return DBContext.OrderList.ToList();
            }
        }

        public OrderList GetDetail(Guid ID)
        {
            using (DBContext)
            {
                return DBContext.OrderList.FirstOrDefault(m => m.ID == ID);
            }
        }

        public OrderList GetDetail(string orderNumber)
        {
            using (DBContext)
            {
                return DBContext.OrderList.FirstOrDefault(m => m.Number == orderNumber);
            }
        }

        public OrderList Insert(Guid? ShopID, Guid UID, Guid addressID, enPayment Payment, enPayType PriceType,
            decimal Fee, enOrderType OrderType, string Remark, Guid ProID, int count)
        {
            using (DBContext)
            {
                //添加订单里的商品
                ProductList pro = new ProductsBLL().GetProduct(ProID);
                if (pro == null || pro.Stock < count)//商品不存在，或者库存不足
                    return null;

                var user = new UsersBLL().GetUserById(UID);
                if (user == null)//人员不存在
                    return null;

                var address = new RecAddressBLL().GetAddressDetail(addressID);
                if (address == null)
                    address = new RecAddress();

                //添加订单
                OrderList model = new OrderList();
                model.ID = Guid.NewGuid();
                model.Number = Common.GetOrderNumer();
                model.ShopID = ShopID;
                model.UID = UID;
                model.RecName = address.TrueName;
                model.RecAreaID = string.IsNullOrEmpty(address.AreaID) ? "" : address.AreaID;
                model.RecAddress = string.IsNullOrEmpty(address.Address) ? "" : address.Address;
                model.RecPhone = address.Phone;
                model.RecAreaCode = address.AreaCode;
                model.RecEmail = address.Email;
                model.RecSex = address.Sex;
                model.Payment = Payment.ToString();
                model.PriceType = (int)PriceType;
                model.Price = (pro.Price * count) + Fee;
                model.Fee = Fee;
                model.OrderType = (int)OrderType;
                model.Remark = Remark;
                model.Status = (int)enOrderStatus.Order;
                model.CreateTime = model.UpdateTime = DateTime.Now;

                OrderProductList orderPro = new OrderProductList();
                orderPro.ID = Guid.NewGuid();
                orderPro.OrderID = model.ID;
                orderPro.OrderNumber = model.Number;
                orderPro.ProductID = pro.ID;
                orderPro.Number = pro.Number;
                orderPro.Name = pro.Name;
                orderPro.CategoryID = pro.CategoryID;
                orderPro.Img = pro.Img;
                orderPro.PriceType = pro.PriceType;
                orderPro.Price = pro.Price;
                orderPro.Count = count;
                orderPro.Type = pro.Type;
                orderPro.BeginTime = pro.BeginTime;
                orderPro.EndTime = pro.EndTime;
                orderPro.Status = pro.Status;
                orderPro.CreateTime = orderPro.UpdateTime = model.CreateTime;

                DBContext.OrderList.Add(model);
                DBContext.OrderProductList.Add(orderPro);
                if (DBContext.SaveChanges() > 0)
                {
                    return model;
                }
                else
                {
                    return null;
                }
            }
        }

        public List<OrderProductList> GetOrderProList(Guid OrderID)
        {
            using (DBContext)
            {
                return DBContext.OrderProductList.Where(m => m.OrderID == OrderID).ToList();
            }
        }

        public int UpdateStatus(Guid ID, enOrderStatus OrderStatus)
        {
            using (DBContext)
            {
                OrderList model = DBContext.OrderList.FirstOrDefault(m => m.ID == ID);
                if (model != null)
                {
                    if (model.Status.Value == (int)enOrderStatus.Cancel)//已被取消的订单
                    {
                        return (int)ErrorCode.状态异常或已处理;
                    }
                    if (OrderStatus == enOrderStatus.Cancel && model.Status != (int)enOrderStatus.Order)//只有下单状态才能取消
                    {
                        return (int)ErrorCode.状态异常或已处理;
                    }
                    if (model.Status >= (int)OrderStatus)
                    {
                        return (int)ErrorCode.状态异常或已处理;
                    }

                    switch (OrderStatus)
                    {
                        case enOrderStatus.Payed:
                            //插入消费记录
                            PayListBLL paybll = new PayListBLL();
                            var user = DBContext.Users.FirstOrDefault(m => m.ID == model.UID);

                            if (model.Payment == enPayment.LOCAL.ToString())//如果是本地支付，则判断金额/积分是否足够
                            {
                                if (model.PriceType == (int)enPayType.Coin)
                                {
                                    if (user.Balance < model.Price)
                                    {
                                        return (int)ErrorCode.账户余额不足;
                                    }
                                    user.Balance -= model.Price;
                                }
                                else
                                {
                                    if (user.Score < model.Price)
                                    {
                                        return (int)ErrorCode.账户积分不足;
                                    }
                                    user.Score -= model.Price;
                                }
                            }

                            //处理库存
                            //判断购买的货物是什么？
                            var order_product = DBContext.OrderProductList.Where(m => m.OrderID == model.ID && m.Status == (int)enStatus.Enabled).ToList();
                            //var order_product = from o in DBContext.OrderProductList
                            //                    join p in DBContext.ProductList on o.ProductID equals p.ID
                            //                    where o.OrderID == model.ID && o.Status == (int)enStatus.Enabled
                            //                    select p;

                            if (order_product == null || order_product.Count() == 0)
                            {
                                return (int)ErrorCode.订单异常;
                            }
                            int card_count = 0;
                            enCardType cardtype = enCardType.普通用户;
                            decimal totalScore = 0;
                            //存在类型为星卡的货物
                            foreach (var tempp in order_product)
                            {
                                if (tempp.CategoryID == 1)
                                {
                                    totalScore += tempp.Price * tempp.Count;
                                    card_count += tempp.Count;
                                    cardtype = tempp.Name == "轻客金卡" ? enCardType.轻客金卡 : enCardType.轻客星卡;
                                }
                            }

                            user.Score += totalScore;
                            user.TotalScore += totalScore;
                            if (user.TotalScore >= 1000)
                            {
                                user.LV = (int)enCardType.轻客金卡;
                            }
                            else if (user.TotalScore >= 500)
                            {
                                user.LV = (int)enCardType.轻客星卡;
                            }
                            else
                            {
                                user.LV = (int)enCardType.普通用户;
                            }
                            //推荐人获得5%奖励
                            var pUser = DBContext.Users.FirstOrDefault(m => m.ID == user.FID);
                            if (pUser != null && pUser.Status == (int)enStatus.Enabled)//推荐人存在，且可用
                            {
                                pUser.Balance += totalScore * (decimal)0.05;

                                string msgcontent = "您的好友" + GetUserShowName(user) + "消费了【" + cardtype + "】，您获得了" + (totalScore * (decimal)0.05).ToString("0.##") + "元奖励。";
                                new UserMsgBLL().Insert(pUser.ID, Guid.Empty, "分享奖励", msgcontent, enMessageType.System, "", msgcontent);
                                paybll.Insert(model.ID, pUser.ID, enPayInOutType.In, (enPayType)model.PriceType, enPayFrom.Reward, totalScore * (decimal)0.05,
                                    "分享人" + GetUserShowName(user) + "消费【" + cardtype + "】");
                            }

                            int rt = DBContext.SaveChanges();

                            if (rt > 0 && card_count > 0)
                            {
                                RedPacksBLL redbll = new RedPacksBLL();
                                redbll.InsertRedQueue(user.ID, cardtype, model.ID, card_count);
                            }

                            if (cardtype != enCardType.普通用户)
                            {
                                paybll.Insert(model.ID, model.UID, enPayInOutType.Out, (enPayType)model.PriceType, enPayFrom.OnLinePay, model.Price, "购买" + cardtype);
                                paybll.Insert(model.ID, model.UID, enPayInOutType.In, enPayType.Point, enPayFrom.OnLinePay, model.Price, "购买" + cardtype);
                            }
                            else
                            {
                                paybll.Insert(model.ID, model.UID, enPayInOutType.Out, (enPayType)model.PriceType, enPayFrom.OnLinePay, model.Price, "线上消费【" + order_product[0].Name + "】");
                            }

                            model.PayTime = DateTime.Now;

                            break;
                        case enOrderStatus.Sended:
                            model.SendTime = DateTime.Now;
                            break;
                        case enOrderStatus.Recieved:
                            model.RecTime = DateTime.Now;
                            break;
                        case enOrderStatus.Completed:
                            model.FinishTime = DateTime.Now;
                            break;
                    }
                    model.Status = (int)OrderStatus;
                    model.UpdateTime = DateTime.Now;

                    return DBContext.SaveChanges();
                }
                else
                    return 0;
            }
        }
    }
}
