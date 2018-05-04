using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelexBarBLL.Models
{
    public class PayListDetailModel:RelexBarDLL.PayListDetail
    {
        public string CardNumber { get; set; }
        public string Name { get; set; }
        public string TrueName { get; set; }
    }
}
