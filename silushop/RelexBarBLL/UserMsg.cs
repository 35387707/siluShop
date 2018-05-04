using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;

namespace RelexBarBLL
{
    public partial class UserMsgBLL : BaseBll
    {
        public int SendMsgToAllUser(string subject, string title, string content, enMessageType type)
        {
            using (DBContext)
            {
                DateTime t = DateTime.Now;
                List<Guid> list = DBContext.Users.Select(m => m.ID).ToList();
                foreach (Guid id in list)
                {
                    UserMsg m = new UserMsg();
                    m.ID = Guid.NewGuid();
                    m.UID = id;
                    m.FromUid = Guid.Empty;
                    m.Subject = subject;
                    m.Content = content;
                    m.MessageType = (int)type;
                    m.Img = "";
                    m.Title = title;
                    m.IsShow = 1;
                    m.Status = 1;
                    m.CreateTime = m.UpdateTime = t;
                    DBContext.UserMsg.Add(m);
                }
                return DBContext.SaveChanges();
            }
        }
        public int SendMsg(Guid recuid, string subject, string title, string content, enMessageType type)
        {
            using (DBContext)
            {
                UserMsg m = new UserMsg();
                m.ID = Guid.NewGuid();
                m.UID = recuid;
                m.FromUid = Guid.Empty;
                m.Subject = subject;
                m.Content = content;
                m.MessageType = (int)type;
                m.Img = "";
                m.Title = title;
                m.IsShow = 1;
                m.Status = 1;
                m.CreateTime = m.UpdateTime = DateTime.Now;
                DBContext.UserMsg.Add(m);
                return DBContext.SaveChanges();
            }
        }
        public int Del(Guid id) {
            using (DBContext) {
                UserMsg ms= DBContext.UserMsg.FirstOrDefault(m=>m.ID==id);
                DBContext.UserMsg.Remove(ms);
                return DBContext.SaveChanges();
            }
        }
        public UserMsg Get(Guid ID) {
            using (DBContext) {
                return DBContext.UserMsg.FirstOrDefault(m=>m.ID==ID);
            }
        }
        public int Update(Guid ID,string Title, string Content,int? isShow) {
            using (DBContext) {
                UserMsg m = DBContext.UserMsg.FirstOrDefault(m1 => m1.ID == ID);
                if (m==null) {
                    throw new Exception("数据不存在");
                }
                if (!string.IsNullOrEmpty(Title)) {
                    m.Title = Title;
                }
                if (!string.IsNullOrEmpty(Content)) {
                    m.Content = Content;
                }
                if (isShow!=null) {
                    m.IsShow = isShow.Value;
                }
                return DBContext.SaveChanges();
            }
        }
        public int Add(string Title, string Content) {
            using (DBContext) {
                UserMsg m = new UserMsg();
                m.ID = Guid.NewGuid();
                m.UID = m.FromUid = Guid.Empty;
                m.Title = Title;
                m.Subject = "";
                m.MessageType = (int)enMessageType.TouTiao;
                m.Content = Content;
                m.IsShow = 1;
                m.Status = 1;
                m.CreateTime = m.UpdateTime = DateTime.Now;
                DBContext.UserMsg.Add(m);
                return DBContext.SaveChanges();
            }
        }
        public List<UserMsg> GetList(int index,int pageSize,out int sum,Guid UID,int? isShow,string key,enMessageType? mt) {
            using (DBContext) {
                var q = DBContext.UserMsg.Where(m => m.UID == UID);
                if (isShow != null) {
                    q = q.Where(m => m.IsShow == isShow.Value);
                }
                if (mt!=null) {
                    q = q.Where(m => m.MessageType == (int)mt);
                }
                if (!string.IsNullOrEmpty(key)) {
                    q = q.Where(m=>m.Title.Contains(key));
                }
                return GetPagedList(q.OrderByDescending(m => m.CreateTime), pageSize, index, out sum);
            }
        }

        public List<UserMsg> GetAllList(Guid Uid, int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                var q = DBContext.UserMsg.Where(m => (m.UID == Uid || m.UID == Guid.Empty) && m.IsShow == 1 && m.Status != (int)enMessageState.Unabled);
                return GetPagedList(q.OrderByDescending(m => m.CreateTime), pagesize, pageinex, out count);
            }
        }

        public dynamic GetAllList(string key, int? type, int pagesize, int pageinex, out int count)
        {
            using (DBContext)
            {
                var q = from a in DBContext.UserMsg
                        join b in DBContext.Users on a.UID equals b.ID into T1
                        from c in T1.DefaultIfEmpty()
                        join d in DBContext.Users on a.FromUid equals d.ID into T2
                        from e in T1.DefaultIfEmpty()
                        select new
                        {
                            ID = a.ID,
                            UID = a.UID,
                            FromUid = a.FromUid,
                            Subject = a.Subject,
                            Content = a.Content,
                            MessageType = a.MessageType,
                            Img = a.Img,
                            Title = a.Title,
                            IsShow = a.IsShow,
                            Status = a.Status,
                            CreateTime = a.CreateTime,
                            UpdateTime = a.UpdateTime,

                            UName = c == null ? "所有人" : c.Phone + "【" + c.CardNumber + "】",
                            FName = e == null ? "系统" : e.Phone + "【" + e.CardNumber + "】",
                        };

                if (!string.IsNullOrEmpty(key))
                {
                    q = q.Where(m => m.UName.Contains(key) || m.FName.Contains(key) || m.Subject.Contains(key) ||
                    m.Content.Contains(key) || m.Title.Contains(key));
                }
                if (type.HasValue)
                {
                    q = q.Where(m => m.MessageType == type.Value);
                }

                return GetPagedList(q.OrderByDescending(m => m.CreateTime), pagesize, pageinex, out count);
            }
        }

        public int GetNoReadCount(Guid Uid)
        {
            using (DBContext)
            {
                return DBContext.UserMsg.Count(m => m.IsShow == 1 && m.UID == Uid && m.Status == (int)enMessageState.Enabled);
            }
        }

        public UserMsg GetLastNews(Guid Uid, out int count, out int realStatus)
        {
            using (DBContext)
            {
                realStatus = (int)enMessageState.Unabled;
                count = DBContext.UserMsg.Count(m => m.IsShow == 1 && m.UID == Uid && m.Status == (int)enMessageState.Enabled);
                var model = DBContext.UserMsg.OrderByDescending(m => m.CreateTime).FirstOrDefault(m => m.IsShow == 1 && m.UID == Uid);

                if (model != null && model.Status == (int)enMessageState.Enabled)
                {
                    realStatus = model.Status;
                    model.Status = (int)enMessageState.HadRead;
                    model.UpdateTime = DateTime.Now;
                    DBContext.SaveChanges();
                }
                return model;
            }
        }

        public int Insert(Guid ToUid, Guid FromUid, string subject, string content, enMessageType type, string Img, string Title)
        {
            using (DBContext)
            {
                UserMsg msg = new UserMsg();
                msg.ID = Guid.NewGuid();
                msg.Img = Img;
                msg.UID = ToUid;
                msg.FromUid = FromUid;
                msg.Subject = subject;
                msg.Content = content;
                msg.MessageType = (int)type;
                msg.Title = Title;
                msg.Status = (int)enStatus.Enabled;
                msg.IsShow = (int)enStatus.Enabled;

                msg.CreateTime = msg.UpdateTime = DateTime.Now;

                DBContext.UserMsg.Add(msg);
                return DBContext.SaveChanges();
            }
        }

        public UserMsg GetDetail(Guid ID)
        {
            using (DBContext)
            {
                UserMsg model = DBContext.UserMsg.FirstOrDefault(m => m.ID == ID);
                if (model != null && model.Status == (int)enMessageState.Enabled)
                {
                    model.Status = (int)enMessageState.HadRead;
                    model.UpdateTime = DateTime.Now;
                    DBContext.SaveChanges();
                }
                return model;
            }
        }

    }
}
