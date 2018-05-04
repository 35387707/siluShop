using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;
using System.Web;

namespace RelexBarBLL
{
    public partial class LogsBLL
    {
        private RelexBarEntities _dbcontext = null;
        private RelexBarEntities DBContext
        {
            get
            {
                if (_dbcontext.IsDispose)
                    _dbcontext = new RelexBarEntities();
                return _dbcontext;
            }
        }

        public LogsBLL()
        {
            _dbcontext = new RelexBarEntities();
        }

        public int InsertLog(Exception ex, Common.enLogType ltype)
        {
            return InsertLog(ex.ToString(), ltype, HttpContext.Current.Request.Path, HttpContext.Current.Request.UserHostAddress);
        }

        public int InsertLog(string msg, Common.enLogType ltype, string pagename, string ip)
        {
            using (DBContext)
            {
                Logs log = new Logs();
                log.UpdateTime = log.CreateTime = DateTime.Now;
                log.LogType = (int)ltype;
                log.Page = pagename;
                log.Remark = msg;
                log.Ip = ip;
                DBContext.Logs.Add(log);
                return DBContext.SaveChanges();
            }
        }

        public int InsertLog(string msg, Common.enLogType ltype)
        {
            return InsertLog(msg, ltype, HttpContext.Current.Request.Path, HttpContext.Current.Request.UserHostAddress);
        }
        public int InsertLog(Guid UID, string msg, Common.enLogType ltype, string pagename, string ip)
        {
            using (DBContext)
            {
                Logs log = new Logs();
                log.UID = UID;
                log.UpdateTime = log.CreateTime = DateTime.Now;
                log.LogType = (int)ltype;
                log.Page = pagename;
                log.Remark = msg;
                log.Ip = ip;
                DBContext.Logs.Add(log);
                return DBContext.SaveChanges();
            }
        }
        public int InsertLog(Guid UID, string msg, Common.enLogType ltype)
        {
            return InsertLog(UID, msg, ltype, HttpContext.Current.Request.Path, HttpContext.Current.Request.UserHostAddress);
        }
        public int InsertLog(Logs log)
        {
            using (DBContext)
            {
                DBContext.Logs.Add(log);
                return DBContext.SaveChanges();
            }
        }

        public int InsertServiceLog(string Payment, decimal PayPrice, string PayNumber, string OrderNumber, string ReqStr, string RespStr, string Remark)
        {
            using (DBContext)
            {
                OtherPayServiceLog log = new OtherPayServiceLog();
                log.UpdateTime = log.CreateTime = DateTime.Now;
                log.Page = string.Empty;
                log.Payment = Payment;
                log.PayPrice = PayPrice;
                log.PayNumber = PayNumber;
                log.OrderNumber = OrderNumber;
                log.ReqStr = ReqStr;
                log.RespStr = RespStr;
                log.Remark = Remark;
                DBContext.OtherPayServiceLog.Add(log);
                return DBContext.SaveChanges();
            }
        }

    }
}
