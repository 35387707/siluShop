using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelexBarBLL.Models
{
    public class MobileProductListModel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        //public string Number { get; set; }
        public string Img { get; set; }
        //public string ImgList { get; set; }
        public string Descrition { get; set; }
        public decimal Price { get; set; }
        public int Payed { get; set; }
        public string ShopName { get; set; }
        public decimal RealPrice { get; set; }
    }
}
