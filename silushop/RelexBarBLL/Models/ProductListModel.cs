using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelexBarBLL.Models
{
    public class ProductListModel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Number { get; set; }
        public int? TasteID { get; set; }
        public int? CategoryID { get; set; }
        public string Img { get; set; }
        public string ImgList { get; set; }
        public string Descrition { get; set; }
        public decimal RealPrice { get; set; }
        public int PriceType { get; set; }
        public decimal Stock { get; set; }
        public decimal Price { get; set; }
        public int OrderID { get; set; }
        public int Type { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Status { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public Guid? ShopID { get; set; }
        public string ShopName { get; set; }
        public string ShopTrueName { get; set; }
        public int ShopStatus { get; set; }
        public string ShopAddress { get; set; }
        public string ShopPhone { get; set; }
        public string TasteName { get; set; }
        public string CategoryName { get; set; }
        public int? CategoryShow { get; set; }
        public int Payed { get; set; }
    }
}
