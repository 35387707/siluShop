using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelexBarBLL.Models
{
    public class MyTicket
    {
        public Guid ID { get; set; }
        public decimal Price { get; set; }
        public DateTime CreateTime { get; set; }
        public int Status { get; set; }
        public Guid PID { get; set; }//商品id
        public int IsBuyProduct { get; set; }//是否兑换商品
        public Guid? OPID { get; set; }
        public Guid? OrderID { get; set; }
        public int? OrderStatus { get; set; }
    }
}
