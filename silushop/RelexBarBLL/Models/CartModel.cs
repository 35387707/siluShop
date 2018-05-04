using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelexBarBLL.Models
{
    public class CartModel
    {
        public Guid ID { get; set; }
        public Guid ProductID { get; set; }
        public int Count { get; set; }
        public string Img { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public Guid ShopID { get; set; }
        public string ShopName { get; set; }
    }
}
