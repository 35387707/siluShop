using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RelexBarBLL.Models
{
    public class UserLevel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string TrueName { get; set; }
        public string HeadImg { get; set; }
        public int Level { get; set; }
        public string LevelName { get; set; }
        public Guid? Fid { get; set; }
        public Guid? CommendID { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}