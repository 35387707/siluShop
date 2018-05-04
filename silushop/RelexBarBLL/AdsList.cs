using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;

namespace RelexBarBLL
{
    public partial class AdsListBLL : BaseBll
    {
        public List<AdsList> GetList(int? Location,int? count=null)
        {
            using (DBContext)
            {
                DateTime now = DateTime.Now;
                var q = DBContext.AdsList.Where(m => m.Status == 1 && m.BeginTime <= DateTime.Now && m.EndTime > DateTime.Now);
                if (Location!=null) {
                    q = q.Where(m => m.Location == Location.Value);
                }
                q = q.OrderByDescending(m => m.CreateTime);
                return q.ToList();
            }
        }
        public int Update(Guid ID, string Name, string Title, string Img, string LinkTo, string Descrition
            , DateTime? BeginTime, DateTime? EndTime, int? Location, int? status)
        {
            using (DBContext)
            {
                AdsList ad = DBContext.AdsList.Where(m => m.ID == ID).FirstOrDefault();
                if (ad == null) { return -1; }
                if (!string.IsNullOrEmpty(Name))
                {
                    ad.Name = Name;
                }
                if (!string.IsNullOrEmpty(Title))
                {
                    ad.Title = Title;
                }
                if (!string.IsNullOrEmpty(Img))
                {
                    ad.Img = Img;
                }
                if (!string.IsNullOrEmpty(LinkTo))
                {
                    ad.LinkTo = LinkTo;
                }
                if (!string.IsNullOrEmpty(Descrition))
                {
                    ad.Descrition = Descrition;
                }
                if (BeginTime != null)
                {
                    ad.BeginTime = BeginTime;
                }
                if (EndTime != null)
                {
                    ad.EndTime = EndTime;
                }
                if (Location != null)
                {
                    ad.Location = Location.Value;
                }
                if (status != null)
                {
                    ad.Status = status;
                }
                ad.UpdateTime = DateTime.Now;
                return DBContext.SaveChanges();
            }
        }
        public int Add(string Name, string Title, string Img, string LinkTo, string Descrition
               , DateTime BeginTime, DateTime EndTime,int Location)
        {
            using (DBContext)
            {
                AdsList ad = new AdsList();
                ad.ID = Guid.NewGuid();
                ad.Name = Name;
                ad.Title = Title;
                ad.Img = Img;
                ad.LinkTo = LinkTo;
                ad.Descrition = Descrition;
                ad.BeginTime = BeginTime;
                ad.EndTime = EndTime;
                ad.CreateTime = DateTime.Now;
                ad.UpdateTime = DateTime.Now;
                ad.Status = 1;
                ad.Location = Location;
                DBContext.AdsList.Add(ad);
                return DBContext.SaveChanges();
            }

        }
        public List<AdsList> GetList(int index, int pageSize, string key, out int sum)
        {
            using (DBContext)
            {
                var q = DBContext.AdsList.Where(m => 1 == 1);
                //if (location != null)
                //{
                //    q = q.Where(m => m.Location == location);
                //}
                if (!string.IsNullOrEmpty(key))
                {
                    q = q.Where(m => m.Name.Contains(key) || m.Title.Contains(key));
                }

                return GetPagedList(q.OrderByDescending(m => m.CreateTime), pageSize, index, out sum);
            }
        }
        public List<AdsList> GetUserAdsList()
        {
            using (DBContext)
            {
                return DBContext.AdsList.Where(m => m.Status == (int)enStatus.Enabled).OrderByDescending(m => m.UpdateTime).ToList();
            }
        }
        //public List<AdsList> GetList()
        //{
        //    using (DBContext)
        //    {
        //        return DBContext.AdsList.OrderByDescending(m => m.CreateTime).ToList();
        //    }
        //}
        public AdsList GetDetail(Guid ID)
        {
            using (DBContext)
            {
                return DBContext.AdsList.FirstOrDefault(m => m.ID == ID);
            }
        }
        public int UpdateStatus(Guid ID, enStatus status)
        {
            using (DBContext)
            {
                var model = DBContext.AdsList.FirstOrDefault(m => m.ID == ID);
                if (model == null)
                {
                    return 0;
                }
                model.Status = (int)status;
                model.UpdateTime = DateTime.Now;
                return DBContext.SaveChanges();
            }
        }
        public int Delete(Guid ID)
        {
            using (DBContext)
            {
                var model = DBContext.AdsList.FirstOrDefault(m => m.ID == ID);
                if (model == null)
                {
                    return 0;
                }
                DBContext.AdsList.Remove(model);
                return DBContext.SaveChanges();
            }
        }
        public int Insert(string name, string title, string linkto, string img)
        {
            using (DBContext)
            {
                AdsList model = new AdsList();
                model.ID = Guid.NewGuid();
                model.Name = name;
                model.Title = title;
                model.LinkTo = linkto;
                model.Img = img;
                model.Status = (int)enStatus.Enabled;
                model.CreateTime = model.UpdateTime = DateTime.Now;

                DBContext.AdsList.Add(model);
                return DBContext.SaveChanges();
            }
        }
        public int Update(Guid id, string name, string title, string linkto, string img)
        {
            using (DBContext)
            {
                AdsList model = DBContext.AdsList.FirstOrDefault(m => m.ID == id);
                if (model == null)
                    return 0;

                model.Name = name;
                model.Title = title;
                model.LinkTo = linkto;
                if (!string.IsNullOrEmpty(img))
                    model.Img = img;
                model.UpdateTime = DateTime.Now;

                return DBContext.SaveChanges();
            }
        }
    }
}
