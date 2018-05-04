using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;
namespace RelexBarBLL
{
    public class UserHelpBLL:BaseBll
    {
        public int Delete(Guid ID) {
            using (DBContext) {
                var uh= DBContext.UserHelp.Where(m => m.ID == ID).FirstOrDefault();
                if (uh==null) {
                    return -1;
                }
                DBContext.UserHelp.Remove(uh);
                return DBContext.SaveChanges();
            }
        }
        public int ChangeStatus(Guid ID,enStatus Status) {
            using (DBContext) {
                UserHelp uh= DBContext.UserHelp.Where(m => m.ID == ID).FirstOrDefault();
                if (uh==null) {
                    return -1;
                }
                uh.Status = (int)Status;
                return DBContext.SaveChanges();
            }
        }
        public List<UserHelp> GetListByType(int Type) {
            using (DBContext) {
                return DBContext.UserHelp.Where(m => m.Type == Type&&m.Status==(int)enStatus.Enabled).ToList();
            }
        }
        public UserHelp Get(Guid ID) {
            using (DBContext) {
                return DBContext.UserHelp.Where(m => m.ID == ID).FirstOrDefault();
            }
        }
        public List<UserHelp> GetList(int index, string key, int pageSize,out int sum) {
            using (DBContext) {
                var q = DBContext.UserHelp.Where(m => 1 == 1);
                if (!string.IsNullOrEmpty(key)) {
                    q = q.Where(m => m.Title.Contains(key));
                }
                return GetPagedList(q.OrderByDescending(m=>m.CreateTime), pageSize, index, out sum);
            }
        }
        public int Add(UserHelp model) {
            using (DBContext) {
                DBContext.UserHelp.Add(model);
                return DBContext.SaveChanges();
            }
        }
        public int Update(Guid ID,string Title,string Content,int? Type,int? Status) {
            using (DBContext) {
                UserHelp uh= DBContext.UserHelp.Where(m=>m.ID==ID).FirstOrDefault();
                if (!string.IsNullOrEmpty(Title)) {
                    uh.Title = Title;
                }
                if (!string.IsNullOrEmpty(Content)) {
                    uh.Content = Content;
                }
                if (Type!=null) {
                    uh.Type = Type.Value;
                }
                if (Status!=null) {
                    uh.Status = Status.Value;
                }
                return DBContext.SaveChanges();
            }
        }
    }
}
