using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;
namespace RelexBarBLL
{
    public class HolidaysBLL:BaseBll
    {
        public bool ToDayIsHolidays() {
            using (DBContext) {
                //查询今天日是否时节假日
                DateTime now = DateTime.Now;
                //now=now.AddDays(-1); 
                DateTime date = new DateTime(now.Year,now.Month,now.Day);
                Holidays h = DBContext.Holidays.Where(m => m.Date == date).FirstOrDefault();
                return h!=null;
            }
        }
        /// <summary>
        /// 查询是否是节假日
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        public bool ToDayIsHolidays(DateTime now)
        {
            using (DBContext)
            {
                now.AddDays(-1);
                DateTime date = new DateTime(now.Year, now.Month, now.Day);
                Holidays h = DBContext.Holidays.Where(m => m.Date == date).FirstOrDefault();
                return h != null;
            }
        }
        public int UpdateHolidays(int year, List<Models.HolidaysModel> list) {
            using (DBContext) {
                try
                {
                    DateTime begin = new DateTime(year,1,1);
                    DateTime end = begin.AddYears(1);
                    List<Holidays> rlist= DBContext.Holidays.Where(m => m.Date >= begin && m.Date < end).ToList();
                    DBContext.Holidays.RemoveRange(rlist);
                    List<Holidays> alist = new List<Holidays>();
                    foreach (var item in list)
                    {
                        alist.Add(new Holidays() { Date=item.Date,Remark=item.Remark,CreateTime=DateTime.Now});
                    }
                    DBContext.Holidays.AddRange(alist);
                    return DBContext.SaveChanges();
                }
                catch (Exception )
                {

                    throw;
                }
            }
        }
        public List<Holidays> GetHolidaysByYear(int year) {
            using (DBContext) {
                DateTime begin = new DateTime(year,1,1);
                DateTime end = begin.AddYears(1);
                return DBContext.Holidays.Where(m => m.Date >= begin && m.Date < end).ToList();
            }
        }
    }
}
