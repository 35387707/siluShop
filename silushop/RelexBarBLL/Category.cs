using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;

namespace RelexBarBLL
{
    public partial class CategoryBLL : BaseBll
    {
        public List<ProductList> GetProductByCID(int CID,int count) {
            using (DBContext) {
                return DBContext.ProductList.Where(m => m.CategoryID == CID).OrderByDescending(m => m.OrderID).Take(count).ToList();
            }
        }

        public List<Category> GetAllList(enStatus? status=null)
        {
            using (DBContext)
            {
                if (status==null) {
                    return DBContext.Category.OrderByDescending(m => m.OrderID).ToList();
                }
                return DBContext.Category.Where(m => m.IsShow == (int)status.Value).OrderByDescending(m=>m.OrderID).ToList();
            }
        }

        public int Add(string Name, string Title, string SrcDetail, string Description,int OrderID,int IsShow,int HeadID) {
            using (DBContext) {
                Category c = new Category();
                c.Name = Name;
                c.HeadID = 0;
                c.LV = 1;
                c.OrderID = OrderID;
                c.Title = Title;
                c.IsShow = IsShow;
                c.SrcDetail = SrcDetail;
                c.HeadID = HeadID;
                DBContext.Category.Add(c);
                return DBContext.SaveChanges();
            }
        }
        public List<Category> GetAllList(bool? isShow, string name, int? headid)
        {
            using (DBContext)
            {
                var q = DBContext.Category.AsEnumerable();
                if (string.IsNullOrEmpty(name))
                {
                    q = q.Where(m => m.Name.Contains(name));
                }
                if (headid.HasValue)
                {
                    q = q.Where(m => m.HeadID == headid.Value);
                }
                if (isShow.HasValue)
                {
                    q = q.Where(m => m.IsShow == (isShow.Value ? 1 : 0));
                }
                return q.ToList();
            }
        }

        public int Insert(int HeadID, string name, int IsShow, string Keywords, string title, int order)
        {
            using (DBContext)
            {
                string Family = string.Empty;
                int lv = 0;
                if (HeadID != 0)
                {
                    var fcategory = DBContext.Category.FirstOrDefault(m => m.ID == HeadID);
                    if (fcategory != null)
                    {
                        Family = fcategory.Family;
                        lv = fcategory.LV.Value;
                    }
                }

                Family += HeadID + ",";
                lv++;

                Category model = new Category();
                model.IsShow = IsShow;
                model.Name = name;
                model.HeadID = HeadID;
                model.Keywords = Keywords;
                model.Title = title;
                model.Family = Family;
                model.LV = lv;
                model.OrderID = order;

                DBContext.Category.Add(model);
                return DBContext.SaveChanges();
            }
        }

        public Category GetDetail(int CID)
        {
            using (DBContext)
            {
                return DBContext.Category.FirstOrDefault(m => m.ID == CID);
            }
        }

        public string GetName(int CID)
        {
            using (DBContext)
            {
                var q = DBContext.Category.FirstOrDefault(m => m.ID == CID);
                if (q == null)
                {
                    return "";
                }
                return q.Name;
            }
        }

        public int Update(int ID,string Name, string Title, string SrcDetail, string Description, int OrderID,int IsShow, int HeadID) {
            using (DBContext) {
                Category c= DBContext.Category.Where(m => m.ID == ID).FirstOrDefault();
                if (c==null) {
                    throw new Exception("分类不存在");
                }
                if (c.ID == HeadID)
                {
                    throw new Exception("父类不能为自己!");
                }
                c.Name = Name;
                c.IsShow = IsShow;
                c.Title = Title;
                c.SrcDetail = SrcDetail;
                c.Description = Description;
                c.OrderID = OrderID;
                c.HeadID = HeadID;
                return DBContext.SaveChanges();
            }
        }


        public int Update(int CID, string name, int headid, int isshow, int order)
        {
            using (DBContext)
            {
                var model = DBContext.Category.FirstOrDefault(m => m.ID == CID);
                if (model == null)
                {
                    return 0;
                }
                if (headid == CID)
                {
                    return 0;
                }
                if (model.HeadID != headid)//父类发生更改
                {
                    var headc = DBContext.Category.FirstOrDefault(m => m.ID == headid);
                    string family = "0,";
                    int lv = 1;

                    if (headc != null)//付类别不存在
                    {
                        family = headc.Family + CID + ",";
                        lv = headc.LV.Value + 1;
                    }

                    model.HeadID = headid;
                    model.LV = lv;
                    model.Family = family;

                    var childC = DBContext.Category.Where(m => m.HeadID == CID).ToList();
                    if (childC != null)
                    {
                        childC.ForEach(m => { m.LV = lv + 1; m.Family = family + m.ID + ","; });
                    }
                }

                model.Name = name;
                model.IsShow = isshow;
                model.OrderID = order;

                return DBContext.SaveChanges();
            }
        }
    }
}
