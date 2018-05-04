using Aop.Api;
using RelexBarBLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace silushop.Utils
{
    /* *
  *类名：AlipayConfig
  *功能：基础配置类
  *详细：设置帐户有关信息及返回路径
  *修改日期：2017-04-05
  *说明：
  *以下代码只是为了方便商户测试而提供的样例代码，商户可以根据自己网站的需要，按照技术文档编写,并非一定要使用该代码。
  *该代码仅供学习和研究支付宝接口使用，只是提供一个参考。
  */
    public class AlipayConfig
    {
        //↓↓↓↓↓↓↓↓↓↓请在这里配置您的基本信息↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓

        // 应用ID,您的APPID，收款账号既是您的APPID对应支付宝账号
        public static String app_id = Convert.ToString(SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "ALI_APPID").Value);

        // 商户私钥，您的PKCS8格式RSA2私钥
        public static String merchant_private_key = Convert.ToString(SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "ALI_PRIVATE_RSA2").Value);

        // 支付宝公钥,查看地址：https://openhome.alipay.com/platform/keyManage.htm 对应APPID下的支付宝公钥。
        public static String alipay_public_key = Convert.ToString(SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "ALI_PUBLIC_RSA2").Value);
        public static String alipay_key = Convert.ToString(SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "ALI_RSA2").Value);
        //// 服务器异步通知页面路径  需http://格式的完整路径，不能加?id=123这类自定义参数，必须外网可以正常访问
        //public static String notify_url = "http://工程公网访问地址/alipay.trade.page.pay-JAVA-UTF-8/notify_url.jsp";

        //// 页面跳转同步通知页面路径 需http://格式的完整路径，不能加?id=123这类自定义参数，必须外网可以正常访问
        //public static String return_url = "http://工程公网访问地址/alipay.trade.page.pay-JAVA-UTF-8/return_url.jsp";

        // 签名方式
        public static String sign_type = "RSA2";

        // 字符编码格式
        public static String charset = "utf-8";

        // 支付宝网关
        public static String gatewayUrl = Convert.ToString(SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "ALI_APIURL").Value);
        //public static String gatewayUrl = "https://openapi.alipaydev.com/gateway.do";
        // 支付宝网关
        // public static String log_path = "C:\\";


        //↑↑↑↑↑↑↑↑↑↑请在这里配置您的基本信息↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
        public static IAopClient client = new DefaultAopClient(gatewayUrl, app_id
            , merchant_private_key, "json", "1.0", sign_type, alipay_public_key, charset, false);
    }
}