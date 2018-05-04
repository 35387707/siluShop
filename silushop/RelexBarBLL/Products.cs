using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;

namespace RelexBarBLL
{
    public partial class ProductsBLL : BaseBll
    {
        public List<ProductList> GetRandProduct(int count)
        {
            using (DBContext)
            {
                int allcount = DBContext.ProductList.Where(m => m.CategoryID != -1).Count();
                if (allcount == 0)
                {
                    return new List<ProductList>();
                }
                if (allcount < count)
                {
                    count = allcount;
                }
                int[] row_num = new int[count];
                Random rand = new Random();
                for (int i = 0; i < row_num.Length; i++)
                {
                    bool flag = true;
                    do
                    {
                        int temp = rand.Next(1, allcount + 1);
                        for (int j = 0; j < row_num.Length; j++)
                        {
                            if (temp == row_num[j])
                            {
                                break;
                            }
                            if (row_num[j] == 0)
                            {
                                row_num[i] = temp;
                                flag = false;
                                break;
                            }
                        }
                    } while (flag);
                }
                StringBuilder str = new StringBuilder();
                for (int i = 0; i < row_num.Length; i++)
                {
                    if (i > 0)
                    {
                        str.Append("," + row_num[i]);
                    }
                    else
                    {
                        str.Append(row_num[i]);
                    }
                }
                string sql = "select * from(select top 5000 ROW_NUMBER() over(order by OrderID) rnum,* from ProductList  where CategoryID<>-1) as temp where temp.rnum in(" + str.ToString() + ")";
                return DBContext.ProductList.SqlQuery(sql).ToList();
            }

        }
        /// <summary>
        /// 添加到购物车
        /// </summary>
        /// <param name="PID"></param>
        /// <param name="SPDesc"></param>
        /// <returns></returns>
        public int Insert(Guid UID, Guid PID, string SPDesc, int count)
        {
            using (DBContext)
            {
                ProductList p = DBContext.ProductList.Where(m => m.ID == PID).FirstOrDefault();
                if (p == null)
                {
                    return -1;
                }
                int j = DBContext.TrueProduct.Where(m => m.ProductID == PID).Count();//查询商品是否有规格
                if (j > 0 && string.IsNullOrEmpty(SPDesc))
                {
                    return -1;
                }
                TrueProduct tp = DBContext.TrueProduct.Where(m => m.ProductID == PID && m.SPDesc.Contains(SPDesc)).FirstOrDefault();

                if (j > 0 && tp == null)
                {
                    return -1;
                }

                Cart c;
                if (j > 0)
                {
                    c = DBContext.Cart.FirstOrDefault(m => m.ProductID == PID && m.UID == UID && m.TrueProductID == tp.SPID);
                }
                else
                {
                    c = DBContext.Cart.FirstOrDefault(m => m.ProductID == PID && m.UID == UID);
                }
                if (c == null)
                {
                    c = new Cart();
                    c.ID = Guid.NewGuid();
                    c.UID = UID;
                    c.ProductID = PID;
                    c.CreateTime = DateTime.Now;
                    c.Count = count;
                    DBContext.Cart.Add(c);
                    if (j > 0)
                    {
                        c.TrueProductID = tp.SPID;
                    }
                }
                else
                {
                    c.Count += count;
                }

                c.IsBuy = 0;
                c.UpdateTime = DateTime.Now;
                return DBContext.SaveChanges();
            }
        }
        /// <summary>
        /// 检查库存
        /// </summary>
        /// <param name="PID"></param>
        /// <param name="SPDesc"></param>
        /// <returns></returns>
        public int CheckStock(Guid PID, string SPDesc, decimal count)
        {
            using (DBContext)
            {
                int j = DBContext.TrueProduct.Where(m => m.ProductID == PID).Count();
                if (j > 0)
                {
                    TrueProduct tp = DBContext.TrueProduct.Where(m => m.ProductID == PID && m.SPDesc.Contains(SPDesc)).FirstOrDefault();
                    if (tp == null)
                    {
                        throw new Exception("商品不存在");
                    }
                    if (tp.Stock < count)
                    {
                        return -1;
                    }
                }
                else
                {
                    ProductList p = DBContext.ProductList.Where(m => m.ID == PID).FirstOrDefault();
                    if (p == null)
                    {
                        return -1;
                    }
                    if (p.Stock < count)
                    {
                        return -1;
                    }
                }
                return 1;

            }
        }
        public bool IsFavorites(Guid UID, Guid PID)
        {
            using (DBContext)
            {
                return DBContext.FavoritesProduct.Where(m => m.UID == UID && m.ProductID == PID).Count() > 0;
            }
        }
        /// <summary>
        /// 添加商品到收藏夹
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="PID"></param>
        /// <returns></returns>
        public int AddFavorites(Guid UID, Guid PID)
        {
            using (DBContext)
            {
                int i = DBContext.ProductList.Where(m => m.ID == PID).Count();
                if (i > 0)
                {
                    FavoritesProduct f = new FavoritesProduct();
                    f.ID = Guid.NewGuid();
                    f.UID = UID;
                    f.ProductID = PID;
                    f.CreateTime = f.UpdateTime = DateTime.Now;
                    DBContext.FavoritesProduct.Add(f);
                    return DBContext.SaveChanges();
                }
                else
                {
                    throw new Exception("商品不存在");
                }
            }
        }
        public List<Models.FavoritesModel> Favorites(Guid UID, int index, int pageSize)
        {
            using (DBContext)
            {
                //sum = DBContext.FavoritesProduct.Where(m => m.UID == UID).Count();
                string sql = "select * from(select ROW_NUMBER() over(order by f.createtime desc) rnum, f.ID, p.ID PID, p.Name, p.Title, p.Img, p.Price "
                    + "from dbo.FavoritesProduct f left join ProductList p on f.ProductID = p.ID where UID='" + UID + "') as temp where rnum>" + (index - 1) * pageSize + " and rnum<=" + pageSize * index;
                return DBContext.Database.SqlQuery<Models.FavoritesModel>(sql).ToList();
            }
        }
        public int DelFavorites(Guid UID, Guid PID)
        {
            using (DBContext)
            {
                FavoritesProduct f = DBContext.FavoritesProduct.Where(m => m.UID == UID && m.ProductID == PID).FirstOrDefault();
                DBContext.FavoritesProduct.Remove(f);
                return DBContext.SaveChanges();
            }
        }
        /// <summary>
        /// 获取商品价格
        /// </summary>
        /// <param name="PID"></param>
        /// <param name="SPDesc"></param>
        /// <returns></returns>
        public void GetPrice(Guid PID, string SPDesc, out decimal? Price, out decimal? RealPrice)
        {
            using (DBContext)
            {
                int j = DBContext.TrueProduct.Where(m => m.ProductID == PID).Count();
                if (j > 0)
                {
                    TrueProduct tp = DBContext.TrueProduct.Where(m => m.ProductID == PID && m.SPDesc.Contains(SPDesc)).FirstOrDefault();
                    if (tp == null)
                    {
                        Price = null; RealPrice = null;
                    }
                    else
                    {
                        Price = tp.Price; RealPrice = tp.RealPrice;
                    }
                }
                else
                {
                    ProductList p = DBContext.ProductList.Where(m => m.ID == PID).FirstOrDefault();
                    if (p == null)
                    {
                        Price = null; RealPrice = null;
                    }
                    else
                    {
                        Price = p.Price; RealPrice = p.RealPrice;
                    }
                }
            }
        }
        public void DeleteSpec(Guid proid)
        {
            ExceSql("delete from TrueProduct where ProductID = {0}", proid);
        }
        public int InsertSpecProduct(Guid? SPID, Guid ProductID, string SPName, string SPDesc, string Number, decimal Weight, decimal RealPrice, enPayType paytpye
               , decimal Price, decimal Stock, string Remark)
        {
            using (DBContext)
            {
                TrueProduct model = new TrueProduct();
                model.ProductID = ProductID;
                if (!SPID.HasValue)
                {
                    model.SPID = Guid.NewGuid();
                }
                else
                {
                    model.SPID = SPID.Value;
                }

                model.SPName = SPName;
                model.SPDesc = SPDesc;
                model.Number = Number;
                model.Weight = Weight;
                model.RealPrice = RealPrice;
                model.PriceType = (int)paytpye;
                model.Price = Price;
                model.Stock = Stock;
                model.Status = (int)enStatus.Enabled;
                model.CreateTime = model.UpdateTime = DateTime.Now;
                model.Remark = Remark;
                DBContext.TrueProduct.Add(model);
                return DBContext.SaveChanges();
            }
        }
        public Guid Insert(Guid ShopID, string Name, string Title, string Number, int CategoryID, string Img, string ImgList, string Descrition,
               decimal RealPrice, enPayType paytpye, decimal Price, decimal Stock, int OrderID, enProductType ptype, DateTime BeginTime, DateTime EndTime, decimal CashDiscount
               , Common.enStatus Status = Common.enStatus.Enabled)
        {
            using (DBContext)
            {
                ProductList model = new ProductList();
                model.ID = Guid.NewGuid();
                model.ShopID = ShopID;
                model.Name = Name;
                model.Title = Title;
                model.CategoryID = CategoryID;
                model.Img = Img;
                model.ImgList = ImgList;
                model.Descrition = Descrition;
                model.RealPrice = RealPrice;
                model.PriceType = (int)paytpye;
                model.Price = Price;
                model.Stock = Stock;
                model.OrderID = OrderID;
                model.Type = (int)ptype;
                model.BeginTime = BeginTime;
                model.EndTime = EndTime;
                model.Number = Number;
                model.Status = (int)Status;
                model.CreateTime = model.UpdateTime = DateTime.Now;
                model.CashDiscount = CashDiscount;

                DBContext.ProductList.Add(model);
                if (DBContext.SaveChanges() > 0)
                    return model.ID;
                else
                    return Guid.Empty;
            }
        }
        /// <summary>
        /// 获取我能使用的开户券
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public List<Models.MyTicket> GetAccountTicket(Guid UID)
        {
            string sql = "select pr.ID,p.Price,pr.CreateTime,pr.Status,pr.IsBuyProduct,p.ID PID,opl.OrderID,opl.Status OrderStatus,opl.ID OPID from PayRecord pr left join ProductList p on pr.Level=p.Level left join OrderProductList opl on pr.OrderID=opl.OrderID where UID ='" + UID + "' order by CreateTime desc";
            using (DBContext)
            {
                DateTime time = DateTime.Now.AddMonths(1);
                return DBContext.Database.SqlQuery<Models.MyTicket>(sql).ToList();
                //return DBContext.ProductList.Where(m => DBContext.PayRecord.Where(p => p.UID == UID && p.IsBuyProduct == 0 && p.CreateTime <= time).Select(p => p.Level).ToList().Contains(m.Level)).ToList();
            }
        }
        public List<TrueProduct> GetProductSpec(Guid proID)
        {
            using (DBContext)
            {
                return DBContext.TrueProduct.Where(m => m.ProductID == proID && m.Status == (int)enStatus.Enabled).ToList();
            }
        }
        /// <summary>
        /// 获取我能使用的开户券
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        //public List<ProductList> GetAccountTicket(Guid UID) {
        //    using (DBContext) {
        //        DateTime time = DateTime.Now.AddMonths(1);
        //        return DBContext.ProductList.Where(m => DBContext.PayRecord.Where(p => p.UID == UID && p.IsBuyProduct == 0&&p.CreateTime<=time).Select(p => p.Level).ToList().Contains(m.Level)).ToList();
        //    }
        //}
        //public List<Models.AdminTicket> GetTicket(int? index, int pageSize,out int sum) {
        //    using (DBContext) {
        //        StringBuilder sql = new StringBuilder();
        //        sql.Append("select ROW_NUMBER() over(order by pr.CreateTime desc) rownumber, u.CardNumber,u.Name,p.Name PName,pr.CreateTime from PayRecord pr");
        //        sql.Append(" left join ProductList p on pr.Level=p.Level");
        //        sql.Append(" left join Users u on pr.UID=u.ID");
        //        sum=DBContext.Database.SqlQuery<int>("select count(rownumber) from ("+sql.ToString()+") as temp")
        //    }
        //}


        /// <summary>
        /// 购买产品
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="PID"></param>
        /// <returns></returns>
        public int Buy(Guid UID, Guid PID)
        {
            return 0;
            //using (DBContext)
            //{
            //    ProductList p = DBContext.ProductList.Where(m => m.ID == PID).FirstOrDefault();
            //    if (p == null)
            //    {
            //        throw new Exception("商品不存在");
            //    }
            //    Users u = DBContext.Users.Where(m => m.ID == UID).FirstOrDefault();
            //    if (u == null)
            //    {
            //        throw new Exception("用户不存在");
            //    }
            //    //if (u.Score<p.Price) {
            //    //    throw new Exception("余额不足");
            //    //}
            //    if (u.Balance < p.Price)
            //    {
            //        throw new Exception("首购券不足");
            //    }
            //    //获取当前级别
            //    PayRecord pr = DBContext.PayRecord.Where(m => m.UID == u.ID).OrderByDescending(m => m.CreateTime).FirstOrDefault();
            //    PayRecord n = new PayRecord();
            //    n.ID = Guid.NewGuid();
            //    n.LocalMoney = p.DailySalary;
            //    //n.MaxMoney = p.Price * 3;
            //    n.MaxMoney = p.Price * 2;//2倍
            //    n.Status = 1;
            //    n.Level = p.Level;
            //    n.UID = u.ID;
            //    n.IsBuyProduct = 0;
            //    n.CreateTime = DateTime.Now;
            //    PayList paylist = new PayList();
            //    paylist.UID = u.ID;
            //    paylist.InOut = (int)Common.enPayInOutType.Out;
            //    paylist.PayType = (int)Common.enPayType.Point;
            //    paylist.FromTo = (int)Common.enPayFrom.OnLinePay;
            //    paylist.Val = p.Price;
            //    paylist.Remark = "购买产品：" + p.Name;
            //    paylist.Status = 1;
            //    paylist.CreateTime = DateTime.Now;
            //    paylist.UpdateTime = DateTime.Now;
            //    #region 购买验证去掉
            //    //if (pr == null)
            //    //{//还没购买过
            //    //    if (p.Level != 1)
            //    //    {
            //    //        throw new Exception("没有权限购买当前产品");
            //    //    }
            //    //    else {
            //    //        DBContext.PayList.Add(paylist);
            //    //        DBContext.PayRecord.Add(n);
            //    //        u.Score -= p.Price;
            //    //        u.LV = p.Level;
            //    //    }

            //    //}
            //    //else {
            //    //    if (pr.Level + 1 == p.Level)
            //    //    {
            //    //        if (pr.Status == 1)
            //    //        {
            //    //            throw new Exception("当前产品收益没有返还完毕，不能购买");
            //    //        }
            //    //        else {
            //    //            DBContext.PayList.Add(paylist);
            //    //            DBContext.PayRecord.Add(n);
            //    //            u.Score -= p.Price;
            //    //            u.LV = p.Level;
            //    //        }
            //    //    }
            //    //    else {
            //    //        throw new Exception("没有权限购买当前产品");
            //    //    }
            //    //}
            //    #endregion
            //    DBContext.PayList.Add(paylist);
            //    DBContext.PayRecord.Add(n);
            //    u.Balance -= p.Price;
            //    //u.Score -= p.Price;
            //    //取最大等级
            //    if (p.Level > u.LV)
            //    {
            //        u.LV = p.Level;
            //    }
            //    Users fuser = DBContext.Users.FirstOrDefault(m => m.ID == u.FID);//父节点也能得到奖励
            //    if (fuser != null)
            //    {
            //        decimal realsy = 0;
            //        UsersBLL userbll = new UsersBLL();
            //        userbll.RealShouYi(DBContext, fuser, p.Price * 0.12M, out realsy, "分享加速12%：", "", DateTime.Now, enPayListType.KaiHu);

            //        fuser.Score += realsy * (1 - userbll.GWQBL);
            //        fuser.TotalScore += realsy * (1 - userbll.GWQBL);
            //        fuser.ShoppingVoucher += realsy * userbll.GWQBL;

            //        userbll.InsertShouRuPayList(DBContext, fuser.ID, DateTime.Now, realsy);
            //    }
            //    return DBContext.SaveChanges();
            //}
        }
        public List<ProductList> GetLevelProductList()
        {
            using (DBContext)
            {
                return DBContext.ProductList.Where(m => m.ShopID == Guid.Empty && m.CategoryID == -1).OrderBy(m => m.OrderID).ToList();
            }
        }

        public List<ProductList> GetProductList(enProductType ptpye)
        {
            using (DBContext)
            {
                return DBContext.ProductList.Where(m => m.Status == (int)enStatus.Enabled && m.Type == (int)ptpye).OrderByDescending(m => m.OrderID).ToList();
            }
        }
        public List<ProductList> GetAllProductList(int categoryid)
        {
            using (DBContext)
            {
                return DBContext.ProductList.Where(m => m.CategoryID == categoryid && m.Status == (int)enStatus.Enabled).OrderByDescending(m => m.OrderID).ToList();
            }
        }

        public List<ProductList> GetAllProductList()
        {
            using (DBContext)
            {
                return DBContext.ProductList.OrderBy(m => m.OrderID).ToList();
            }
        }

        public List<ProductList> GetProductList(enProductType ptpye, int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                var q = DBContext.ProductList.Where(m => m.Status == (int)enStatus.Enabled && m.Type == (int)ptpye).OrderByDescending(m => m.OrderID);
                return GetPagedList(q, pagesize, pageinex, out count);
            }
        }
        public List<ProductList> GetProductList(int categoryid, int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                var q = DBContext.ProductList.Where(m => m.Status == (int)enStatus.Enabled && m.CategoryID == categoryid).OrderByDescending(m => m.OrderID);

                return GetPagedList(q, pagesize, pageinex, out count);
            }
        }
        public List<Models.ProductListModel> GetAllProductList(int? categoryid, string key, int? status, enProductType? protype
            , int pagesize, int pageinex, out int count, Guid? shopID = null, enOrderBy orderby = enOrderBy.TimeDESC, int C = 0)
        {
            using (DBContext)
            {
                var q = from a in DBContext.ProductList
                        join b in DBContext.Category on a.CategoryID equals b.ID into t
                        from c in t.DefaultIfEmpty()
                        join d in DBContext.Users on a.ShopID equals d.ID into t2
                        from e in t2.DefaultIfEmpty()
                        select new Models.ProductListModel
                        {
                            ID = a.ID,
                            Name = a.Name,
                            Title = a.Title,
                            Number = a.Number,
                            CategoryID = a.CategoryID,
                            Img = a.Img,
                            ImgList = a.ImgList,
                            Descrition = a.Descrition,
                            RealPrice = a.RealPrice,
                            PriceType = a.PriceType,
                            Stock = a.Stock,
                            Price = a.Price,
                            OrderID = a.OrderID,
                            Type = a.Type,
                            BeginTime = a.BeginTime,
                            EndTime = a.EndTime,
                            Status = a.Status,
                            CreateTime = a.CreateTime,
                            UpdateTime = a.UpdateTime,
                            ShopID = a.ShopID,
                            ShopName = e == null ? "" : e.Name,
                            ShopTrueName = e == null ? "" : e.TrueName,
                            ShopStatus = e == null ? -1 : e.Status,
                            ShopAddress = e == null ? "" : e.Address,
                            ShopPhone = e == null ? "" : e.Phone,
                            CategoryName = c == null ? "" : c.Name,
                            CategoryShow = c == null ? null : c.IsShow,
                            Payed = a.Payed
                        };
                if (C == 1)
                {
                    q = q.Where(m => m.CategoryID != -1);
                }

                if (categoryid.HasValue)
                {
                    q = q.Where(m => m.CategoryID.Value == categoryid.Value);
                }
                if (shopID != null)
                {
                    q = q.Where(m => m.ShopID == shopID);
                }
                if (!string.IsNullOrEmpty(key))
                {
                    //q = q.Where(m => m.Name.Contains(key) || m.Number.Contains(key) || m.Title.Contains(key) || m.CategoryName.Contains(key) ||
                    //m.ShopName.Contains(key) || m.Descrition.Contains(key));
                    q = q.Where(m => m.Name.Contains(key) || m.Title.Contains(key));
                }
                if (protype.HasValue)
                {
                    q = q.Where(m => m.Type == (int)protype.Value);
                }
                if (status.HasValue)
                {
                    q = q.Where(m => m.Status == status.Value);
                }
                switch (orderby)
                {
                    case enOrderBy.OrderID:
                        q = q.OrderBy(m => m.OrderID);
                        break;
                    case enOrderBy.TimeASC:
                        q = q.OrderBy(m => m.OrderID);
                        break;
                    case enOrderBy.TimeDESC:
                        q = q.OrderByDescending(m => m.OrderID);
                        break;
                    case enOrderBy.SalesASC:
                        q = q.OrderBy(m => m.Payed);
                        break;
                    case enOrderBy.SalesDESC:
                        q = q.OrderByDescending(m => m.Payed);
                        break;
                    case enOrderBy.PriceASC:
                        q = q.OrderBy(m => m.Price);
                        break;
                    case enOrderBy.PriceDESC:
                        q = q.OrderByDescending(m => m.Price);
                        break;
                }

                return GetPagedList(q, pagesize, pageinex, out count);
            }
        }
        public List<ProductList> GetAllProductList(int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                var q = DBContext.ProductList.OrderByDescending(m => m.OrderID);

                return GetPagedList(q, pagesize, pageinex, out count);
            }
        }

        public ProductList GetProduct(string Uid)
        {
            return GetProduct(Guid.Parse(Uid));
        }
        public ProductList GetProduct(Guid id)
        {
            using (DBContext)
            {
                return DBContext.ProductList.FirstOrDefault(m => m.ID == id);
            }
        }

        //public int Insert(Guid ShopID, string Name, string Title, string Number, int CategoryID, string Img, string ImgList, string Descrition,
        //    decimal RealPrice, enPayType paytpye, decimal Price, decimal Stock, int OrderID, enProductType ptype, DateTime BeginTime, DateTime EndTime)
        //{
        //    using (DBContext)
        //    {
        //        ProductList model = new ProductList();
        //        model.ID = Guid.NewGuid();
        //        model.ShopID = ShopID;
        //        model.Name = Name;
        //        model.Title = Title;
        //        model.CategoryID = CategoryID;
        //        model.Img = Img;
        //        model.ImgList = ImgList;
        //        model.Descrition = Descrition;
        //        model.RealPrice = RealPrice;
        //        model.PriceType = (int)paytpye;
        //        model.Price = Price;
        //        model.Stock = Stock;
        //        model.OrderID = OrderID;
        //        model.Type = (int)ptype;
        //        model.BeginTime = BeginTime;
        //        model.EndTime = EndTime;
        //        model.Number = Number;
        //        model.Status = 1;
        //        model.CreateTime = model.UpdateTime = DateTime.Now;

        //        DBContext.ProductList.Add(model);
        //        return DBContext.SaveChanges();
        //    }
        //}

        public int Update(ProductList model)
        {
            using (DBContext)
            {
                DBContext.ProductList.Attach(model);
                DBContext.Entry<ProductList>(model).State = System.Data.Entity.EntityState.Modified;
                return DBContext.SaveChanges();
            }
        }

        public int UpdateStatus(Guid ID, int status)
        {
            using (DBContext)
            {
                var pro = DBContext.ProductList.FirstOrDefault(m => m.ID == ID);
                if (pro == null)
                {
                    return 0;
                }
                pro.Status = status;
                pro.UpdateTime = DateTime.Now;

                return DBContext.SaveChanges();
            }
        }
    }
}
