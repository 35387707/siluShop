using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;
using Newtonsoft.Json;

namespace RelexBarBLL
{
    public enum enAdminMsgType {
        /// <summary>
        /// 商家入驻申请
        /// </summary>
        ShopReq = 0,
    }
    public enum enAdminMsgResult {
        /// <summary>
        /// 未处理
        /// </summary>
        undisposed=-1,
        /// <summary>
        /// 同意
        /// </summary>
        agree=1,
        /// <summary>
        /// 不同意
        /// </summary>
        disagree=-1,
    }
    public class AdminMsgBLL:BaseBll
    {
        public int Update(Guid ID,enAdminMsgResult result) {
            using (DBContext) {
                AdminMsg m= DBContext.AdminMsg.Where(ms => ms.ID == ID).FirstOrDefault();
                if (m.Result!=(int)enAdminMsgResult.undisposed) {
                    throw new Exception("不能重复处理");
                }
                m.IsRead = 1;
                m.Result = (int)result;
                if (m.Type==(int)enAdminMsgType.ShopReq) {
                    Users user= DBContext.Users.Where(u => u.ID == m.Sender).FirstOrDefault();
                    if (user==null) {
                        throw new Exception("用户不存在");
                    }
                    user.UserType = (int)enUserType.Shop;
                    Dictionary<string, string> d = JsonConvert.DeserializeObject<Dictionary<string, string>>(m.Content);
                    Shop shop = new Shop();
                    shop.ID = Guid.NewGuid();
                    shop.ShopName = d["ShopName"];
                    shop.UID = m.Sender;
                    shop.Remark = m.Remark;
                    shop.CreateTime = shop.UpdateTime = DateTime.Now;
                    shop.Img = d["HeadImg"];
                    shop.AreaID= d["AreaID"];
                    shop.Address= d["Address"];
                    shop.BackImg= d["BackImg"];
                    shop.IDcard_Img= d["IDcard_img"];
                    shop.IDcard_Img2= d["IDcard_img2"];
                    shop.BusinessLicense_Img= d["BLImg"];
                    DBContext.Shop.Add(shop);
                }
                return DBContext.SaveChanges();
            }
        }
        public AdminMsg Get(Guid ID) {
            using (DBContext) {
                return DBContext.AdminMsg.Where(m => m.ID == ID).FirstOrDefault();
            }
        }
        public List<Models.AdminMsgModel> GetList(int index,int pageSize,out int sum,enAdminMsgType? type,enAdminMsgResult? result) {
            using (DBContext) {

                var q= DBContext.AdminMsg.Join(DBContext.Users,m=>m.Sender,u=>u.ID,(m,u)=>new Models.AdminMsgModel {
                    ID=m.ID,
                    Sender=m.Sender,
                    Content=m.Content,
                    Type=m.Type,
                    Status=m.Status,
                    IsRead=m.IsRead,
                    Result=m.Result,
                    Remark=m.Remark,
                    CreateTime=m.CreateTime,
                    UpdateTime=m.UpdateTime,
                    SenderAccount=u.Name
                }).Where(m => 1 == 1);
                if (type!=null) {
                    q = q.Where(m => m.Type == (int)type);
                }
                if (result!=null) {
                    q = q.Where(m => m.Result == (int)result);
                }
                return GetPagedList(q.OrderByDescending(m => m.CreateTime), pageSize, index,out sum);
            }

        }
        /// <summary>
        /// 含有未处理的商家入驻申请
        /// </summary>
        /// <param name="Sender"></param>
        /// <returns></returns>
        public bool HasShopReq(Guid Sender) {
            using (DBContext) {
                int i=DBContext.AdminMsg.Where(m=>m.Sender==Sender&&m.Type==(int)enAdminMsgType.ShopReq&&m.Result==-1).Count();
                return i > 0;
            }
        }
        public int Add(Guid Sender,string content,enAdminMsgType type,string remark) {
            using (DBContext) {
                AdminMsg m = new AdminMsg();
                m.ID = Guid.NewGuid();
                m.Sender = Sender;
                m.Content = content;
                m.Type = (int)type;
                m.Status = 1;
                m.IsRead = 0;
                m.Result = -1;
                m.Remark = remark;
                m.CreateTime = m.UpdateTime = DateTime.Now;
                DBContext.AdminMsg.Add(m);
                return DBContext.SaveChanges();
            }
        }
        public int Add(AdminMsg msg) {
            using (DBContext) {
                DBContext.AdminMsg.Add(msg);
                return DBContext.SaveChanges();
            }
        }
    }
}
