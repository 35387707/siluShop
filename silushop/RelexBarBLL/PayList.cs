using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;
using RelexBarBLL.Models;
using System.Data.SqlClient;

namespace RelexBarBLL
{
    public partial class PayListBLL : BaseBll
    {

        public decimal? GetPayPrice(Guid UID, enPayInOutType inout)
        {
            using (DBContext)
            {
                return DBContext.PayList.Where(m => m.UID == UID && m.InOut == (int)inout && m.Remark != "提现申请").Sum(m => (decimal?)m.Val);
            }
        }
        public decimal? GetPayListDetailPrice(Guid UID, DateTime date, enPayListType type)
        {
            DateTime begin = DateTime.Parse(date.ToString("yyyy-MM-dd"));//当前时间
            DateTime end = date.AddDays(1);
            using (DBContext)
            {
                decimal? sum = DBContext.PayListDetail.Where(m => m.UID == UID && m.Type == (int)type && m.CreateTime >= begin && m.CreateTime < end).Sum(m => (decimal?)m.Price);
                return sum == null ? 0 : sum.Value;
            }
        }
        public List<UserPayList> GetUserPayList(int index, int pageSize, Guid UID, int? from, out int sum)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("select * from(select ROW_NUMBER() over(order by temp.CreateTime desc) rnum,temp.* from (");
            sql.Append("select UID,InOut,Val,Remark,CreateTime from PayList where fromto<>9 and UID='" + UID + "'" + (from.HasValue ? (" and fromto=" + from.Value) + " " : " "));
            sql.Append("union all select UID,1,Price,Remark,CreateTime from PayListDetail where UID='" + UID + "') as temp");
            sql.Append(") as temp2 where rnum>" + (index - 1) * pageSize + " and rnum<=" + index * pageSize);
            sum = DBContext.Database.SqlQuery<int>("select COUNT(UID) from (select UID from PayList where fromto <> 9 union all select UID from PayListDetail ) as temp").First();
            using (DBContext)
            {
                return DBContext.Database.SqlQuery<UserPayList>(sql.ToString()).ToList();
            }
        }
        public List<UserPayList> GetUserPayList2(int index, int pageSize, Guid UID, int? from, out int sum)
        {
            sum = 0;
            StringBuilder sql = new StringBuilder();
            if (from == 1)//丝路易物券
            {
                //sql.Append("select * from(select ROW_NUMBER() over(order by temp.CreateTime desc) rnum,temp.* from (");
                //sql.Append("select UID,InOut,Val,Remark,CreateTime from PayList where fromto<>9 and UID='" + UID + "'" + (from.HasValue ? (" and fromto=" + from.Value) + " " : " "));
                //sql.Append("union all select UID,1,Price,Remark,CreateTime from PayListDetail where UID='" + UID + "') as temp");
                //sql.Append(") as temp2 where rnum>" + (index - 1) * pageSize + " and rnum<=" + index * pageSize);
                //sum = DBContext.Database.SqlQuery<int>("select COUNT(UID) from (select UID from PayList where fromto <> 9 union all select UID from PayListDetail ) as temp").First();

                using (DBContext)
                {
                    sql.Append("select * from(select ROW_NUMBER() over(order by temp.CreateTime desc) rnum,temp.* from (");
                    sql.Append("select UID,1 as InOut,MaxMoney as Val,(case when orderid is null then '转让易物券' else '商城购物得易物券' end) as Remark,CreateTime from PayRecord where UID='" + UID + "' ");
                    sql.Append("union all select UID,1,Price,Remark,CreateTime from PayListDetail where UID='" + UID + "') as temp");
                    sql.Append(") as temp2 where rnum>" + (index - 1) * pageSize + " and rnum<=" + index * pageSize);

                    using (DBContext)
                    {
                        sum = DBContext.Database.SqlQuery<int>("select COUNT(UID) from (select UID from PayRecord where  UID='" + UID + "' union all select UID from PayListDetail  where  UID='" + UID + "') as temp").First();
                        return DBContext.Database.SqlQuery<UserPayList>(sql.ToString()).ToList();
                    }
                }
            }
            else if (from == 2)//回馈易物券
            {
                using (DBContext)
                {
                    sql.Append("select * from(select ROW_NUMBER() over(order by temp.CreateTime desc) rnum,temp.* from (");
                    sql.Append("select UID,1 as InOut,Price as Val,Remark,CreateTime from PayListDetail where UID='" + UID + "' and remark not like '转出易物券%' ) as temp");
                    sql.Append(") as temp2 where rnum>" + (index - 1) * pageSize + " and rnum<=" + index * pageSize);

                    using (DBContext)
                    {
                        sum = DBContext.Database.SqlQuery<int>("select COUNT(*) from (select ID from PayListDetail where UID='" + UID + "' and remark not like '转出易物券%') as temp").First();
                        var list = DBContext.Database.SqlQuery<UserPayList>(sql.ToString()).ToList();
                        foreach (var m in list)
                        {
                            m.Val = Math.Round(m.Val * 0.3M, 2);
                            m.Remark = "每日返回馈易物券：" + m.Val;
                        }
                        return list;
                    }
                }
            }
            else if (from == 3)//今日行票
            {
                //sql.Append("select * from(select ROW_NUMBER() over(order by temp.CreateTime desc) rnum,temp.* from (");
                //sql.Append("select UID,InOut,Val,Remark,CreateTime from PayList where  UID='" + UID + "' and (fromto=2 or fromto=8 or fromto=12) and CreateTime >'" + DateTime.Now.ToString("yyyy-MM-dd") + "' ) as temp");
                //sql.Append(") as temp2 where rnum>" + (index - 1) * pageSize + " and rnum<=" + index * pageSize);

                //using (DBContext)
                //{
                //    sum = DBContext.Database.SqlQuery<int>("select COUNT(*) from (select ID from PayList where UID='" + UID + "' and (fromto=2 or fromto=8 or fromto=12) and CreateTime >'" + DateTime.Now.ToString("yyyy-MM-dd") + "') as temp").First();
                //    return DBContext.Database.SqlQuery<UserPayList>(sql.ToString()).ToList();
                //}
                sql.Append("select * from(select ROW_NUMBER() over(order by temp.CreateTime desc) rnum,temp.* from (");
                sql.Append("select UID,InOut,Val,Remark,CreateTime from PayList where  UID='" + UID + "' and (fromto=2 or fromto=8 or fromto=12) ) as temp");
                sql.Append(") as temp2 where rnum>" + (index - 1) * pageSize + " and rnum<=" + index * pageSize);

                using (DBContext)
                {
                    sum = DBContext.Database.SqlQuery<int>("select COUNT(*) from (select ID from PayList where UID='" + UID + "' and (fromto=2 or fromto=8 or fromto=12) ) as temp").First();
                    return DBContext.Database.SqlQuery<UserPayList>(sql.ToString()).ToList();
                }
            }
            else if (from == 4)//行票消费券
            {
                sql.Append("select * from(select ROW_NUMBER() over(order by temp.CreateTime desc) rnum,temp.* from (");
                sql.Append("select UID,InOut,Val,Remark,CreateTime from PayList where  UID='" + UID + "' and payType="+(int)enPayType.KaPoint+" ) as temp");
                sql.Append(") as temp2 where rnum>" + (index - 1) * pageSize + " and rnum<=" + index * pageSize);

                using (DBContext)
                {
                    sum = DBContext.Database.SqlQuery<int>("select COUNT(*) from (select ID from TransferOut where UID='" + UID + "' and status=1 ) as temp").First();
                    return DBContext.Database.SqlQuery<UserPayList>(sql.ToString()).ToList();
                }
            }
            return null;
        }

        /// <summary>
        /// 获取账单
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="FromTo"></param>
        /// <param name="InOut"></param>
        /// <param name="index"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<AdminPayListModel> GetPayList(string phone, enPayFrom? FromTo, enPayInOutType? InOut, DateTime? beginTime, DateTime? endTime, int index, int pageSize, out int sum, Guid? UID = null, string name = null)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("select * from(select  ROW_NUMBER() over(order by p.createtime desc) row_number,p.UID,u.Phone,u.CardNumber,p.CID OUID,o.Phone OPhone,o.Name OName,u.Name,p.InOut,p.FromTo,p.Val,p.Remark,p.CreateTime ");
            sql.Append("from PayList p left join Users u on p.UID=u.ID ");
            sql.Append("left join Users o on p.CID=o.ID ");
            StringBuilder tj = new StringBuilder();
            sql.Append("where 1=1 ");
            if (FromTo != null)
            {
                tj.Append("and p.FromTo=" + (int)FromTo.Value);
            }
            if (InOut != null)
            {
                tj.Append("and p.InOut=" + (int)InOut.Value);
            }
            if (string.IsNullOrEmpty(phone))
            {
                tj.Append(" and (1=1 or u.Phone like @phone) ");
            }
            else
            {
                tj.Append(" and u.Phone like @phone ");
            }
            if (beginTime != null)
            {
                tj.Append(" and p.CreateTime>convert(datetime,'" + beginTime.Value.ToString("yyyy-MM-dd") + "')");
            }
            if (endTime != null)
            {
                tj.Append(" and p.CreateTime<convert(datetime,'" + endTime.Value.AddDays(1).ToString("yyyy-MM-dd") + "')");
            }
            if (UID != null)
            {
                tj.Append(" and p.UID='" + UID.Value + "'");
            }
            if (name == null)
            {
                tj.Append(" and (1=1 or u.Name like @name)");
            }
            else
            {
                tj.Append(" and u.Name like @name");
            }
            string sqlend = " ) as t where t.row_number > @min and t.row_number <= @max";
            using (DBContext)
            {
                sum = DBContext.Database.SqlQuery<int>("select count(p.UID) from PayList p left join Users u on p.UID=u.ID where 1=1 " + tj.ToString(), new SqlParameter[] {
                    new SqlParameter("@phone","%"+phone+"%"),
                    new SqlParameter("@name","%"+name+"%")
                }).FirstOrDefault();
                return DBContext.Database.SqlQuery<AdminPayListModel>(sql.Append(tj).ToString() + sqlend, new SqlParameter[] {
                    new SqlParameter("@phone","%"+phone+"%"),
                    new SqlParameter("@name","%"+name+"%"),
                    new SqlParameter("@min",(index-1)*pageSize),
                    new SqlParameter("@max",index*pageSize)
                }).ToList();
            }
        }
        /// <summary>
        /// 获取当日收益
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public decimal GetDailySalary(Guid UID)
        {
            using (DBContext)
            {
                DateTime date = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));//当前时间
                DateTime end = date.AddDays(1);
                decimal? sum = DBContext.PayList.Where(m => m.CreateTime >= date && m.CreateTime < end && m.UID == UID
                && (m.FromTo == 2 || m.FromTo == 8 || m.FromTo == 10)
                && m.InOut == (int)Common.enPayInOutType.In)
                    .Sum(m => (decimal?)m.Val);
                return sum == null ? 0 : sum.Value;
            }
        }
        public List<PayList> GetPayList(Guid userid, DateTime? date, enPayFrom? FromTo, enPayInOutType? InOut, enPayType? PayType
            , int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                var q = DBContext.PayList.Where(m => m.UID == userid);
                if (FromTo.HasValue)
                {
                    q = q.Where(m => m.FromTo == (int)FromTo);
                }
                if (date.HasValue)
                {
                    DateTime begin, end;
                    begin = DateTime.Parse(date.Value.ToString("yyyy-MM-01"));
                    end = DateTime.Parse(date.Value.AddMonths(1).ToString("yyyy-MM-01"));

                    q = q.Where(m => m.CreateTime.Value > begin && m.CreateTime.Value < end);
                }
                if (InOut.HasValue)
                {
                    q = q.Where(m => m.InOut == (int)InOut);
                }
                if (PayType.HasValue)
                {
                    q = q.Where(m => m.PayType == (int)PayType);
                }
                return GetPagedList(q.OrderByDescending(m => m.ID), pagesize, pageinex, out count);
            }
        }

        public decimal TotalPays(Guid userid, DateTime? date, enPayFrom? FromTo, enPayInOutType? InOut, enPayType? PayType)
        {
            using (DBContext)
            {
                var q = DBContext.PayList.Where(m => m.UID == userid);
                if (FromTo.HasValue)
                {
                    q = q.Where(m => m.FromTo == (int)FromTo);
                }
                if (date.HasValue)
                {
                    DateTime begin, end;
                    begin = DateTime.Parse(date.Value.ToString("yyyy-MM-01"));
                    end = DateTime.Parse(date.Value.AddMonths(1).ToString("yyyy-MM-01"));

                    q = q.Where(m => m.CreateTime.Value > begin && m.CreateTime.Value < end);
                }
                if (InOut.HasValue)
                {
                    q = q.Where(m => m.InOut == (int)InOut);
                }
                if (PayType.HasValue)
                {
                    q = q.Where(m => m.PayType == (int)PayType);
                }
                decimal? total = q.Sum(m => (decimal?)m.Val);

                return total.HasValue ? total.Value : 0;
            }
        }

        public PayList Details(int? ID, Guid? UID, Guid? CID)
        {
            using (DBContext)
            {
                var q = DBContext.PayList.AsEnumerable();
                if (ID.HasValue)
                {
                    q = q.Where(m => m.ID == ID.Value);
                }
                if (UID.HasValue)
                {
                    q = q.Where(m => m.UID == UID.Value);
                }
                if (CID.HasValue)
                {
                    q = q.Where(m => m.CID == CID.Value);
                }
                return q.FirstOrDefault();
            }
        }

        /// <summary>
        /// 线下支付消费（没有订单，直接给商家增加金额）
        /// </summary>
        /// <returns></returns>
        public int PayForOutline(Guid UID, Guid? ShopID, Guid CID, enPayment Payment, decimal Val)
        {
            using (DBContext)
            {
                var user = DBContext.Users.FirstOrDefault(m => m.ID == UID);
                if (user == null)
                {
                    return (int)ErrorCode.账号不存在;
                }
                if (user.UserType != (int)enUserType.User)
                {
                    return (int)ErrorCode.账号类型不正确;
                }
                if (user.Status == (int)enStatus.Unabled)
                {
                    return (int)ErrorCode.账号不可用;
                }
                //支付金额到谁手里，有可能到平台上
                Users shoper = null;
                if (ShopID.HasValue)
                {
                    shoper = DBContext.Users.FirstOrDefault(m => m.ID == ShopID);
                }

                if (shoper != null)
                {
                    shoper.Balance += Val;
                }

                PayList model = new PayList();
                model.CID = CID;
                model.UID = UID;
                model.InOut = (int)enPayInOutType.Out;
                model.PayType = (int)enPayType.Coin;
                model.FromTo = (int)enPayFrom.OutLinePay;
                model.Val = Val;
                model.Remark = "微信消费金额：" + Val;
                model.Status = (int)enStatus.Enabled;
                model.CreateTime = model.UpdateTime = DateTime.Now;
                DBContext.PayList.Add(model);

                if (shoper != null)
                {
                    PayList model2 = new PayList();
                    model2.CID = CID;
                    model2.UID = ShopID;
                    model2.InOut = (int)enPayInOutType.In;
                    model2.PayType = (int)enPayType.Coin;
                    model2.FromTo = (int)enPayFrom.OutLinePay;
                    model2.Val = Val;
                    model2.Remark = "微信收款：" + Val + "--" + user.Phone;
                    model2.Status = (int)enStatus.Enabled;
                    model2.CreateTime = model2.UpdateTime = DateTime.Now;
                    DBContext.PayList.Add(model2);
                }

                return DBContext.SaveChanges() > 0 ? 1 : (int)ErrorCode.数据库操作失败;
            }
        }

        /// <summary>
        /// 本地支付消费（没有订单，直接消费扣除金额）
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="PayType"></param>
        /// <param name="Val"></param>
        /// <returns></returns>
        //public int PayForLocal(Guid UID, Guid? ShopID, Guid CID, enPayType PayType, decimal Val)
        //{
        //    using (DBContext)
        //    {
        //        var user = DBContext.Users.FirstOrDefault(m => m.ID == UID);
        //        if (user == null)
        //        {
        //            return (int)ErrorCode.账号不存在;
        //        }
        //        if (user.UserType != (int)enUserType.User)
        //        {
        //            return (int)ErrorCode.账号类型不正确;
        //        }
        //        if (user.Status == (int)enStatus.Unabled)
        //        {
        //            return (int)ErrorCode.账号不可用;
        //        }
        //        //支付金额到谁手里，有可能到平台上
        //        Users shoper = null;
        //        if (ShopID.HasValue)
        //        {
        //            shoper = DBContext.Users.FirstOrDefault(m => m.ID == ShopID);
        //        }
        //        if (user.LV > (int)enCardType.普通用户 && PayType != enPayType.KaPoint)//大于普通用户,则打折（活动？），充值卡消费不参与折扣
        //        {
        //            var discount = SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "Discount");
        //            decimal zhekou = 90;
        //            if (discount != null)
        //            {
        //                decimal.TryParse(discount.Value, out zhekou);
        //            }
        //            Val = Val * zhekou / 100;//折扣价
        //        }

        //        if (shoper != null)
        //        {
        //            if (PayType == enPayType.Coin)
        //            {
        //                if (user.Balance < Val)
        //                {
        //                    return (int)ErrorCode.账户余额不足;
        //                }
        //                user.Balance -= Val;
        //                shoper.Balance += Val;
        //            }
        //            else if(PayType == enPayType.Point)
        //            {
        //                if (user.Score < Val)
        //                {
        //                    return (int)ErrorCode.账户积分不足;
        //                }
        //                user.Score -= Val;
        //                shoper.Score += Val;
        //                shoper.TotalScore += Val;
        //            }
        //            else
        //            {
        //                if (user.KaPoint < Val)
        //                {
        //                    return (int)ErrorCode.账户卡积分不足;
        //                }
        //                user.KaPoint -= Val;
        //                shoper.Score += Val;
        //                shoper.TotalScore += Val;
        //            }
        //        }
        //        else
        //        {

        //            if (PayType == enPayType.Coin)
        //            {
        //                if (user.Balance < Val)
        //                {
        //                    return (int)ErrorCode.账户余额不足;
        //                }
        //                user.Balance -= Val;
        //            }
        //            else if (PayType == enPayType.Point)
        //            {
        //                if (user.Score < Val)
        //                {
        //                    return (int)ErrorCode.账户积分不足;
        //                }
        //                user.Score -= Val;
        //            }
        //            else
        //            {
        //                if (user.KaPoint < Val)
        //                {
        //                    return (int)ErrorCode.账户卡积分不足;
        //                }
        //                user.KaPoint -= Val;
        //            }
        //        }

        //        PayList model = new PayList();
        //        model.CID = CID;
        //        model.UID = UID;
        //        model.InOut = (int)enPayInOutType.Out;
        //        model.PayType = (int)PayType;
        //        model.FromTo = (int)enPayFrom.OutLinePay;
        //        model.Val = Val;
        //        model.Remark = "线下消费金额：" + Val;
        //        model.Status = (int)enStatus.Enabled;
        //        model.CreateTime = model.UpdateTime = DateTime.Now;
        //        DBContext.PayList.Add(model);

        //        if (shoper != null)
        //        {
        //            PayList model2 = new PayList();
        //            model2.CID = CID;
        //            model2.UID = ShopID;
        //            model2.InOut = (int)enPayInOutType.In;
        //            model2.PayType = (int)PayType;
        //            model2.FromTo = (int)enPayFrom.OutLinePay;
        //            model2.Val = Val;
        //            model2.Remark = "线下收款：" + Val + "--" + user.Phone;
        //            model2.Status = (int)enStatus.Enabled;
        //            model2.CreateTime = model2.UpdateTime = DateTime.Now;
        //            DBContext.PayList.Add(model2);
        //        }

        //        return DBContext.SaveChanges() > 0 ? 1 : (int)ErrorCode.数据库操作失败;
        //    }
        //}

        public int Insert(Guid? CID, Guid? UID, enPayInOutType InOut, enPayType PayType, enPayFrom FromTo, decimal Val, string Remark)
        {
            using (DBContext)
            {
                PayList model = new PayList();
                model.CID = CID;
                model.UID = UID;
                model.InOut = (int)InOut;
                model.PayType = (int)PayType;
                model.FromTo = (int)FromTo;
                model.Val = Val;
                model.Remark = Remark;
                model.Status = (int)enStatus.Enabled;
                model.CreateTime = model.UpdateTime = DateTime.Now;

                DBContext.PayList.Add(model);
                return DBContext.SaveChanges();
            }
        }

        public int Delete(int id)
        {
            using (DBContext)
            {
                var pay = DBContext.PayList.FirstOrDefault(m => m.ID == id);
                if (pay != null)
                {
                    pay.Status = (int)enStatus.Unabled;
                    return DBContext.SaveChanges();
                }
                return 0;
            }
        }

        public dynamic GetPayList(string key, DateTime? bdate, DateTime? edate, enUserType? UserType, enPayFrom? FromTo,
            enPayInOutType? InOut, enPayType? PayType, int pagesize, int pageinex, out int count, out decimal totalprice)
        {
            using (DBContext)
            {
                var q = from a in DBContext.Users
                        from b in DBContext.PayList
                        where a.ID == b.UID
                        select new
                        {
                            UID = a.ID,
                            Phone = a.Phone,
                            CardNumber = a.CardNumber,
                            TrueName = a.TrueName,
                            Score = a.Score,
                            Balance = a.Balance,
                            Status = a.Status,
                            UserType = a.UserType,
                            LV = a.LV,
                            ID = b.ID,
                            CID = b.CID,
                            InOut = b.InOut,
                            PayType = b.PayType,
                            FromTo = b.FromTo,
                            Val = b.Val,
                            Remark = b.Remark,
                            CreateTime = b.CreateTime,
                            UpdateTime = b.UpdateTime,
                        };

                if (!string.IsNullOrEmpty(key))
                {
                    q = q.Where(m => m.TrueName.Contains(key) || m.Phone == key || m.CardNumber.Contains(key) || m.Remark.Contains(key));
                }
                if (FromTo.HasValue)
                {
                    q = q.Where(m => m.FromTo == (int)FromTo);
                }
                if (bdate.HasValue)
                {
                    q = q.Where(m => m.CreateTime.Value > bdate.Value);
                }
                if (edate.HasValue)
                {
                    q = q.Where(m => m.CreateTime.Value < edate.Value);
                }
                if (InOut.HasValue)
                {
                    q = q.Where(m => m.InOut == (int)InOut);
                }
                if (PayType.HasValue)
                {
                    q = q.Where(m => m.PayType == (int)PayType);
                }
                if (UserType.HasValue)
                {
                    q = q.Where(m => m.UserType == (int)UserType);
                }
                totalprice = 0;
                decimal? inmoney = q.Where(m => m.InOut == (int)enPayInOutType.In).Sum(m => (decimal?)m.Val);//入账数
                decimal? outmoney = q.Where(m => m.InOut == (int)enPayInOutType.Out).Sum(m => (decimal?)m.Val);//出账数
                totalprice = (inmoney.HasValue ? inmoney.Value : 0) - (outmoney.HasValue ? outmoney.Value : 0);

                return GetPagedList(q.OrderByDescending(m => m.ID), pagesize, pageinex, out count);
            }
        }
    }
}
