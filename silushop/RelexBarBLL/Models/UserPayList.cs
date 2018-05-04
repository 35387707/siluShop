using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelexBarBLL.Models
{
    public class UserPayList
    {
        public Guid UID { get; set; }
        public int InOut { get; set; }
        public decimal Val { get; set; }
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
