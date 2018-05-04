using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;

namespace RelexBarBLL
{
    public partial class RecAddressBLL : BaseBll
    {
        /// <summary>
        /// 获取默认收货地址
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public RecAddress GetRecDefault(Guid UID) {
            using (DBContext) {
                return DBContext.RecAddress.Where(m => m.UID == UID && m.IsDefault == 1).FirstOrDefault();
            }
        }
        /// <summary>
        /// 删除地址
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int DeleteAddress(Guid UID,Guid id)
        {
            using (DBContext)
            {
                var q = DBContext.RecAddress.FirstOrDefault(m => m.ID == id&&m.UID==UID);
                if (q == null) {
                    throw new Exception("收货地址不存在");
                }
                q.Status = (int)enStatus.Unabled;
                return DBContext.SaveChanges();
            }
        }
        /// <summary>
        /// 设置收获地址为默认
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="RID"></param>
        /// <returns></returns>
        public int SetDefault(Guid UID,Guid RID) {
            using (DBContext) {
                RecAddress rec= DBContext.RecAddress.Where(m => m.ID == RID&&m.UID==UID).FirstOrDefault();
                if (rec==null) {
                    throw new Exception("收货地址不存在");
                }else
                {
                    rec.IsDefault = 1;
                    int i= DBContext.Database.ExecuteSqlCommand("update RecAddress set isDefault=0 where UID='"+UID+"' and ID<>'"+RID+"'");
                    return DBContext.SaveChanges();
                }
            }
        }
        /// <summary>
        /// 新增地址
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="recname"></param>
        /// <param name="sex"></param>
        /// <param name="phone"></param>
        /// <param name="qu"></param>
        /// <param name="detail"></param>
        /// <returns></returns>
        public int Insert(Guid UID, string recname, int sex, string phone, int qu, string detail)
        {
            using (DBContext)
            {
                Web_Area area = DBContext.Web_Area.Where(m => m.ID == qu).FirstOrDefault();
                if (area == null)
                {
                    return -1;
                }
                RecAddress r = new RecAddress();
                r.ID = Guid.NewGuid();
                r.UID = UID;
                r.TrueName = recname;
                r.AreaID = area.Family;
                r.Address = DBContext.Web_Area.Where(m => m.ID == area.HeadID).Select(a => a.Name).FirstOrDefault() + area.Name + "," + detail;
                r.Phone = phone;
                r.Sex = sex;
                r.Status = 1;
                r.CreateTime = DateTime.Now;
                r.UpdateTime = DateTime.Now;
                DBContext.RecAddress.Add(r);
                return DBContext.SaveChanges();
            }
        }
        /// <summary>
        /// 修改收货地址
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="ID"></param>
        /// <param name="recname"></param>
        /// <param name="sex"></param>
        /// <param name="phone"></param>
        /// <param name="qu"></param>
        /// <param name="detail"></param>
        /// <returns></returns>
        public int Update(Guid UID, Guid ID, string recname, int sex, string phone, int qu, string detail)
        {
            using (DBContext)
            {
                RecAddress add = DBContext.RecAddress.Where(m => m.ID == ID && m.UID == UID).FirstOrDefault();
                if (add == null)
                {
                    return -1;
                }
                Web_Area area = DBContext.Web_Area.Where(m => m.ID == qu).FirstOrDefault();
                if (area == null)
                {
                    return -1;
                }
                add.TrueName = recname;
                add.AreaID = area.Family;
                add.Address = (DBContext.Web_Area.Where(m => m.ID == area.HeadID).Select(a => a.Name).FirstOrDefault() + area.Name + "," + detail);
                add.Phone = phone;
                add.Sex = sex;
                add.UpdateTime = DateTime.Now;
                return DBContext.SaveChanges();

            }
        }
        public List<RecAddress> GetUserAddressList(Guid Uid)
        {
            using (DBContext)
            {
                return DBContext.RecAddress.Where(m => m.UID == Uid && m.Status == (int)enStatus.Enabled).ToList();
            }
        }
        public List<RecAddress> GetUserAddressList(string Uid)
        {
            return GetUserAddressList(Guid.Parse(Uid));
        }

        public RecAddress GetAddressDetail(Guid id)
        {
            using (DBContext)
            {
                return DBContext.RecAddress.FirstOrDefault(m => m.ID == id && m.Status == (int)enStatus.Enabled);
            }
        }
        public RecAddress GetAddressDetail(string id)
        {
            return GetAddressDetail(Guid.Parse(id));
        }

        public int InsertAddress(Guid uid, string recname, string areid, string address, string phone, string areacode, string email, int? sex)
        {
            using (DBContext)
            {
                RecAddress model = new RecAddress();
                model.ID = Guid.NewGuid();
                model.UID = uid;
                model.Address = address;
                model.AreaCode = areacode;
                model.AreaID = areid;
                model.Email = email;
                model.Phone = phone;
                model.Sex = sex;
                model.Status = (int)enStatus.Enabled;
                model.TrueName = recname;
                model.UpdateTime = model.CreateTime = DateTime.Now;

                DBContext.RecAddress.Add(model);
                return DBContext.SaveChanges();
            }
        }

        public int DeleteAddress(Guid id)
        {
            using (DBContext)
            {
                var q = DBContext.RecAddress.FirstOrDefault(m => m.ID == id);
                if (q == null)
                    return 0;
                q.Status = (int)enStatus.Enabled;
                return DBContext.SaveChanges();
            }
        }
        public int DeleteAddress(string id)
        {
            return DeleteAddress(Guid.Parse(id));
        }

        public int UpdateAddress(Guid id, string recname, string areid, string address, string phone, string areacode, string email, int? sex)
        {
            using (DBContext)
            {
                RecAddress model = DBContext.RecAddress.FirstOrDefault(m => m.ID == id);
                if (model == null)
                    return 0;
                model.Address = address;
                model.AreaCode = areacode;
                model.AreaID = areid;
                model.Email = email;
                model.Phone = phone;
                model.Sex = sex;
                model.TrueName = recname;
                model.UpdateTime = DateTime.Now;

                return DBContext.SaveChanges();
            }
        }
    }
}
