using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelexBarBLL.Models
{
    public class FavoritesModel
    {
        public Guid ID { get; set; }
        public Guid PID { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Img { get; set; }
        public decimal Price { get; set; }
    }
}
