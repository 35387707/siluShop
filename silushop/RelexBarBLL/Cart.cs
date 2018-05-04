using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;

namespace RelexBarBLL
{
    public partial class CartBLL : BaseBll
    {
        /// <summary>
        /// 获取购物车商品数量
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public int GetCarCount(Guid UID, int isBuy)
        {
            using (DBContext)
            {
                return DBContext.Cart.Where(m => m.UID == UID && m.IsBuy == isBuy).Count();
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

                Cart c = new Cart();
                c.ID = Guid.NewGuid();
                c.UID = UID;
                c.ProductID = PID;
                if (j > 0)
                {
                    c.TrueProductID = tp.SPID;
                    //tp.Stock -= count;//加入购物车，减库存
                    //tp.UpdateTime = DateTime.Now;
                }
                // else {
                // p.Stock -= count;
                // p.UpdateTime = DateTime.Now;
                // }

                c.IsBuy = 0;
                c.Count = count;
                c.CreateTime = DateTime.Now;
                c.UpdateTime = DateTime.Now;
                DBContext.Cart.Add(c);
                return DBContext.SaveChanges();
            }
        }

        //public List<Models.ShopCarModel> GetList(Guid UID)
        //{
        //    using (DBContext)
        //    {
        //        StringBuilder sql = new StringBuilder();
        //        sql.Append("select c.*,p.Name,p.Img,tp.SPDesc,tp.Remark SPRemark,case when c.TrueProductID is null then p.Price else tp.Price end as Price from Cart c ");
        //        sql.Append("left join ProductList p on c.ProductID = p.ID ");
        //        sql.Append("left join TrueProduct tp on c.TrueProductID = tp.SPID ");
        //        sql.Append("where c.UID = @uid and IsBuy = 0");
        //        return DBContext.Database.SqlQuery<Models.ShopCarModel>(sql.ToString(), new SqlParameter[]{
        //            new SqlParameter("@uid",UID)
        //        }).ToList();
        //    }
        //}
        /// <summary>
        /// 更新购物车数量
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public int UpdateCount(List<Models.UpdateCartModel> list)
        {
            if (list != null)
            {
                using (DBContext)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        Guid tempid = list[i].ID;
                        Cart c = DBContext.Cart.Where(m => m.ID == tempid).FirstOrDefault();
                        if (c == null)
                        {
                            return -1;
                        }
                        c.Count = list[i].Count;
                        c.UpdateTime = DateTime.Now;
                    }
                    return DBContext.SaveChanges();

                }
            }
            return -1;
        }
        public List<Cart> List(Guid Uid)
        {
            using (DBContext)
            {
                ////EF 执行SQL语句
                //DBContext.Database.ExecuteSqlCommand("");
                return DBContext.Cart.Where(m => m.UID == Uid).ToList();
            }
        }
        /// <summary>
        /// 删除购物车中的商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Delete(Guid id)
        {
            using (DBContext)
            {
                Cart c = DBContext.Cart.Where(m => m.ID == id && m.IsBuy == 0).FirstOrDefault();
                if (c == null)
                {
                    return -1;
                }
                DBContext.Cart.Remove(c);
                return DBContext.SaveChanges();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public List<Models.CartModel> GetList(Guid UID) {
            using (DBContext) {
                StringBuilder sql = new StringBuilder();
                sql.Append("select c.ID,c.Count,p.Img,p.Name,c.ProductID,(case when c.TrueProductID IS null then p.Price else (select Price from TrueProduct where SPID=c.TrueProductID) end) Price,s.ShopName,s.ID ShopID from Cart c ");
                sql.Append("left join ProductList p on c.ProductID=p.ID left join Shop s on p.ShopID=s.ID where c.UID='" + UID + "' and IsBuy=0");
                return DBContext.Database.SqlQuery<Models.CartModel>(sql.ToString()).ToList();
            }
        }
        
    }
}
