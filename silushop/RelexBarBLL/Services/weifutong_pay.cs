using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RelexBarBLL.Services
{
    public class weifutong_pay
    {
        string url_pay = "https://pay.swiftpass.cn/pay/jspay?token_id={0}&showwxtitle=1";
        string url_gettokenid = "https://pay.swiftpass.cn/pay/gateway";

        /// <summary>
        /// 公众号id
        /// </summary>
        public static string APPID = string.Empty;
        /// <summary>
        /// 商户号
        /// </summary>
        public static string MCHID = string.Empty;
        /// <summary>
        /// 公众号秘钥
        /// </summary>
        public static string KEY = string.Empty;

        public string GetPayUrl(string ordernum, string body, decimal fee, string callbackurl, string useropenid, string notify_url)
        {
            try
            {
                WX_Services.WxPayData data = new WX_Services.WxPayData(WX_Services.euEncryptType.MD5);
                data.SetValue("out_trade_no", ordernum);//商户订单号
                data.SetValue("body", body);//商品描述

//#if DEBUG
                //data.SetValue("total_fee", 1);//总金额
//#endif
//#if !DEBUG
                data.SetValue("total_fee", (int)(fee * 100));//总金额
//#endif
                data.SetValue("mch_create_ip", HttpContext.Current.Request.UserHostAddress);//终端IP
                data.SetValue("service", "pay.weixin.jspay");//接口类型：pay.weixin.jspay
                data.SetValue("mch_id", MCHID);//必填项，商户号，由平台分配
                data.SetValue("notify_url", notify_url);
                //通知地址，必填项，接收平台通知的URL，需给绝对路径，255字符内;此URL要保证外网能访问   
                data.SetValue("nonce_str", WX_Services.WxPayData.GenerateNonceStr());//随机字符串，必填项，不长于 32 位
                data.SetValue("sign_type", "MD5");//签名方式
                data.SetValue("sub_openid", useropenid);//测试账号不传值,此处默认给空值。正式账号必须传openid值，获取openid值指导文档地址：http://www.cnblogs.com/txw1958/p/weixin76-user-info.html
                data.SetValue("callback_url", callbackurl);//前台地址  交易完成后跳转的 URL，需给绝对路径，255字 符 内 格 式如:http://wap.tenpay.com/callback.asp

                string sign = data.MakeSign(KEY);
                data.SetValue("sign", sign);//签名方式

                string toxml = data.ToXml();
                new LogsBLL().InsertLog(toxml, Common.enLogType.Services);
                string result = WX_Services.HttpService.Post(toxml, url_gettokenid, false, 60);
                WX_Services.WxPayData token = new WX_Services.WxPayData(WX_Services.euEncryptType.MD5);
                var r = token.FromXml(result);

                if (int.Parse(r["status"].ToString()) == 0)
                {
                    return string.Format(url_pay, r["token_id"].ToString());
                }
                else
                {
                    new LogsBLL().InsertLog(result, Common.enLogType.Services);
                    return "";
                }
            }
            catch (Exception ex)
            {
                new LogsBLL().InsertLog(ex, Common.enLogType.Services);
                return "";
            }
        }

        public string GetPayResultNumber(string content, out decimal fee)
        {
            fee = 0;
            try
            {
                new LogsBLL().InsertLog(content, Common.enLogType.Services);
                WX_Services.WxPayData data = new WX_Services.WxPayData(WX_Services.euEncryptType.MD5);
                var r = data.FromXml(content);
                string sign = data.MakeSign(KEY);
                if (data.CheckSign(KEY))
                {
                    if (int.Parse(r["status"].ToString()) == 0 && int.Parse(r["result_code"].ToString()) == 0)
                    {
                        fee = decimal.Parse(r["total_fee"].ToString());
                        return r["out_trade_no"].ToString();
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                new LogsBLL().InsertLog(ex, Common.enLogType.Services);
                return string.Empty;
            }
        }
    }
}
