using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RelexBarBLL
{
    public class Common
    {
        #region Fields

        /// <summary>
        /// 最顶层的用户ID，也就是为空的
        /// </summary>
        public const string TOPUSER = "00000000-0000-0000-0000-000000000000";

        #endregion

        #region Verify

        /// <summary>
        /// 是否手机号码
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public static bool IsPhone(string phone)
        {
            return !string.IsNullOrWhiteSpace(phone) && phone.Length == 11 && phone.StartsWith("1");
        }

        /// <summary>
        /// 是否邮箱号码
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsEmail(string email)
        {
            return new System.Text.RegularExpressions.Regex("^[a-z0-9_-\\.]+@[a-z0-9_-]+(\\.[a-z0-9_-]+)+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase).IsMatch(email);
        }

        #endregion

        #region Function

        public static int GetPageIndex(int pageinex)
        {
            return pageinex > 0 ? (pageinex - 1) : 0;
        }

        public static string MD5(string source)
        {
            return CommonClass.EncryptDecrypt.GetMd5Hash(source + SysConfigBLL.MD5Key);
        }

        public static string MD5(string source, string key)
        {
            return CommonClass.EncryptDecrypt.GetMd5Hash(source + key);
        }

        public static string Encrypt(string source)
        {
            return CommonClass.EncryptDecrypt.DESEncrypt(source, SysConfigBLL.MD5Key);
        }

        public static string Decrypt(string source)
        {
            return CommonClass.EncryptDecrypt.DESDecrypt(source, SysConfigBLL.MD5Key);
        }

        /// <summary>
        /// 加密，并加上时间戳
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string EncryptWithTime(string source)
        {
            return Encrypt(source + "||ts=" + GetTimeStamp(DateTime.Now));
        }

        /// <summary>
        /// 加密，并加上时间戳
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string EncryptWithTime(string source, DateTime dtVal)
        {
            return Encrypt(source + "||ts=" + GetTimeStamp(dtVal));
        }

        /// <summary>
        /// 解密，并判断时间戳
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string DecryptWithTime(string source, out DateTime dtVal)
        {
            string result = Decrypt(source);
            dtVal = DateTime.MinValue;
            if (!string.IsNullOrEmpty(result))
            {
                int ls = result.LastIndexOf("||ts=");
                if (ls > 0)
                {
                    string ts = result.Substring(ls + 5);
                    if (!string.IsNullOrEmpty(ts))
                    {
                        dtVal = GetTime(ts);
                    }
                    result = result.Substring(0, ls);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            return GetTimeStamp(DateTime.Now);
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp(DateTime val)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return ((int)(val - startTime).TotalSeconds).ToString();
        }

        /// <summary>  
        /// 时间戳转为C#格式时间  
        /// </summary>  
        /// <param name="timeStamp">Unix时间戳格式</param>  
        /// <returns>C#格式时间</returns>  
        public static DateTime GetTime(string timeStamp)
        {
            try
            {
                DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                long lTime = long.Parse(timeStamp + "0000000");
                TimeSpan toNow = new TimeSpan(lTime);
                return dtStart.Add(toNow);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// 获取商品流水号
        /// </summary>
        /// <returns></returns>
        public static string GetNumer()
        {
            return "PD" + new Random().Next(100, 999) + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        /// <summary>
        /// 获取购物单流水号
        /// </summary>
        /// <returns></returns>
        public static string GetOrderNumer()
        {
            return "OD" + new Random().Next(100, 999) + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        /// <summary>
        /// 获取第三方购物单流水号
        /// </summary>
        /// <returns></returns>
        public static string GetServiceNumer()
        {
            return "SN" + new Random().Next(100, 999) + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        public static string GetUserShowName(RelexBarDLL.Users user)
        {
            if (user == null)
                return string.Empty;
            return user.RealCheck == (int)enRealCheckStatus.已验证 ? user.TrueName : HidePhone(user.Phone);
        }

        public static string HideSomeChar(string source, int begin, int length)
        {
            return HideSomeChar(source, begin, length, source.Length, '*');
        }
        public static string HideSomeChar(string source, int begin, int length, int maxLength, char replaceChar)
        {
            if (maxLength < source.Length)
            {
                source = source.Substring(0, maxLength);
            }

            string result = string.Empty;
            if (source.Length > begin && length > 0 && source.Length >= begin + length)
            {
                result = source.Substring(0, begin);//开头
                result = result.PadRight(begin + length, replaceChar);
                result += source.Substring(begin + length); //结尾
            }
            else
            {
                result = source;
            }

            return result;
        }
        public static string HidePhone(string phone)
        {
            return HideSomeChar(phone, 3, 4);
        }
        public static string HideBankAccount(string bankaccount)
        {
            return HideSomeChar(bankaccount, 0, bankaccount.Length - 4);
        }

        /// <summary>
        /// 获取随机验证码
        /// </summary>
        /// <param name="len">验证码字数</param>
        /// <param name="numAndchar">是否英文加字母（默认为纯数字）</param>
        /// <returns></returns>
        public static string GetRandomCode(int len, bool numAndchar = false)
        {
            string result = string.Empty;
            string code = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int maxInt = numAndchar ? (code.Length - 1) : 9;
            Random rd = new Random();
            for (int i = 0; i < len; i++)
            {
                result += code[rd.Next(0, maxInt)];
            }
            return result;
        }

        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <param name="code"></param>
        /// <param name="recphone"></param>
        /// <param name="len"></param>
        /// <param name="numAndchar"></param>
        /// <returns></returns>
        public static bool SendSmsVerify(out string code, string recphone, int len = 6, bool numAndchar = false)
        {
            code = GetRandomCode(len, numAndchar);
            return SendSmsVerify(code, recphone);
        }
        public static bool SendSmsVerify(string code, string recphone)
        {
            //return new ThirdServices().SendSms(recphone, "【丝路联盟】您本次操作的验证码为 " + code + " ,10分钟有效,请不要告诉任何人。");
            return new ThirdServices().SendSms(recphone, code);
        }

        /// <summary>
        /// 发送图片验证码
        /// </summary>
        /// <param name="code"></param>
        /// <param name="len"></param>
        /// <param name="numAndchar"></param>
        /// <returns></returns>
        public static System.Drawing.Bitmap SendImgVerify(out string code, int len = 6, bool numAndchar = false)
        {
            code = GetRandomCode(len, numAndchar);
            return SendImgVerify(code);
        }
        public static System.Drawing.Bitmap SendImgVerify(string code)
        {
            //int width = Convert.ToInt32(code.Length * 12);    //计算图像宽度
            //System.Drawing.Bitmap img = new System.Drawing.Bitmap(width, 23);
            //System.Drawing.Graphics gfc = System.Drawing.Graphics.FromImage(img);//产生Graphics对象，进行画图
            //gfc.Clear(System.Drawing.Color.White);
            //drawLine(gfc, img);
            ////写验证码，需要定义Font
            //System.Drawing.Font font = new System.Drawing.Font("arial", 12, System.Drawing.FontStyle.Bold);
            //System.Drawing.Drawing2D.LinearGradientBrush brush =
            //    new System.Drawing.Drawing2D.LinearGradientBrush(new System.Drawing.Rectangle(0, 0, img.Width, img.Height),
            //    System.Drawing.Color.DarkOrchid, System.Drawing.Color.Blue, 1.5f, true);
            //gfc.DrawString(code, font, brush, 3, 2);
            //drawPoint(img);
            //gfc.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.DarkBlue), 0, 0, img.Width - 1, img.Height - 1);

            //gfc.Dispose();
            //return img;
            return new Models.VerifyCode().CreateImageCode(code);
        }
        private static void drawLine(System.Drawing.Graphics gfc, System.Drawing.Bitmap img)
        {
            Random ran = new Random();
            //选择画10条线,也可以增加，也可以不要线，只要随机杂点即可
            for (int i = 0; i < 10; i++)
            {
                int x1 = ran.Next(img.Width);
                int y1 = ran.Next(img.Height);
                int x2 = ran.Next(img.Width);
                int y2 = ran.Next(img.Height);
                gfc.DrawLine(new System.Drawing.Pen(System.Drawing.Color.Silver), x1, y1, x2, y2);      //注意画笔一定要浅颜色，否则验证码看不清楚
            }
        }
        private static void drawPoint(System.Drawing.Bitmap img)
        {
            Random ran = new Random();
            /*
            //选择画100个点,可以根据实际情况改变
            for (int i = 0; i < 100; i++)
            {
                int x = ran.Next(img.Width);
                int y = ran.Next(img.Height);
                img.SetPixel(x,y,Color.FromArgb(ran.Next()));//杂点颜色随机
            }
             */
            int col = ran.Next();//在一次的图片中杂店颜色相同
            for (int i = 0; i < 100; i++)
            {
                int x = ran.Next(img.Width);
                int y = ran.Next(img.Height);
                img.SetPixel(x, y, System.Drawing.Color.FromArgb(col));
            }
        }

        public static System.Drawing.Bitmap GetQrCodeImg(string contents)
        {
            CommonClass.QRCode qr = new CommonClass.QRCode();
            return GetQrCodeImgAndLogo(contents, "");
        }
        public static System.Drawing.Bitmap GetQrCodeImgAndLogo(string contents, string logopath)
        {
            CommonClass.QRCode qr = new CommonClass.QRCode();
            return qr.EncodetoBitmap(contents, com.google.zxing.BarcodeFormat.QR_CODE, logopath);
        }
        public static void GetQrCodeImg(string contents, string path)
        {
            CommonClass.QRCode qr = new CommonClass.QRCode();
            qr.EncodeToFile(contents, path);
        }
        public static string GetQrCodeValue(string path)
        {
            CommonClass.QRCode qr = new CommonClass.QRCode();
            return qr.Decode(path);
        }

        #endregion

        #region Enum
        public enum enPayListType
        {
            /// <summary>
            /// 固定每日工资
            /// </summary>
            GuDing = 0,
            /// <summary>
            /// 下级奖励
            /// </summary>
            XiaJi = 1,
            /// <summary>
            /// 团队奖励
            /// </summary>
            TuanDui = 2,
            /// <summary>
            /// 开户奖励
            /// </summary>
            KaiHu = 3,
            /// <summary>
            /// 首购券
            /// </summary>
            SGQ = 4,
            /// <summary>
            /// 购物券
            /// </summary>
            ShoppingVoucher = 5,
            /// <summary>
            /// 支付宝
            /// </summary>
            ALi = 6,
            /// <summary>
            /// 线下支付
            /// </summary>
            OutLinePay = 7,
            /// <summary>
            /// 小区奖励
            /// </summary>
            CommunityRewards = 8,

        }
        /// <summary>
        /// 可用状态
        /// </summary>
        public enum enStatus
        {
            /// <summary>
            /// 可用
            /// </summary>
            Enabled = 1,
            /// <summary>
            /// 不可用
            /// </summary>
            Unabled = 0,
        }

        /// <summary>
        /// 消息状态
        /// </summary>
        public enum enMessageState
        {
            /// <summary>
            /// 可用
            /// </summary>
            Enabled = 1,
            /// <summary>
            /// 不可用
            /// </summary>
            Unabled = 0,
            /// <summary>
            /// 已读
            /// </summary>
            HadRead = 2,
        }

        /// <summary>
        /// 消息类型
        /// </summary>
        public enum enMessageType
        {
            /// <summary>
            /// 系统消息
            /// </summary>
            System = 0,
            /// <summary>
            /// 用户消息
            /// </summary>
            Customer = 1,
            /// <summary>
            /// 头条
            /// </summary>
            TouTiao = 2,
            /// <summary>
            /// 其他消息
            /// </summary>
            Other = 3,
        }

        /// <summary>
        /// 充值卡类型
        /// </summary>
        public enum enCardType
        {
            /// <summary>
            /// 普通用户
            /// </summary>
            普通用户 = 0,
            /// <summary>
            /// 星卡
            /// </summary>
            轻客星卡 = 1,
            /// <summary>
            /// 金卡
            /// </summary>
            轻客金卡 = 2,
        }

        /// <summary>
        /// 红包状态
        /// </summary>
        public enum enPacketStatus
        {
            /// <summary>
            /// 未激活
            /// </summary>
            NoActive = 0,
            /// <summary>
            /// 已激活
            /// </summary>
            Actived = 1,
            /// <summary>
            /// 资格卡激活完毕
            /// </summary>
            ActivedAll = 2,
            /// <summary>
            /// 已领取
            /// </summary>
            Used = 3,
        }

        /// <summary>
        /// 红包层级（一级还是二级？）
        /// </summary>
        public enum enPacketLV
        {
            /// <summary>
            /// 大红包（一层）
            /// </summary>
            First = 1,
            /// <summary>
            /// 小红包（二层）
            /// </summary>
            Second = 2,
        }

        /// <summary>
        /// 支付进出类型，记账用
        /// </summary>
        public enum enPayInOutType
        {
            /// <summary>
            /// 收入
            /// </summary>
            In = 1,
            /// <summary>
            /// 支出
            /// </summary>
            Out = 0,
        }

        /// <summary>
        /// 获取途径：0充值，1转账，2奖励，3红包，4提现，5返现
        /// </summary>
        public enum enPayFrom
        {
            /// <summary>
            /// 充值
            /// </summary>
            Recharge = 0,
            /// <summary>
            /// 转账
            /// </summary>
            Exchange = 1,
            /// <summary>
            /// 奖励
            /// </summary>
            Reward = 2,
            /// <summary>
            /// 红包
            /// </summary>
            RedPaged = 3,
            /// <summary>
            /// 提现
            /// </summary>
            Transfor = 4,
            /// <summary>
            /// 返现
            /// </summary>
            Cashback = 5,
            /// <summary>
            /// 线下支付
            /// </summary>
            OutLinePay = 6,
            /// <summary>
            /// 线上支付（商城消费等）
            /// </summary>
            OnLinePay = 7,
            /// <summary>
            /// 收入
            /// </summary>
            ShouRu = 8,
            /// <summary>
            /// 开户
            /// </summary>
            KaiHu = 9,
            /// <summary>
            /// 商家店铺收入
            /// </summary>
            Shop = 10,
            /// <summary>
            /// 支付
            /// </summary>
            pay = 11,
            /// <summary>
            /// 店铺代理收入
            /// </summary>
            ShopAgent = 12,
            /// <summary>
            /// 下级商家卖出商品
            /// </summary>
            NextShop = 13,
            /// <summary>
            /// 小区奖励
            /// </summary>
            CommunityRewards = 14,
            /// <summary>
            /// 升级会员
            /// </summary>
            UpdateUserLV = 15,
        }

        /// <summary>
        /// 币种/积分类型
        /// </summary>
        public enum enPayType
        {
            /// <summary>
            /// 余额/金币
            /// </summary>
            Coin = 1,
            /// <summary>
            /// 积分
            /// </summary>
            Point = 2,
            /// <summary>
            /// 充值卡积分
            /// </summary>
            KaPoint = 3,
            /// <summary>
            /// 都可以
            /// </summary>
            All = 0,
        }

        /// <summary>
        /// 订单状态
        /// </summary>
        public enum enOrderStatus
        {
            /// <summary>
            /// 取消
            /// </summary>
            Cancel = -1,
            /// <summary>
            /// 下单
            /// </summary>
            Order = 0,
            /// <summary>
            /// 已支付
            /// </summary>
            Payed = 1,
            /// <summary>
            /// 已发货
            /// </summary>
            Sended = 2,
            /// <summary>
            /// 已收货
            /// </summary>
            Recieved = 3,
            /// <summary>
            /// 已完成订单
            /// </summary>
            Completed = 4,
            /// <summary>
            /// 退货中
            /// </summary>
            Return = 5,
        }
        /// <summary>
        /// 排序方式
        /// </summary>
        public enum enOrderBy
        {
            /// <summary>
            /// 默认字段orderid
            /// </summary>
            OrderID = 0,

            TimeASC = 1,
            TimeDESC = 2,
            SalesASC = 3,
            SalesDESC = 4,
            PriceASC = 5,
            PriceDESC = 6,
        }
        /// <summary>
        /// 订单类型
        /// </summary>
        public enum enOrderType
        {
            /// <summary>
            /// 线上买，线下收货
            /// </summary>
            OnLine = 1,
            /// <summary>
            /// 现场收货，线下购买
            /// </summary>
            Down = 2,
        }

        /// <summary>
        /// 商品类型
        /// </summary>
        public enum enProductType
        {
            /// <summary>
            /// 实体商品
            /// </summary>
            Real = 0,
            /// <summary>
            /// 虚拟商品
            /// </summary>
            Virtual = 1,
        }

        /// <summary>
        /// 支付方式
        /// </summary>
        public enum enPayment
        {
            /// <summary>
            /// 本系统余额？积分支付
            /// </summary>
            LOCAL = 0,
            /// <summary>
            /// 微信
            /// </summary>
            WX = 1,
            /// <summary>
            /// 阿里巴巴
            /// </summary>
            ALI = 2,
            /// <summary>
            /// 威富通
            /// </summary>
            WFT = 3,
        }

        /// <summary>
        /// 用户类型
        /// </summary>
        public enum enUserType
        {
            /// <summary>
            /// 会员
            /// </summary>
            User = 0,
            /// <summary>
            /// 商家
            /// </summary>
            Shop = 1,
        }

        /// <summary>
        /// 用户等级
        /// </summary>
        public enum enUserLV
        {
            /// <summary>
            /// 会员
            /// </summary>
            普通用户 = 0,
            /// <summary>
            /// 商家
            /// </summary>
            牙商 = 1,
            /// <summary>
            /// 股东
            /// </summary>
            股东 = 2,
            /// <summary>
            /// 行商
            /// </summary>
            行商 = 3,
        }

        /// <summary>
        /// 商家类型
        /// </summary>
        public enum enShopType
        {
            /// <summary>
            /// 自营
            /// </summary>
            Self = 0,
            /// <summary>
            /// 联盟
            /// </summary>
            Member = 1,
        }

        public enum enAdminPower
        {
            /// <summary>
            /// 超级管理员
            /// </summary>
            Super = 10,
            Normal = 1,
        }

        /// <summary>
        /// 日志类型
        /// </summary>
        public enum enLogType
        {
            /// <summary>
            /// 错误
            /// </summary>
            Error = -1,
            /// <summary>
            /// 普通
            /// </summary>
            None = 0,
            /// <summary>
            /// 登录
            /// </summary>
            Login = 1,
            /// <summary>
            /// 支付
            /// </summary>
            Pay = 2,
            /// <summary>
            /// 充值，转账
            /// </summary>
            Recharge = 3,
            /// <summary>
            /// 红包
            /// </summary>
            Redpackage = 4,
            /// <summary>
            /// 订单
            /// </summary>
            Order = 5,
            /// <summary>
            /// 资料
            /// </summary>
            Info = 6,
            /// <summary>
            /// 提现
            /// </summary>
            Transferout = 7,
            /// <summary>
            /// 短信
            /// </summary>
            SMS = 8,
            /// <summary>
            /// 邮件
            /// </summary>
            Email = 9,
            /// <summary>
            /// 用户操作
            /// </summary>
            User = 10,
            /// <summary>
            /// 接口错误
            /// </summary>
            Services = 11,
            Admin = 12,
        }

        /// <summary>
        /// 实名制验证状态
        /// </summary>
        public enum enRealCheckStatus
        {
            /// <summary>
            /// 未验证
            /// </summary>
            未验证 = 0,
            /// <summary>
            /// 审核中
            /// </summary>
            审核中 = 1,
            /// <summary>
            /// 已验证
            /// </summary>
            已验证 = 2,
            /// <summary>
            /// 不通过
            /// </summary>
            不通过 = 3,
        }

        /// <summary>
        /// 提现申请状态
        /// </summary>
        public enum enApplyStatus
        {
            Faild = -1,
            Normal = 0,
            Success = 1,
        }

        /// <summary>
        /// 验证码类型（手机、邮箱、图片、文字、线下扫码支付）
        /// </summary>
        public enum enCodeType
        {
            SMS = 1,
            Email = 2,
            Img = 3,
            Text = 4,
            Pay = 5,
        }

        /// <summary>
        /// 错误编码
        /// </summary>
        public enum ErrorCode
        {
            没有错误 = 0,

            数据库操作失败 = -89,

            账号不存在 = -99,
            密码不正确 = -100,
            姓名不正确 = -101,
            手机不正确 = -102,
            账户余额不足 = -103,
            账号尚未实名制 = -104,
            银行卡不存在 = -105,
            账号不可用 = -106,
            账号类型不正确 = -107,
            账户积分不足 = -108,
            密码格式不正确 = -109,
            账号已被注册 = -110,
            微信已被注册 = -111,
            密码尚未设置 = -112,
            账户卡积分不足 = -113,
            身份证已被注册 = -115,
            账户已经是牙商 = -116,

            验证码已过期 = -399,
            请先获取验证码 = -400,
            验证码不正确 = -401,
            验证码错误次数过多 = -402,
            验证码异常 = -403,

            商品不存在 = -999,
            商品不可购买 = -1000,
            商品数量不足 = -1001,
            订单异常 = -1002,
            订单不存在 = -1003,

            充值卡不存在 = -2001,
            充值卡已被使用 = -2002,
            充值卡已失效 = -2003,

            红包不存在 = -9999,
            红包未激活 = -9998,

            状态异常或已处理 = -99998,
            未知异常 = -999999,
        }
        public enum enRechType
        {
            /// <summary>
            /// 首购券
            /// </summary>
            SGQ = 0,
        }
        public enum enReportType
        {
            Register = 0,
            Login = 1,
            NewFriend = 2,

            ReCharge = 3,
            Exchange = 4,
            TransOut = 5,

            Order = 6,
            Payed = 7,

            BuyCard = 8,//购买轻客
            PayMoney = 9,//总消费金额
        }
        /// <summary>
        /// 代理类型
        /// </summary>
        public enum enShopAgentType
        {
            /// <summary>
            /// 市代理
            /// </summary>
            City = 0,
            /// <summary>
            /// 区县代理
            /// </summary>
            District = 1
        }
        public enum enPriceType
        {
            /// <summary>
            /// 回购券
            /// </summary>
            Score = 0,
            /// <summary>
            /// 首购券
            /// </summary>
            Balance = 1,
            /// <summary>
            /// 购物券
            /// </summary>
            ShoppingVoucher = 2,
            /// <summary>
            /// 人名币
            /// </summary>
            RMB = 3
        }
        #endregion
    }
}
