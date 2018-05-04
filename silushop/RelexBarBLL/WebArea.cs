using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;

namespace RelexBarBLL
{
    public partial class WebAreaBll : BaseBll
    {
        public List<Web_Area> Areas()
        {
            using (DBContext)
                return DBContext.Web_Area.OrderBy(m => m.HeadID).ThenBy(m => m.Name).ToList();
        }
        public List<Web_Area> Areas(int fid)
        {
            using (DBContext)
                return DBContext.Web_Area.Where(m => m.HeadID == fid).OrderBy(m => m.ID).ThenBy(m => m.HeadID).ToList();
        }
        public Web_Area Detail(string id)
        {
            int aid;
            if (!int.TryParse(id, out aid))
            {
                return null;
            }
            return Detail(aid);
        }
        public Web_Area Detail(int id)
        {
            using (DBContext)
                return DBContext.Web_Area.FirstOrDefault(m => m.ID == id);
        }
        public List<Web_Area> Areas(string family)
        {
            using (DBContext)
                return DBContext.Web_Area.Where(m => m.Family.Contains(family + ",")).OrderBy(m => m.ID).ThenBy(m => m.HeadID).ToList();
        }
    }
}
