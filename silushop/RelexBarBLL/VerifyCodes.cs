using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;

namespace RelexBarBLL
{
    /// <summary>
    /// 验证码相关操作
    /// </summary>
    public class VerifyCodesBLL : BaseBll
    {
        /// <summary>
        /// 插入一条验证码到数据库保存
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="Code"></param>
        /// <param name="CodeType"></param>
        /// <returns></returns>
        public int InsertCode(Guid UID, string Code, enCodeType CodeType)
        {
            using (DBContext)
            {
                VerifyCodes model = new VerifyCodes();
                model.ID = Guid.NewGuid();
                model.UID = UID;
                model.Code = Code;
                model.MaxHitTime = 3;
                model.HitTime = 0;
                model.CodeType = (int)CodeType;
                model.Status = (int)enStatus.Enabled;
                model.CreateTime = model.UpdateTime = DateTime.Now;

                DBContext.VerifyCodes.Add(model);
                return DBContext.SaveChanges();
            }
        }

        public Guid InsertPayCode(Guid UID, enPayType PayType)
        {
            using (DBContext)
            {
                VerifyCodes model = new VerifyCodes();
                model.ID = Guid.NewGuid();
                model.UID = UID;
                model.Code = "1234";
                model.MaxHitTime = 3;
                model.HitTime = (int)PayType;
                model.CodeType = (int)enCodeType.Pay;
                model.Status = (int)enStatus.Enabled;
                model.CreateTime = model.UpdateTime = DateTime.Now;

                DBContext.VerifyCodes.Add(model);
                DBContext.SaveChanges();

                return model.ID;
            }
        }

        /// <summary>
        /// 校验验证码是否正确
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="inputCode"></param>
        /// <returns></returns>
        public int CheckCode(Guid UID, string inputCode, enCodeType CodeType)
        {
            using (DBContext)
            {
                var model = DBContext.VerifyCodes.OrderByDescending(m => m.CreateTime)
                    .FirstOrDefault(m => m.UID == UID && m.CodeType == (int)CodeType);
                if (model == null || model.Status != (int)enStatus.Enabled)
                {
                    return (int)ErrorCode.请先获取验证码;
                }
                if (DateTime.Now > model.CreateTime.Value.AddMinutes(5))
                {
                    return (int)ErrorCode.验证码已过期;
                }
                if (model.HitTime >= model.MaxHitTime)
                {
                    return (int)ErrorCode.验证码错误次数过多;
                }
                model.HitTime++;
                int result = 0;
                if (model.Code != inputCode)
                {
                    result = (int)ErrorCode.验证码不正确;
                }
                else
                {
                    model.Status = (int)enStatus.Unabled;
                }

                DBContext.SaveChanges();
                return result;
            }
        }

        /// <summary>
        /// 校验线下支付的验证码是否已使用
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="inputCode"></param>
        /// <returns></returns>
        public int CheckCode(Guid UID, Guid ID, out enPayType PayType)
        {
            using (DBContext)
            {
                PayType = enPayType.All;
                var model = DBContext.VerifyCodes.OrderByDescending(m => m.CreateTime)
                    .FirstOrDefault(m => m.UID == UID && m.CodeType == (int)enCodeType.Pay);
                if (model == null || model.Status != (int)enStatus.Enabled || model.ID != ID)
                {
                    return (int)ErrorCode.请先获取验证码;
                }
                if (DateTime.Now > model.CreateTime.Value.AddMinutes(1))
                {
                    return (int)ErrorCode.验证码已过期;
                }
                model.Status = (int)enStatus.Unabled;
                PayType = (enPayType)model.HitTime.Value;

                return DBContext.SaveChanges();
            }
        }

        public int GetCodeStatus(Guid ID)
        {
            using (DBContext)
            {
                var model = DBContext.VerifyCodes.FirstOrDefault(m => m.ID == ID);
                if (model != null)
                {
                    return model.Status.Value;
                }
                else
                { return 0; }
            }
        }
    }
}
