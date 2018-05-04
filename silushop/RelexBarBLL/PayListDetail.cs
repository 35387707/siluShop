using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;
namespace RelexBarBLL
{
    public class PayListDetailBLL:BaseBll
    {
        public List<Models.PayListDetailModel> GetPayListDetail(string name,enPayListType? type, DateTime? beginTime, DateTime? endTime, int index, int pageSize, out int sum) {
            using (DBContext) {
                var q = DBContext.PayListDetail.Join(DBContext.Users, p => p.UID, u => u.ID, (p, u) => new Models.PayListDetailModel {
                    ID = p.ID,
                    UID=p.UID,
                    Price=p.Price,
                    Remark=p.Remark,
                    CreateTime=p.CreateTime,
                    Level=p.Level,
                    Type=p.Type,
                    FromUID=p.FromUID,
                    Name=u.Name,
                    CardNumber=u.CardNumber,
                    TrueName=u.TrueName,
                }).Where(m => 1 == 1);
                if (type!=null) {
                    q = q.Where(m => m.Type == (int)type.Value);
                }
                if (beginTime!=null) {
                    q = q.Where(m => m.CreateTime >= beginTime.Value);
                }
                if (endTime!=null) {
                    q = q.Where(m => m.CreateTime < endTime.Value);
                }
                if (!string.IsNullOrEmpty(name)) {
                    q = q.Where(m => m.Name.Contains(name));
                }
                return this.GetPagedList(q.OrderByDescending(m=>m.CreateTime), pageSize, index,out sum);
            }
        }
    }
}
