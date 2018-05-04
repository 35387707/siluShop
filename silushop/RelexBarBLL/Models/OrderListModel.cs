using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelexBarBLL.Models
{
    public class OrderListModel
    {
        public Guid ID { get; set; }
        public Guid OPID { get; set; }
        public string Number { get; set; }
        public DateTime CreateTime { get; set; }
        public int Status { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public string Name { get; set; }
        public string Img { get; set; }
        public string CateName { get; set; }
        public string RecName { get; set; }
        public string RecPhone { get; set; }
        public string RecAddress { get; set; }
        public int Type { get; set; }
        public int Count { get; set; }
        public int OrderType { get; set; }
    }
}
