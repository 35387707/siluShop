using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelexBarBLL.Models
{
    public class AdminTicket
    {
        public string CardNumber { get; set; }
        public string Name { get; set; }
        public string PName { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
