using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;

namespace RelexBarBLL
{
    public partial class BaseBll : Common
    {
        private RelexBarEntities _dbcontext = null;
        protected RelexBarEntities DBContext
        {
            get
            {
                if (_dbcontext.IsDispose)
                    _dbcontext = new RelexBarEntities();
                return _dbcontext;
            }
        }

        private LogsBLL _logBll = null;
        protected LogsBLL logBll
        {
            get { return _logBll; }
        }

        public BaseBll()
        {
            _dbcontext = new RelexBarEntities();
            _logBll = new LogsBLL();
        }

        public static List<T> GetPagedList2<T>(IQueryable<T> lamda, int pagesize, int pageinex, out int count)
        {
            count = lamda.Count();
            return GetPagedList2(lamda, pagesize, pageinex);
        }
        public static List<T> GetPagedList2<T>(IQueryable<T> lamda, int pagesize, int pageinex)
        {
            pageinex = GetPageIndex(pageinex);
            return lamda.Skip(pageinex * pagesize).Take(pagesize).ToList();
        }


        protected List<T> GetPagedList<T>(IQueryable<T> lamda, int pagesize, int pageinex, out int count)
        {
            count = lamda.Count();
            return GetPagedList(lamda, pagesize, pageinex);
        }
        protected List<T> GetPagedList<T>(IQueryable<T> lamda, int pagesize, int pageinex)
        {
            pageinex = GetPageIndex(pageinex);
            return lamda.Skip(pageinex * pagesize).Take(pagesize).ToList();
        }

        public int ExceSql(string sql, params object[] paras)
        {
            using (DBContext)
            {
                return DBContext.Database.ExecuteSqlCommand(sql, paras);
            }
        }

        public System.Data.DataTable GetDataTable(string sql, params object[] paras)
        {
            using (DBContext)
            {
                var rows = DBContext.Database.SqlQuery<System.Data.DataRow>(sql, paras).ToArray();
                System.Data.DataTable dt = new System.Data.DataTable();
                dt.Rows.Add(rows);
                return dt;
            }
        }
    }
}
