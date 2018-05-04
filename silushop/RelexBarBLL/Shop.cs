using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;
namespace RelexBarBLL
{
    public class ShopBLL: BaseBll
    {
        public int UpdateShop(Guid ID,string ShopName,int AgentType) {
            using (DBContext) {
                Shop shop= DBContext.Shop.Where(m => m.ID == ID).FirstOrDefault();
                if (shop!=null) {
                    shop.ShopName = ShopName;
                    shop.AgentType = AgentType;
                    shop.UpdateTime = DateTime.Now;
                }
                return DBContext.SaveChanges();
            }
        }
        public List<Models.AdminShop> GetAdminShopList(int index, int pageSize, out int sum, string key)
        {
            using (DBContext)
            {
                sum = DBContext.Shop.Count();
                var q = DBContext.Shop.Join(DBContext.Users, s => s.UID, u => u.ID, (s, u) => new Models.AdminShop()
                {
                    ID = s.ID,
                    ShopName = s.ShopName,
                    UID = s.UID,
                    Remark = s.Remark,
                    CreateTime = s.CreateTime,
                    UpdateTime = s.UpdateTime,
                    Img = s.Img,
                    AreaID = s.AreaID,
                    Address = s.Address,
                    BackImg = s.BackImg,
                    IDcard_Img = s.IDcard_Img,
                    IDcard_Img2 = s.IDcard_Img2,
                    BusinessLicense_Img = s.BusinessLicense_Img,
                    ChatQQ = s.ChatQQ,
                    ServicePhone = s.ServicePhone,
                    AgentType = s.AgentType,
                    Account = u.Name
                });
                if (!string.IsNullOrEmpty(key))
                {
                    q = q.Where(m => m.ShopName.Contains(key));
                }
                return GetPagedList(q.OrderByDescending(m => m.CreateTime), pageSize, index);
            }
        }
        public List<Shop> GetList(int index, int pageSize,out int sum) {
            using (DBContext) {
                sum = DBContext.Shop.Count();
                return GetPagedList(DBContext.Shop.OrderByDescending(m=>m.CreateTime), pageSize, index);
            }
        }
        public int Update(Guid ID, string Img, string ShopName, string ChatQQ, string BackImg,string ServicePhone) {
            using (DBContext) {
                Shop s = DBContext.Shop.Where(m => m.ID == ID).First();
                s.Img = Img;
                s.ShopName = ShopName;
                s.ChatQQ = ChatQQ;
                s.BackImg = BackImg;
                s.ServicePhone = ServicePhone;
                return DBContext.SaveChanges();
            }
        }
        public Shop Get(Guid ID) {
            using (DBContext) {
                return DBContext.Shop.Where(m => m.ID == ID).FirstOrDefault();
            }
        }
        public Shop GetByUID(Guid UID) {
            using (DBContext) {
                return DBContext.Shop.Where(m => m.UID == UID).FirstOrDefault();
            }
        }
        public string GetShopName(Guid ID) {
            using (DBContext) {
                Shop shop= DBContext.Shop.Where(m => m.ID == ID).FirstOrDefault();
                return shop == null ? null : shop.ShopName;
            }
        }
        public int GetShopProductCount(Guid? ShopID) {
            using (DBContext) {
                var q = DBContext.ProductList.Where(m => m.CategoryID!=-1);
                if (ShopID != null)
                {
                    q = q.Where(m => m.ShopID == ShopID.Value);
                }
                return q.Count();
            }
            
        }
    }
}
