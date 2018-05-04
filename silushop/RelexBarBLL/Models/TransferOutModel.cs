using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelexBarBLL.Models
{
    public class TransferOutModel
    {
        public Guid UID { get; set; }
        public string Name { get; set; }
        public string TrueName { get; set; }
        public string Phone { get; set; }
        public int UserType { get; set; }
        public int RealCheck { get; set; }
        public Guid ID { get; set; }
        public string CardNumber { get; set; }
        public string BankName { get; set; }
        public string BankZhiHang { get; set; }
        public string BankAccount { get; set; }
        public string BankUser { get; set; }
        public decimal Price { get; set; }
        public decimal ComPrice { get; set; }
        public string Reason { get; set; }
        public string ApplyRemark { get; set; }
        public int? Status { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
