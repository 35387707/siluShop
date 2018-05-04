using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RelexBarDLL;
namespace silushop.Models
{
    public class CateGoryProductModel
    {
        public Category category { get; set; }
        public List<ProductList> PList { get; set; }
    }
}