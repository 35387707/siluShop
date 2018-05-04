using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;

namespace RelexBarBLL
{
    public partial class RedPacksBLL : BaseBll
    {
        int _PerDuilieCount = 4;
        int _PerSmallCount = 4;

        public dynamic GetAllReds(Guid? UID, string key, int? enRedStatus, out decimal totalmoney, int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                count = 0;
                totalmoney = 0;

                var q = from a in DBContext.Users
                        from b in DBContext.RedPacketList
                        where a.ID == b.UID
                        orderby b.C_index descending
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

                            Money = b.Money,
                            RStatus = b.Status,
                            RedType = b.RedType,

                            CreateTime = b.CreateTime,
                            UpdateTime = b.UpdateTime,
                        };

                if (UID.HasValue)
                {
                    q = q.Where(m => m.UID == UID.Value);
                }
                if (string.IsNullOrEmpty(key))
                {
                    q = q.Where(m => m.Phone.Contains(key) || m.TrueName.Contains(key));
                }
                if (enRedStatus.HasValue)
                {
                    q = q.Where(m => m.RStatus == enRedStatus.Value);
                }

                decimal? total = q.Sum(m => (decimal?)m.Money);
                if (total.HasValue)
                    totalmoney = total.Value;

                return GetPagedList(q, pagesize, pageinex, out count);
            }
        }

        public List<RedPacket> GetUserReds(Guid Uid, Guid ZGCardID, out int GetRedOrderID)
        {
            using (DBContext)
            {
                GetRedOrderID = -1;

                var q = DBContext.RedPacket.Where(m => m.UID == Uid && m.ZGCardID == ZGCardID).OrderBy(m => m.C_index);
                //获取资格卡领取排队
                //var f = q.FirstOrDefault(m => m.Status == (int)enPacketStatus.Actived);
                //if (f != null)
                //{
                //    var t = DBContext.RedPacketList.OrderBy(m => m.C_index).FirstOrDefault(m => m.RID == f.RID && m.LV == (int)enPacketLV.First && m.Status == (int)enPacketStatus.NoActive);
                //    if (t != null)
                //    {
                //        GetRedOrderID = DBContext.RedPacketList.Count(m => m.RedType == t.RedType && m.LV == (int)enPacketLV.First
                //        && m.Status == (int)enPacketStatus.NoActive && m.C_index < f.C_index);
                //    }
                //    t = DBContext.RedPacketList.OrderBy(m => m.C_index).FirstOrDefault(m => m.RID == f.RID && m.LV == (int)enPacketLV.Second && m.Status == (int)enPacketStatus.NoActive);
                //    if (t != null)
                //    {
                //        int smallcount = DBContext.RedPacketList.Count(m => m.RedType == t.RedType
                //         && m.LV == (int)(int)enPacketLV.Second
                //        && m.Status == (int)enPacketStatus.NoActive && m.C_index < t.C_index);
                //        if (GetRedOrderID > -1)
                //        {
                //            GetRedOrderID = smallcount > GetRedOrderID ? GetRedOrderID : smallcount;
                //        }
                //        else
                //        {
                //            GetRedOrderID = smallcount;
                //        }
                //    }
                //}

                return q.ToList();
            }
        }

        public List<RedPacket> GetUserReds(Guid Uid, enCardType? cardtype)
        {
            using (DBContext)
            {
                string sql = string.Empty;
                if (cardtype.HasValue)
                {
                    sql = @"select ZGCardID,newid() as RID, UID," + (int)cardtype + @" as RedType,min(_index) as C_index,
                                 null as BelongID,null as BeginTime,null as EndTime,null as CreateTime,null as UpdateTime,
                                (select COUNT(1) from RedPacket b where b.ZGCardID = a.ZGCardID and Status = 3 ) as [status] 
                                from RedPacket a
                                where uid = '" + Uid + "' and RedType = " + (int)cardtype + @"
                                group by ZGCardID,UID";
                }
                else
                {
                    sql = @"select ZGCardID,newid() as RID, UID, RedType,min(_index) as C_index,
                                 null as BelongID,null as BeginTime,null as EndTime,CreateTime as CreateTime,null as UpdateTime,
                                (select COUNT(1) from RedPacket b where b.ZGCardID = a.ZGCardID and Status = 3 ) as [status] 
                                from RedPacket a
                                where uid = '" + Uid + "' group by ZGCardID,UID,RedType,CreateTime";
                }
                return DBContext.RedPacket.SqlQuery(sql).OrderBy(m => m.C_index).ToList();
            }
        }

        public List<RedPacket> GetUserReds(Guid Uid, enCardType cardtype, int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                count = 0;
                var q = DBContext.RedPacket.Where(m => m.UID == Uid && m.RedType == (int)cardtype).OrderBy(m => m.C_index);

                return GetPagedList(q, pagesize, pageinex, out count);
            }
        }

        public List<RedPacketList> GetUserRedPacks(Guid Uid, int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                count = 0;
                var q = DBContext.RedPacketList.Where(m => m.UID == Uid).OrderBy(m => m.C_index);

                return GetPagedList(q, pagesize, pageinex, out count);
            }
        }

        public List<RedPacketList> GetUserRedPackByRID(Guid RID, enPacketLV packetLv, out int GetRedOrderID)
        {
            using (DBContext)
            {
                GetRedOrderID = -1;
                var q = DBContext.RedPacketList.Where(m => m.RID == RID && m.LV == (int)packetLv).OrderBy(m => m.C_index).ToList();
                var f = q.FirstOrDefault(m => m.Status == (int)enPacketStatus.NoActive);
                if (f != null)
                {
                    GetRedOrderID = DBContext.RedPacketList.Count(m => m.RedType == f.RedType && m.LV == (int)packetLv && m.Status == (int)enPacketStatus.NoActive && m.C_index < f.C_index);
                }
                return q;
            }
        }

        public List<RedPacketList> GetUserRedPackByRID(enPacketStatus status, int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                count = 0;
                var q = DBContext.RedPacketList.Where(m => m.Status == (int)status).OrderBy(m => m.C_index);

                return GetPagedList(q, pagesize, pageinex, out count);
            }
        }

        /// <summary>
        /// 插入到红包队列中等待
        /// </summary>
        /// <returns></returns>
        public int InsertRedQueue(Guid UID, enCardType cardType, Guid? orderid, int count)
        {
            /*
             每一个“获赠星卡红包领取”资格里面有4个队列，点击“获赠星卡红包领取”进入红包领取队列状态，
             每一个队列里面有5个红包，按顺序领取，如红包领取完，队列即显示已领取，
             随着第一个队列已领取，第二个队列自动激活，每次领取红包只允许一个队列，
             每领取完一个队列后，下一个队列自动激活排队，以此类推。
             */
            using (DBContext)
            {
                Guid ZGCardID;

                RedPacketList rplBig, rplSmall;
                RedPacket rplZG;//红包资格卡
                int result = 0;

                //购买N张，则插入N次表
                for (int i = 0; i < count; i++)
                {
                    ZGCardID = Guid.NewGuid();//同一个资格卡的ID，表明这是同一批次的

                    RedPacket redpk = new RedPacket();
                    //插入一张红包资格卡
                    redpk.RID = Guid.NewGuid();
                    redpk.UID = UID;
                    redpk.ZGCardID = ZGCardID;
                    redpk.RedType = (int)cardType;
                    redpk.BelongID = orderid;
                    redpk.Status = (int)enPacketStatus.Actived;
                    redpk.CreateTime = redpk.UpdateTime = DateTime.Now;
                    DBContext.RedPacket.Add(redpk);

                    //插入红包列表
                    RedPacketList rplist = new RedPacketList();
                    rplist.RLID = Guid.NewGuid();
                    rplist.RID = redpk.RID;
                    rplist.UID = UID;
                    rplist.Money = GetRedMoney(enPacketLV.First, cardType);
                    rplist.Score = 0;
                    rplist.RedType = redpk.RedType;
                    rplist.LV = (int)enPacketLV.First;
                    rplist.BelongID = redpk.BelongID;
                    rplist.Status = (int)enPacketStatus.NoActive;
                    rplist.CreateTime = rplist.UpdateTime = DateTime.Now;
                    DBContext.RedPacketList.Add(rplist);

                    for (int j = 0; j < _PerSmallCount; j++)
                    {
                        rplist = new RedPacketList();//插入一条记录
                        rplist.RLID = Guid.NewGuid();
                        rplist.RID = redpk.RID;
                        rplist.UID = UID;
                        rplist.Money = GetRedMoney(enPacketLV.Second, cardType); ;
                        rplist.Score = 0;
                        rplist.RedType = redpk.RedType;
                        rplist.LV = (int)enPacketLV.Second;
                        rplist.BelongID = redpk.BelongID;
                        rplist.Status = (int)enPacketStatus.NoActive;
                        rplist.CreateTime = rplist.UpdateTime = DateTime.Now;
                        DBContext.RedPacketList.Add(rplist);
                    }

                    /////////////////////////////////////////////////////////////////////////////////////////////////
                    //处理上一个待收红包：
                    //已激活的资格卡
                    //rplZG = DBContext.RedPacket.OrderBy(m => m.C_index)
                    //    .FirstOrDefault(m => m.RedType == (int)cardType && m.Status != (int)enPacketStatus.NoActive);
                    var q = from a in DBContext.RedPacket
                            where a.RedType == (int)cardType && a.Status < (int)enPacketStatus.ActivedAll &&
                                DBContext.RedPacketList.Count(m => m.RID == a.RID && m.Status == (int)enPacketStatus.NoActive) > 0
                            orderby a.C_index
                            select a;
                    rplZG = q.FirstOrDefault();

                    if (rplZG == null)//为空，则设置上一个资格卡为待处理
                    {
                        rplZG = DBContext.RedPacket.OrderBy(m => m.C_index)
                            .FirstOrDefault(m => m.RedType == (int)cardType && m.Status == (int)enPacketStatus.NoActive);

                        if (rplZG == null)
                        {
                            result = DBContext.SaveChanges();//插入待收红包列表
                            continue;
                        }

                        rplZG.Status = (int)enPacketStatus.Actived;
                        rplZG.UpdateTime = DateTime.Now;
                    }

                    rplBig = DBContext.RedPacketList.OrderBy(m => m.C_index)
                        .FirstOrDefault(m => m.RedType == (int)cardType && m.LV == (int)enPacketLV.First && m.Status == (int)enPacketStatus.NoActive);
                    rplSmall = DBContext.RedPacketList.OrderBy(m => m.C_index)
                        .FirstOrDefault(m => m.RedType == (int)cardType && m.LV == (int)enPacketLV.Second && m.Status == (int)enPacketStatus.NoActive);

                    if (rplBig != null)
                    {
                        rplBig.Status = (int)enPacketStatus.Actived;
                        rplBig.UpdateTime = DateTime.Now;

                        var bigpack = DBContext.RedPacket.FirstOrDefault(m => m.RID == rplBig.RID);
                        if (bigpack != null && bigpack.Status == (int)enPacketStatus.NoActive)
                            bigpack.Status = (int)enPacketStatus.Actived;
                    }
                    if (rplSmall != null)
                    {
                        rplSmall.Status = (int)enPacketStatus.Actived;
                        rplSmall.UpdateTime = DateTime.Now;
                    }

                    result = DBContext.SaveChanges();//插入待收红包列表

                    if (DBContext.RedPacketList.Count(m => m.RID == rplZG.RID && m.Status == (int)enPacketStatus.NoActive) == 0
                        && DBContext.RedPacket.Count(m => m.ZGCardID == rplZG.ZGCardID) < _PerDuilieCount)//本次资格卡里的红包已领取完，且资格卡数小于规定数
                    {
                        rplZG.Status = (int)enPacketStatus.ActivedAll;

                        //由上一次未完成的资格卡分裂，并自动启动为待领取
                        RedPacket n_redpk = new RedPacket();
                        //插入一张红包资格卡
                        n_redpk.RID = Guid.NewGuid();
                        n_redpk.UID = rplZG.UID;
                        n_redpk.ZGCardID = rplZG.ZGCardID;
                        n_redpk.RedType = rplZG.RedType;
                        n_redpk.BelongID = rplZG.BelongID;
                        n_redpk.Status = (int)enPacketStatus.Actived;
                        n_redpk.CreateTime = n_redpk.UpdateTime = DateTime.Now;
                        DBContext.RedPacket.Add(n_redpk);

                        //插入红包列表
                        RedPacketList n_rplist = new RedPacketList();
                        n_rplist.RLID = Guid.NewGuid();
                        n_rplist.RID = n_redpk.RID;
                        n_rplist.UID = UID;
                        n_rplist.Money = GetRedMoney(enPacketLV.First, (enCardType)rplZG.RedType);
                        n_rplist.Score = 0;
                        n_rplist.RedType = n_redpk.RedType;
                        n_rplist.LV = (int)enPacketLV.First;
                        n_rplist.BelongID = n_redpk.BelongID;
                        n_rplist.Status = (int)enPacketStatus.NoActive;
                        n_rplist.CreateTime = n_rplist.UpdateTime = DateTime.Now;
                        DBContext.RedPacketList.Add(n_rplist);

                        for (int j = 0; j < _PerSmallCount; j++)
                        {
                            n_rplist = new RedPacketList();//插入一条记录
                            n_rplist.RLID = Guid.NewGuid();
                            n_rplist.RID = n_redpk.RID;
                            n_rplist.UID = UID;
                            n_rplist.Money = GetRedMoney(enPacketLV.Second, (enCardType)rplZG.RedType); ;
                            n_rplist.Score = 0;
                            n_rplist.RedType = n_redpk.RedType;
                            n_rplist.LV = (int)enPacketLV.Second;
                            n_rplist.BelongID = n_redpk.BelongID;
                            n_rplist.Status = (int)enPacketStatus.NoActive;
                            n_rplist.CreateTime = n_rplist.UpdateTime = DateTime.Now;
                            DBContext.RedPacketList.Add(n_rplist);
                        }

                        result = DBContext.SaveChanges();//插入待收红包列表
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 激活红包
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public int ActiveRed(Guid ID)
        {
            using (DBContext)
            {
                var redpack = DBContext.RedPacketList.FirstOrDefault(m => m.RLID == ID);
                if (redpack == null)
                    return (int)ErrorCode.红包不存在;
                if (redpack.Status != (int)enPacketStatus.NoActive)//如果不是已激活
                    return (int)ErrorCode.状态异常或已处理;

                redpack.Status = (int)enPacketStatus.Actived;
                redpack.UpdateTime = DateTime.Now;
                var redpak = DBContext.RedPacket.FirstOrDefault(m => m.RID == redpack.RID && m.Status == (int)enPacketStatus.NoActive);
                if (redpak != null)
                {
                    redpak.Status = (int)enPacketStatus.Actived;
                    redpak.UpdateTime = redpack.UpdateTime;
                }

                return DBContext.SaveChanges();
            }
        }

        /// <summary>
        /// 领取红包的金额
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        //public int RecRedMoney(Guid ID, ref decimal redPrice)
        //{
        //    using (DBContext)
        //    {
        //        var redpack = DBContext.RedPacketList.FirstOrDefault(m => m.RLID == ID);
        //        if (redpack == null)
        //            return (int)ErrorCode.红包不存在;
        //        if (redpack.Status == (int)enPacketStatus.Used)//如果不是已激活
        //            return (int)ErrorCode.状态异常或已处理;
        //        if (redpack.Status != (int)enPacketStatus.Actived)//如果不是已激活
        //            return (int)ErrorCode.红包未激活;
        //        redpack.Status = (int)enPacketStatus.Used;
        //        redpack.UpdateTime = DateTime.Now;

        //        //用户金额要增加
        //        var user = DBContext.Users.FirstOrDefault(m => m.ID == redpack.UID);
        //        if (user == null)
        //            return (int)ErrorCode.账号不存在;

        //        user.Balance += redpack.Money;
        //        user.Score += redpack.Score;
        //        user.TotalScore += redpack.Score;

        //        int result = DBContext.SaveChanges();

        //        PayListBLL paybll = new PayListBLL();
        //        paybll.Insert(ID, user.ID, enPayInOutType.In, enPayType.Coin, enPayFrom.RedPaged, redpack.Money, "领取红包");

        //        //设置为已领完状态
        //        if (DBContext.RedPacketList.FirstOrDefault(m => m.RID == redpack.RID && m.Status != (int)enPacketStatus.Used) == null)
        //        {
        //            var redpak = DBContext.RedPacket.FirstOrDefault(m => m.RID == redpack.RID);
        //            if (redpak != null)
        //            {
        //                redpak.Status = (int)enPacketStatus.Used;
        //                redpak.UpdateTime = redpack.UpdateTime;

        //                result = DBContext.SaveChanges();//保存状态为已全部领取
        //            }
        //        }

        //        if (result > 0)
        //        {
        //            redPrice = redpack.Money;
        //        }

        //        return result;
        //    }
        //}

        private decimal GetRedMoney(enPacketLV lv, enCardType cardType)
        {
            if (cardType == enCardType.轻客星卡)
            {
                if (lv == enPacketLV.First)
                {
                    return SysConfigBLL.RedLvSmall1;
                }
                else
                {
                    return SysConfigBLL.RedLvSmall2;
                }
            }
            else if (cardType == enCardType.轻客金卡)
            {
                if (lv == enPacketLV.First)
                {
                    return SysConfigBLL.RedLvBig1;
                }
                else
                {
                    return SysConfigBLL.RedLvBig2;
                }
            }
            else
                return 0;
        }

        /// <summary>
        /// 获取未领取红包数
        /// </summary>
        /// <returns></returns>
        public int ActivedRedCount(Guid UID)
        {
            using (DBContext)
            {
                return DBContext.RedPacketList.Count(m => m.UID == UID && m.Status == (int)enPacketStatus.Actived);
            }
        }

        /// <summary>
        /// 获取未领取红包数
        /// </summary>
        /// <returns></returns>
        public List<RedPacketList> ActivedReds(Guid UID)
        {
            using (DBContext)
            {
                return DBContext.RedPacketList.Where(m => m.UID == UID && m.Status == (int)enPacketStatus.Actived).ToList();
            }
        }
    }
}
