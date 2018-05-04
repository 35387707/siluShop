using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;

namespace RelexBarBLL
{
    public partial class TransferOutBLL : BaseBll
    {
        public decimal? GetAllTransforout(Guid UID, int status)
        {
            using (DBContext)
            {
                return DBContext.TransferOut.Where(m => m.UID == UID).Sum(m => (decimal?)m.Price);
            }
        }

        public List<Models.TransferOutModel> GetList(string key, enApplyStatus? Status, DateTime? begin, DateTime? end
            , int pagesize, int pageinex, out int count, Guid? UID)
        {
            using (DBContext)
            {
                var q = from u1 in DBContext.Users
                        from b in DBContext.TransferOut
                        where u1.ID == b.UID
                        orderby b.CreateTime, b.Status
                        select new Models.TransferOutModel
                        {
                            UID = b.UID.Value,
                            Name = u1.Name,
                            CardNumber = u1.CardNumber,
                            TrueName = u1.TrueName,
                            Phone = u1.Phone,
                            UserType = u1.UserType,
                            RealCheck = u1.RealCheck,

                            ID = b.ID,
                            BankName = b.BankName,
                            BankZhiHang = b.BankZhiHang,
                            BankAccount = b.BankAccount,
                            BankUser = b.BankUser,
                            Price = b.Price,
                            ComPrice = b.ComPrice,
                            Reason = b.Reason,
                            ApplyRemark = b.ApplyRemark,
                            Status = b.Status,
                            CreateTime = b.CreateTime,
                            UpdateTime = b.UpdateTime,
                        };
                if (UID != null)
                {
                    q = q.Where(m => m.UID == UID.Value);
                }
                if (!string.IsNullOrWhiteSpace(key))
                {
                    q = q.Where(m => m.Name == key || m.TrueName.Contains(key) || m.Phone.Contains(key)
                      || m.CardNumber == key);
                }
                if (Status.HasValue)
                {
                    q = q.Where(m => m.Status == (int)Status);
                }
                if (begin.HasValue)
                {
                    q = q.Where(m => m.CreateTime > begin);
                }
                if (end.HasValue)
                {
                    q = q.Where(m => m.CreateTime < end);
                }

                return GetPagedList(q.OrderByDescending(m => m.CreateTime), pagesize, pageinex, out count);
            }
        }
        public List<TransferOut> GetUserList(Guid Uid)
        {
            using (DBContext)
            {
                return DBContext.TransferOut.Where(m => m.UID == Uid).ToList();
            }
        }

        public TransferOut GetDetail(Guid ID)
        {
            using (DBContext)
            {
                return DBContext.TransferOut.FirstOrDefault(m => m.ID == ID);
            }
        }

        public int ApplyTransferOut(Guid Uid, Guid BankID, decimal money, string reason)
        {
            using (DBContext)
            {
                var bank = DBContext.BankList.FirstOrDefault(m => m.ID == BankID);
                if (bank == null)
                    throw new Exception("银行卡不存在");
                var user = DBContext.Users.FirstOrDefault(m => m.ID == Uid);
                if (user == null)
                    throw new Exception("账号不存在");
                if (user.Status != (int)enStatus.Enabled)
                    throw new Exception("账号不可用");
                //if (user.Score < money)
                //    throw new Exception("账户余额不足");

                //user.Score -= money;
                if (user.Balance < money)
                    throw new Exception("账户余额不足");

                user.Balance -= money;
                //user.ShoppingVoucher += money * 0.1M;//1% 去到消费券，（释放的时候自动转到消费券）

                TransferOut model = new TransferOut();
                model.ID = Guid.NewGuid();
                model.UID = Uid;
                model.Reason = reason;
                model.Price = money;
                //model.ComPrice = money * SysConfigBLL.Transout;
                model.ComPrice = 0;//手续费每日释放已经扣除
                model.BankName = bank.BankName;
                model.BankZhiHang = bank.BankZhiHang;
                model.BankAccount = bank.BankAccount;
                model.BankUser = bank.BankUser;
                model.Status = (int)enApplyStatus.Normal;
                model.CreateTime = model.UpdateTime = DateTime.Now;

                DBContext.TransferOut.Add(model);
                int result = DBContext.SaveChanges();
                if (result > 0)
                {
                    PayListBLL paybll = new PayListBLL();
                    paybll.Insert(model.ID, Uid, enPayInOutType.Out, enPayType.Coin, enPayFrom.Transfor, money, "提现申请");
                    //paybll.Insert(model.ID, Uid, enPayInOutType.In, enPayType.KaPoint, enPayFrom.Transfor, money * 0.1M, "提现得10%消费券");
                }
                return result;
            }
        }

        public int UpdateStatus(Guid ID, enApplyStatus status, string remark, decimal comprice)
        {
            using (DBContext)
            {
                var model = DBContext.TransferOut.FirstOrDefault(m => m.ID == ID);
                if (model.Status != (int)enApplyStatus.Normal)//
                {
                    return (int)ErrorCode.状态异常或已处理;
                }
                model.Status = (int)status;
                model.UpdateTime = DateTime.Now;
                if (status == enApplyStatus.Faild)
                {
                    model.ApplyRemark = remark;

                    PayList paylist = new PayList();
                    paylist.CID = model.ID;
                    paylist.UID = model.UID;
                    paylist.InOut = (int)enPayInOutType.In;
                    paylist.PayType = (int)enPayType.Point;
                    paylist.FromTo = (int)enPayFrom.Transfor;
                    paylist.Val = model.Price;
                    paylist.Remark = "提现失败，金额已退还！失败原因：" + remark;
                    paylist.Status = 1;
                    paylist.CreateTime = paylist.UpdateTime = DateTime.Now;
                    DBContext.PayList.Add(paylist);

                }
                else
                {
                    if (comprice > 0)
                        model.ComPrice = comprice;
                }

                var user = DBContext.Users.FirstOrDefault(m => m.ID == model.UID);
                if (user != null)
                {
                    string msgcontent = string.Empty;
                    if (status == enApplyStatus.Success)
                    {
                        //user.ShoppingVoucher += model.Price * 0.1M;//10% 返回购物券
                        msgcontent = "您的提现申请【金额" + model.Price + "，实际到账" + (model.Price - model.ComPrice) + "】已审批通过,请注意您的银行卡账单信息。";
                    }
                    else
                    {
                        user.Balance += model.Price;
                        msgcontent = "您的提现申请【金额" + model.Price + "】已被拒绝,原因：" + remark;
                        new UserMsgBLL().Insert(user.ID, Guid.Empty, "提现通知", msgcontent, enMessageType.System, "", msgcontent);
                    }
                }

                return DBContext.SaveChanges();
            }
        }

        public dynamic GetList(string key, decimal? price, enApplyStatus? Status, DateTime? begin, DateTime? end
            , int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                var q = from u1 in DBContext.Users
                        from b in DBContext.TransferOut
                        where u1.ID == b.UID
                        orderby b.CreateTime, b.Status
                        select new
                        {
                            Name = u1.Name,
                            CardNumber = u1.CardNumber,
                            TrueName = u1.TrueName,
                            Phone = u1.Phone,
                            UserType = u1.UserType,
                            RealCheck = u1.RealCheck,

                            ID = b.ID,
                            BankName = b.BankName,
                            BankZhiHang = b.BankZhiHang,
                            BankAccount = b.BankAccount,
                            BankUser = b.BankUser,
                            Price = b.Price,
                            ComPrice = b.ComPrice,
                            Reason = b.Reason,
                            ApplyRemark = b.ApplyRemark,
                            Status = b.Status,
                            CreateTime = b.CreateTime,
                            UpdateTime = b.UpdateTime,
                        };

                if (!string.IsNullOrWhiteSpace(key))
                {
                    q = q.Where(m => m.Name == key || m.TrueName.Contains(key) || m.Phone.Contains(key)
                      || m.CardNumber == key);
                }
                if (price.HasValue)
                {
                    q = q.Where(m => m.Price > price.Value);
                }
                if (Status.HasValue)
                {
                    q = q.Where(m => m.Status == (int)Status);
                }
                if (begin.HasValue)
                {
                    q = q.Where(m => m.CreateTime > begin);
                }
                if (end.HasValue)
                {
                    q = q.Where(m => m.CreateTime < end);
                }

                return GetPagedList(q, pagesize, pageinex, out count);
            }
        }
    }
}
