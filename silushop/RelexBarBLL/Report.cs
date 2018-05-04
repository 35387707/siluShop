using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;

namespace RelexBarBLL
{
    /// <summary>
    /// 报表
    /// </summary>
    public class ReportBLL : BaseBll
    {
        #region mvc新增方法
        public int GetLoginCount(DateTime date)
        {
            using (DBContext)
            {
                date = DateTime.Parse(date.ToString("yyyy-MM-dd"));
                DateTime dtend = date.AddDays(1);
                return DBContext.Logs.Where(m => m.CreateTime.Value > date && m.CreateTime.Value < dtend && m.LogType == (int)enLogType.Login).Select(m => new { UID = m.UID }).GroupBy(m => m.UID).Count();
            }
        }
        public List<Report> GetList(enReportType Type, int Top)
        {
            using (DBContext)
            {
                return DBContext.Report.OrderByDescending(m => m.CreateTime).Take(Top).ToList();
            }
        }
        public int Insert(decimal value, enReportType Type, DateTime? Date)
        {
            using (DBContext)
            {
                Report r = new Report();
                r.Name = Type.ToString();
                r.Value = value;
                r.CountDate = 3;
                r.Type = ((int)Type).ToString();
                r.CreateTime = Date == null ? DateTime.Now : Date.Value;
                DBContext.Report.Add(r);
                return DBContext.SaveChanges();
            }
        }
        #endregion
        public int GetNewUser(DateTime date)
        {
            using (DBContext)
            {
                date = DateTime.Parse(date.ToString("yyyy-MM-dd"));
                DateTime dtend = date.AddDays(1);
                return DBContext.Users.Count(m => m.CreateTime.Value > date && m.CreateTime.Value < dtend);
            }
        }

        public int GetPayedOrder()
        {
            using (DBContext)
            {
                return DBContext.OrderList.Count(m => m.Status == (int)enOrderStatus.Payed);
            }
        }

        /// <summary>
        /// 获取登录数据
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public List<Report> GetLoginReport(int day)
        {
            using (DBContext)
            {
                return DBContext.Report.Where(m => m.Name == enReportType.Login.ToString()).OrderByDescending(m => m.CreateTime).Take(day).ToList();
            }
        }


        /// <summary>
        /// 获取报表数据
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public List<Report> GetReport(int count, enReportType type)
        {
            using (DBContext)
            {
                string typename = type.ToString();
                return DBContext.Report.Where(m => m.Name == typename).OrderByDescending(m => m.CreateTime).Take(count).ToList();
            }
        }
    }
}
