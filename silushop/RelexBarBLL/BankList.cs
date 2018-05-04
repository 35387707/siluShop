using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;

namespace RelexBarBLL
{
    public partial class BankListBLL : BaseBll
    {
        public int Update(Guid ID,string BankUser,string BankName,string BankZhiHang,string BankAccount) {
            using (DBContext) {
                BankList bank= DBContext.BankList.Where(m => m.ID == ID).FirstOrDefault();
                if (bank==null) {
                    return -1;
                }
                bank.BankUser = BankUser;
                bank.BankName = BankName;
                bank.BankZhiHang = BankZhiHang;
                bank.BankAccount = BankAccount;
                bank.UpdateTime = DateTime.Now;
                return DBContext.SaveChanges();
            }
        }

        /// <summary>
        /// 获得银行卡数量
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public int GetCardCount(Guid UID) {
            using (DBContext) {
                return DBContext.BankList.Where(m => m.UID == UID).Count();
            }
        }
        /// <summary>
        /// 获取最新的一个银行卡信息
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public BankList GetFirst(Guid UID) {
            using (DBContext) {
                return DBContext.BankList.Where(m => m.UID == UID).OrderByDescending(m=>m.CreateTime).FirstOrDefault();
            }
        }
        /// <summary>
        /// 验证用户是否填写银行卡信息
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public bool HasBankInfo(Guid UID) {
            using (DBContext) {
                return DBContext.BankList.Where(m => m.UID == UID).Count() > 0;
            }
        }

        public List<BankList> GetUserBankList(Guid Uid)
        {
            using (DBContext)
            {
                return DBContext.BankList.Where(m => m.UID == Uid && m.Status == (int)enStatus.Enabled).ToList();
            }
        }
        public BankList GetDetail(Guid ID)
        {
            using (DBContext)
            {
                return DBContext.BankList.FirstOrDefault(m => m.ID == ID);
            }
        }

        public Guid Insert(Guid Uid, string BankName, string BankZhiHang, string BankAccount, string BankUser)
        {
            using (DBContext)
            {
                int i= DBContext.BankList.Where(m => m.UID == Uid).Count();
                if (i>0) {
                    throw new Exception("您已添加银行卡，不能重复添加");
                }
                BankList model = new BankList();
                model.ID = Guid.NewGuid();
                model.UID = Uid;
                model.BankName = BankName;
                model.BankZhiHang = BankZhiHang;
                model.BankAccount = BankAccount;
                model.BankUser = BankUser;
                model.Status = (int)enStatus.Enabled;
                model.CreateTime = model.UpdateTime = DateTime.Now;

                DBContext.BankList.Add(model);
                if (DBContext.SaveChanges() > 0)
                {
                    return model.ID;
                }
                else
                {
                    return Guid.Empty;
                }
            }
        }
        public int Delete(Guid ID)
        {
            using (DBContext)
            {
                var model = DBContext.BankList.FirstOrDefault(m => m.ID == ID);
                if (model != null)
                {
                    model.Status = (int)enStatus.Unabled;
                    model.UpdateTime = DateTime.Now;

                    //DBContext.BankList.Remove(model);
                    return DBContext.SaveChanges();
                }
                return 0;
            }
        }
    }
}
