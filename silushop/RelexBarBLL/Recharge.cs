using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;

namespace RelexBarBLL
{
    public partial class RechargeBLL : BaseBll
    {
        /// <summary>
        /// 账号充值
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        public int InsertBalance(Guid UID, decimal money)
        {
            using (DBContext)
            {
                var user = DBContext.Users.FirstOrDefault(m => m.ID == UID);
                if (user == null)
                    return (int)ErrorCode.账号不存在;
                user.Balance += money;
                int result = DBContext.SaveChanges();
                if (result > 0)
                {
                    PayListBLL paybll = new PayListBLL();
                    paybll.Insert(null, UID, enPayInOutType.In, enPayType.Coin, enPayFrom.Recharge, money, "充值");
                }
                return result;
            }
        }

        /// <summary>
        /// 转入积分
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        public int ExchangeToScore(Guid UID, decimal money)
        {
            using (DBContext)
            {
                var user = DBContext.Users.FirstOrDefault(m => m.ID == UID);
                if (user == null)
                    return (int)ErrorCode.账号不存在;
                if (user.Balance < money)
                {
                    return (int)ErrorCode.账户余额不足;
                }
                user.Balance -= money;
                user.Score += money;
                user.TotalScore += money;
                int result = DBContext.SaveChanges();
                if (result > 0)
                {
                    PayListBLL paybll = new PayListBLL();
                    paybll.Insert(null, UID, enPayInOutType.Out, enPayType.Coin, enPayFrom.Exchange, money, "转出金额");
                    paybll.Insert(null, UID, enPayInOutType.In, enPayType.Point, enPayFrom.Exchange, money, "转入积分");
                }
                return result;
            }
        }

        /// <summary>
        /// 转账给他人
        /// </summary>
        /// <returns></returns>
        public int ExchangeToOther(Guid FromUID, Guid ToUID, decimal money, enPayType PayType)
        {
            using (DBContext)
            {
                var touser = DBContext.Users.FirstOrDefault(m => m.ID == ToUID);
                if (touser == null)
                    return (int)ErrorCode.账号不存在;
                if (touser.Status == (int)enStatus.Unabled)
                    return (int)ErrorCode.账号不可用;

                var user = DBContext.Users.FirstOrDefault(m => m.ID == FromUID);
                if (user == null)
                    return (int)ErrorCode.账号不存在;

                decimal outMoney, getMoney;//支付金额，获得金额
                decimal sxf = money * SysConfigBLL.Poundage;
                sxf = sxf > 2 ? sxf : 2;//转账他人手续费

                outMoney = money;
                getMoney = money - sxf;

                if (PayType == enPayType.Coin)
                {
                    if (user.Balance < outMoney)
                        return (int)ErrorCode.账户余额不足;

                    user.Balance -= outMoney;
                    touser.Balance += getMoney;
                }
                if (PayType == enPayType.Point)
                {
                    if (user.Score < outMoney)
                        return (int)ErrorCode.账户积分不足;

                    user.Score -= outMoney;
                    touser.Score += getMoney;
                    touser.TotalScore += getMoney;
                }

                int result = DBContext.SaveChanges();
                if (result > 0)
                {
                    string paytype = (PayType == enPayType.Coin ? "金额" : "积分");
                    PayListBLL paybll = new PayListBLL();
                    UserMsgBLL msgbll = new UserMsgBLL();
                    paybll.Insert(null, FromUID, enPayInOutType.Out, PayType, enPayFrom.Exchange, outMoney, "转出" + paytype + "到" + GetUserShowName(touser));
                    paybll.Insert(null, ToUID, enPayInOutType.In, PayType, enPayFrom.Exchange, getMoney, GetUserShowName(user) + "转入" + paytype);

                    string msgcontent = "您已成功转出" + paytype + outMoney + "给" + GetUserShowName(touser) + ",实际到账" + getMoney + "(手续费" + (outMoney - getMoney) + "。";
                    msgbll.Insert(user.ID, Guid.Empty, "转账成功", msgcontent, enMessageType.System, "", msgcontent);
                    msgcontent = GetUserShowName(user) + "转出了" + paytype + outMoney + "给您" + ",实际到账" + getMoney + "(手续费" + (outMoney - getMoney) + ")。";
                    msgbll.Insert(touser.ID, Guid.Empty, "转账成功", msgcontent, enMessageType.System, "", msgcontent);
                }
                return result;
            }
        }


        /// <summary>
        /// 充值卡充值
        /// </summary>
        /// <returns></returns>
        public int RechargeByCard(Guid UID, string cardnum, string cardpsw)
        {
            using (DBContext)
            {
                var model = DBContext.RechargeCard.FirstOrDefault(m => m.CardNum == cardnum && m.CardPsw == cardpsw);
                if (model == null)
                    return (int)ErrorCode.充值卡不存在;
                if (model.Status == (int)enMessageState.Unabled)
                    return (int)ErrorCode.充值卡已失效;
                if (model.Status == (int)enMessageState.HadRead)
                    return (int)ErrorCode.充值卡已被使用;

                model.Status = (int)enMessageState.HadRead;
                model.UpdateTime = DateTime.Now;
               // model.UseID = UID;//使用人

                var user = DBContext.Users.FirstOrDefault(m => m.ID == UID);
                if (user == null)
                {
                    return (int)ErrorCode.账号不存在;
                }
                if (user.Status != (int)enStatus.Enabled)
                {
                    return (int)ErrorCode.账号不可用;
                }

               // user.KaPoint += model.Val;//单独分开卡分充值

                int result = DBContext.SaveChanges();
                if (result > 0)
                {
                    PayListBLL paybll = new PayListBLL();
                    paybll.Insert(null, UID, enPayInOutType.In, (enPayType)model.ValType, enPayFrom.Recharge, model.Val, "充值卡充值");
                }

                return result;
            }
        }

        /// <summary>
        /// 生成充值卡卡号卡密
        /// </summary>
        /// <param name="cardtype">充值卡类型</param>
        /// <param name="priceType">金额类型（积分还是钱）</param>
        /// <param name="price">金额</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        public int CreateRechargeCard(enCardType cardtype, enPayType priceType, decimal price, int count, string remark)
        {
            using (DBContext)
            {
                for (int i = 0; i < count; i++)
                {
                    RechargeCard model = new RechargeCard();
                    model.CardNum = Guid.NewGuid().ToString("N").Substring(0, 10);
                    model.CardPsw = Guid.NewGuid().ToString().Substring(0, 6);
                    model.CardType = (int)cardtype;
                    model.Val = price;
                    model.ValType = (int)priceType;
                    model.Status = (int)enMessageState.Enabled;
                    model.CreateTime = model.UpdateTime = DateTime.Now;

                    DBContext.RechargeCard.Add(model);
                }

                return DBContext.SaveChanges();
            }
        }

        /// <summary>
        /// 注销该卡
        /// </summary>
        /// <param name="CardID"></param>
        /// <returns></returns>
        public int DisposeCard(int CardID)
        {
            using (DBContext)
            {
                var model = DBContext.RechargeCard.FirstOrDefault(m => m.ID == CardID);
                if (model == null)
                    return (int)ErrorCode.充值卡不存在;
                if (model.Status == (int)enMessageState.Unabled)
                    return (int)ErrorCode.充值卡已失效;
                if (model.Status == (int)enMessageState.HadRead)
                    return (int)ErrorCode.充值卡已被使用;

                model.Status = (int)enMessageState.Unabled;
                model.UpdateTime = DateTime.Now;

                return DBContext.SaveChanges();
            }
        }

        /// <summary>
        /// 获取充值卡列表
        /// </summary>
        /// <param name="status"></param>
        /// <param name="price"></param>
        /// <param name="pagesize"></param>
        /// <param name="pageinex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<RechargeCard> ReCardList(Guid? UID, enMessageState? status, decimal? price, int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                var q = DBContext.RechargeCard.AsQueryable();
                //if (UID.HasValue)
                //{
                //    q = q.Where(m => m.UseID == UID.Value);
                //}
                if (status.HasValue)
                {
                    q = q.Where(m => m.Status == (int)status.Value);
                }
                if (price.HasValue)
                {
                    q = q.Where(m => m.Val == price.Value);
                }

                return GetPagedList(q.OrderByDescending(m => m.ID), pagesize, pageinex, out count);
            }
        }
    }
}
