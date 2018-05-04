using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;

namespace RelexBarBLL
{
    /// <summary>
    /// 系统变量
    /// </summary>
    public class SysConfigBLL
    {
        //常用变量：
        private const string MD5KEY = "MD5KEY";
        private const string SMSURL = "SMSURL";
        private const string SMSUSER = "SMSUSER";
        private const string SMSPSW = "SMSPSW";
        private const string EMAILUSER = "EMAILUSER";
        private const string EMAILPSW = "EMAILPSW";
        private const string EMAILSERVER = "EMAILSERVER";
        private const string POUNDAGE = "POUNDAGE";
        private const string TRANSOUT = "TRANSOUT";
        public const string CANMOREACCOUNT = "CANMOREACCOUNT";

        //红包金额设置
        private const string REDLVSMALL1 = "REDLVSMALL1";
        private const string REDLVSMALL2 = "REDLVSMALL2";
        private const string REDLVBIG1 = "REDLVBIG1";
        private const string REDLVBIG2 = "REDLVBIG2";

        //微信参数设置
        private const string WXAPPID = "WXAPPID";
        private const string WXMCHID = "WXMCHID";
        private const string WXKEY = "WXKEY";
        //威富通参数设置
        private const string WFT_APPID = "WFT_APPID";
        private const string WFT_MCHID = "WFT_MCHID";
        private const string WFT_KEY = "WFT_KEY";
        //本地支付购物券比例
        private const string localPay = "LocalPay";

        public static List<SysConfig> SysConfigList;//所有可用的系统配置

        private static string _md5key = string.Empty;
        public static string MD5Key
        {
            get
            {
                return _md5key;
            }
        }

        private static string _SMSUrl = string.Empty;
        public static string SMSUrl
        {
            get
            {
                return _SMSUrl;
            }
        }

        private static string _SMSUser = string.Empty;
        public static string SMSUser
        {
            get
            {
                return _SMSUser;
            }
        }

        private static string _SMSPsw = string.Empty;
        public static string SMSPsw
        {
            get
            {
                return _SMSPsw;
            }
        }

        private static string _EmailUser = string.Empty;
        public static string EmailUser
        {
            get
            {
                return _EmailUser;
            }
        }

        private static string _EmailPsw = string.Empty;
        public static string EmailPsw
        {
            get
            {
                return _EmailPsw;
            }
        }

        private static string _EmailServer = string.Empty;
        public static string EmailServer
        {
            get
            {
                return _EmailServer;
            }
        }

        private static decimal _Transout = 0.05M;//手续费
        /// <summary>
        /// 提现手续费
        /// </summary>
        public static decimal Transout//手续费
        {
            get
            {
                string val= new SysConfigBLL().Get("TRANSOUT");
                if (string.IsNullOrEmpty(val))
                {
                    return _Transout;
                }
                else {
                    decimal retval = 0;
                    if (decimal.TryParse(val, out retval))
                    {
                        return retval;
                    }
                    else {
                        return _Transout;
                    }

                }
                
            }
        }
        private static decimal _Poundage = 0.05M;//手续费
        /// <summary>
        /// 转出手续费
        /// </summary>
        public static decimal Poundage//手续费
        {
            get
            {
                return _Poundage;
            }
        }

        private static decimal _RedLvBig1 = 16M;
        /// <summary>
        /// 金卡红包第一层金额
        /// </summary>
        public static decimal RedLvBig1
        {
            get
            {
                return _RedLvBig1;
            }
        }
        private static decimal _RedLvBig2 = 2M;
        /// <summary>
        /// 金卡红包第二层金额
        /// </summary>
        public static decimal RedLvBig2
        {
            get
            {
                return _RedLvBig2;
            }
        }

        private static decimal _RedLvSmall1 = 10M;
        /// <summary>
        /// 星卡红包第一层金额
        /// </summary>
        public static decimal RedLvSmall1
        {
            get
            {
                return _RedLvSmall1;
            }
        }
        private static decimal _RedLvSmall2 = 1M;
        /// <summary>
        /// 星卡红包第二层金额
        /// </summary>
        public static decimal RedLvSmall2
        {
            get
            {
                return _RedLvSmall2;
            }
        }
        private static decimal _localPay=1;
        public static decimal LocalPay
        {
            get
            {
                return _localPay;
            }
        }
        public void InitConfig()
        {
            SysConfigList = GetAllConfig();//可用的系统配置拿出来

            //特殊配置需要单独处理出来
            var k = SysConfigList.FirstOrDefault(m => m.Name == MD5KEY);
            if (k != null)
                _md5key = k.Value;
            k = SysConfigList.FirstOrDefault(m => m.Name == SMSURL);
            if (k != null)
                _SMSUrl = k.Value;
            k = SysConfigList.FirstOrDefault(m => m.Name == SMSUSER);
            if (k != null)
                _SMSUser = k.Value;
            k = SysConfigList.FirstOrDefault(m => m.Name == SMSPSW);
            if (k != null)
                _SMSPsw = k.Value;
            k = SysConfigList.FirstOrDefault(m => m.Name == EMAILUSER);
            if (k != null)
                _EmailUser = k.Value;
            k = SysConfigList.FirstOrDefault(m => m.Name == EMAILPSW);
            if (k != null)
                _EmailPsw = k.Value;
            k = SysConfigList.FirstOrDefault(m => m.Name == EMAILSERVER);
            if (k != null)
                _EmailServer = k.Value;
            k = SysConfigList.FirstOrDefault(m => m.Name == POUNDAGE);
            if (k != null)
                _Poundage = decimal.Parse(k.Value);
            k = SysConfigList.FirstOrDefault(m => m.Name == TRANSOUT);
            if (k != null)
                _Transout = decimal.Parse(k.Value);

            k = SysConfigList.FirstOrDefault(m => m.Name == REDLVBIG1);
            if (k != null)
                _RedLvBig1 = decimal.Parse(k.Value);
            k = SysConfigList.FirstOrDefault(m => m.Name == REDLVBIG2);
            if (k != null)
                _RedLvBig2 = decimal.Parse(k.Value);
            k = SysConfigList.FirstOrDefault(m => m.Name == REDLVSMALL1);
            if (k != null)
                _RedLvSmall1 = decimal.Parse(k.Value);
            k = SysConfigList.FirstOrDefault(m => m.Name == REDLVSMALL2);
            if (k != null)
                _RedLvSmall2 = decimal.Parse(k.Value);

            //微信接口参数
            k = SysConfigList.FirstOrDefault(m => m.Name == WXAPPID);
            if (k != null)
                Services.WX_Services.WxPayData.wxAppid = k.Value;
            k = SysConfigList.FirstOrDefault(m => m.Name == WXMCHID);
            if (k != null)
                Services.WX_Services.WxPayData.wxMCHID = k.Value;
            k = SysConfigList.FirstOrDefault(m => m.Name == WXKEY);
            if (k != null)
                Services.WX_Services.WxPayData.wxKey = k.Value;
            //威富通接口参数（第三方）
            k = SysConfigList.FirstOrDefault(m => m.Name == WFT_APPID);
            if (k != null)
                Services.weifutong_pay.APPID = k.Value;
            k = SysConfigList.FirstOrDefault(m => m.Name == WFT_MCHID);
            if (k != null)
                Services.weifutong_pay.MCHID = k.Value;
            k = SysConfigList.FirstOrDefault(m => m.Name == WFT_KEY);
            if (k != null)
                Services.weifutong_pay.KEY = k.Value;
            k = SysConfigList.FirstOrDefault(m => m.Name == localPay);
            if (k != null)
            {
                _localPay = decimal.Parse(k.Value);
            }
        }
        
        public List<SysConfig> GetAllConfig(string keyword, int pagesize, int pageinex, out int count)
        {
            using (RelexBarEntities db = new RelexBarEntities())
            {
                var q = db.SysConfig.OrderBy(m => m.ID);
                if (!string.IsNullOrEmpty(keyword))
                {
                    q.Where(m => m.Name.Contains(keyword) || m.Value.Contains(keyword) || m.Descrition.Contains(keyword));
                }
                return BaseBll.GetPagedList2(q, pagesize, pageinex, out count);
            }
        }
        public string Get(string key) {
            using (RelexBarEntities entity=new RelexBarEntities()) {
                SysConfig c = entity.SysConfig.Where(m => m.Name == key).FirstOrDefault();
                return c == null ? null : c.Value;
            }
        }
        public List<SysConfig> GetAllConfig()
        {
            using (RelexBarEntities db = new RelexBarEntities())
            {
                var q = db.SysConfig.Where(m => m.Status == (int)Common.enStatus.Enabled);
                return q.ToList();
            }
        }

        public int Insert(string name, string value, string desc, int status)
        {
            using (RelexBarEntities db = new RelexBarEntities())
            {
                if (db.SysConfig.FirstOrDefault(m => m.Name == name) != null)//存在，不能加入
                {
                    return 0;
                }
                SysConfig config = new SysConfig();
                config.Name = name;
                config.Value = value;
                config.Descrition = desc;
                config.Status = status;
                config.CreateTime = config.UpdateTime = DateTime.Now;

                db.SysConfig.Add(config);

                return db.SaveChanges();
            }
        }
        public int Update(int ID, string value, string desc,int status)
        {
            using (RelexBarEntities db = new RelexBarEntities())
            {
                var model = db.SysConfig.FirstOrDefault(m => m.ID == ID);
                if (model == null)//不存在
                {
                    return 0;
                }
                model.Value = value;
                model.Descrition = desc;
                model.Status = status;
                model.UpdateTime = DateTime.Now;

                return db.SaveChanges();
            }
        }

        public SysConfig Details(int ID)
        {
            using (RelexBarEntities db = new RelexBarEntities())
            {
                var model = db.SysConfig.FirstOrDefault(m => m.ID == ID);
                return model;
            }
        }

        public int ChangeStatus(int ID,Common.enStatus status)
        {
            using (RelexBarEntities db = new RelexBarEntities())
            {
                var model = db.SysConfig.FirstOrDefault(m => m.ID == ID);
                if (model == null)//不存在
                {
                    return 0;
                }
                model.Status = (int)status;
                model.UpdateTime = DateTime.Now;

                return db.SaveChanges();
            }
        }
        #region mvc新增方法
        public int UpdateStatus(int ID, Common.enStatus status)
        {
            using (RelexBarEntities db = new RelexBarEntities())
            {
                var model = db.SysConfig.FirstOrDefault(m => m.ID == ID);
                if (model == null)//不存在
                {
                    return 0;
                }
                model.Status = (int)status;
                model.UpdateTime = DateTime.Now;

                return db.SaveChanges();
            }
        }
        public int Update(int ID, string value, string desc, int? status)
        {
            using (RelexBarEntities db = new RelexBarEntities())
            {
                var model = db.SysConfig.FirstOrDefault(m => m.ID == ID);
                if (model == null)//不存在
                {
                    return 0;
                }
                model.Value = value;
                model.Descrition = desc;
                if (status != null)
                    model.Status = status.Value;
                model.UpdateTime = DateTime.Now;

                return db.SaveChanges();
            }
        }
        #endregion
    }
}
