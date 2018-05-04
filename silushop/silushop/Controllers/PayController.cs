using Aop.Api.Domain;
using Aop.Api.Request;
using Aop.Api.Response;
using Aop.Api.Util;
using RelexBarBLL;
using RelexBarDLL;
using silushop.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static silushop.Utils.WX_Services;

namespace silushop.Controllers
{
    [Filter.AutoLogin]
    [Filter.CheckLogin]
    public class PayController : BaseController
    {
        LogsBLL logbll = new LogsBLL();
        // GET: Pay
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult PaySuccess()
        {
            if (Request.Browser.IsMobileDevice)
            {
                return Redirect("/Mobile/OrderList/1");
            }
            else
            {
                return Redirect("/Account/personalCenter");
            }
        }
        /// <summary>
        /// 本地购物券支付
        /// </summary>
        /// <param name="id">订单号</param>
        /// <returns></returns>
        public JsonResult LocalPay(Guid id)
        {
            OrdersBLL bll = new OrdersBLL();
            OrderList order = bll.GetDetail(id, UserInfo.ID);

            if (order == null)
            {
                return RJson(-1, "订单不存在");
            }
            if (order.Status != 0)
            {
                return RJson(-2, "订单状态不正确");
            }

            string url = SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "DOMAIN").Value;
            try
            {
                decimal? localPrice = bll.LocalPay(UserInfo.ID, order.ID);
                if (localPrice == null)
                {
                    return RJson(-1, "订单提交失败");
                }
                return RJson(1, "");
            }
            catch (Exception ex)
            {
                return RJson(-1, "支付失败:" + ex.Message);
            }

        }
        /*
        public JsonResult LocalPay(Guid id) {
            OrdersBLL bll = new OrdersBLL();
            Users user = Session["user"] as Users;
            try
            {
                int i = bll.LocalPay(user.ID, id);
                if (i > 0)
                {
                    return RJson(1, "支付成功");
                }
                else
                {
                    return RJson(-1, "支付失败");
                }
            }
            catch (Exception ex)
            {
                return RJson(-1,"支付失败:"+ex.Message);
            }
            
        }
        */

        #region 支付宝支付
        //阿里支付调用
        public JsonResult ALiPay(Guid id, int t)
        {
            OrdersBLL bll = new OrdersBLL();
            OrderList order = bll.GetDetail(id, UserInfo.ID);

            if (order == null)
            {
                return RJson(-1, "订单不存在");
            }
            if (order.Status != 0)
            {
                return RJson(-2, "订单状态不正确");
            }
            string url = SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "DOMAIN").Value;
            try
            {
                string ChaoPay = SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "ChaoPay").Value;
                string ChaoPayKey = SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "ChaoPayKey").Value;
                //order.Price = 0.01M;//测试用
                SortedDictionary<string, object> data = new SortedDictionary<string, object>();
                data.Add("type", t);
                data.Add("uid", ChaoPayKey);
                data.Add("fee", order.Price);
                data.Add("order_no", order.Number);
                data.Add("url", url + "/Pay/OtherNotifyCallBack");
                data.Add("returnurl", (url+ "/Shop/PaymentPattern?id=" + id).ToLower());

                string reqStr = GetChaoPayStr(data);
                return RJson(1, ChaoPay + reqStr);
                //return RJson(1, ChaoPay + CommonClass.EncryptDecrypt.AESEncrypt( string.Format(reqStr, order.UID, order.Price, order.Number, url + "/Pay/ALiNotifyCallBack"), ChaoPayKey));
            }
            catch (Exception ex)
            {
                return RJson(-3, ex.Message);
            }
            //try
            //{
            //    AlipayTradeAppPayRequest request = new AlipayTradeAppPayRequest();
            //    AlipayTradePagePayModel model = new AlipayTradePagePayModel();
            //    model.Subject = "丝路商城订单支付";
            //    //model.TotalAmount = "0.01";
            //    model.TotalAmount = order.Price.ToString();
            //    model.ProductCode = "FAST_INSTANT_TRADE_PAY";
            //    model.OutTradeNo = order.Number;
            //    model.TimeoutExpress = "30m";
            //    logbll.InsertLog("支付宝下单，OID：" + order.Number, Common.enLogType.Pay, "/Pay/ALiPay", "");
            //    if (Request.Browser.IsMobileDevice)//移动端
            //    {
            //        AlipayTradeWapPayRequest req = new AlipayTradeWapPayRequest();
            //        req.SetBizModel(model);
            //        req.SetReturnUrl(url + "/Mobile/OrderList/1");
            //        req.SetNotifyUrl(url + "/Pay/ALiNotifyCallBack");
            //        AlipayTradeWapPayResponse res = Utils.AlipayConfig.client.SdkExecute(req);
            //        return RJson(1, Utils.AlipayConfig.gatewayUrl + "?" + res.Body);
            //    }
            //    else
            //    {
            //        AlipayTradePagePayRequest req = new AlipayTradePagePayRequest();//pc端
            //        req.SetBizModel(model);
            //        req.SetReturnUrl(url + "/Account/personalCenter");
            //        req.SetNotifyUrl(url + "/Pay/ALiNotifyCallBack");
            //        AlipayTradePagePayResponse res = Utils.AlipayConfig.client.SdkExecute(req);
            //        return RJson(1, Utils.AlipayConfig.gatewayUrl + "?" + res.Body);
            //    }
            //}
            //catch (Exception e)
            //{

            //    return RJson(-3, e.Message);
            //}
        }

        private string GetChaoPayStr(SortedDictionary<string, object> data)
        {
            string ChaoPaySec = SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "ChaoPaySec").Value;
            string result = "";

            foreach (var a in data)
            {
                //result += a.Key + "=" + Server.UrlEncode(a.Value.ToString()) + "&";
                result += a.Key + "=" + a.Value.ToString() + "&";
            }
            result = result.Trim('&');
            result += "&sign=" + Common.MD5(result+"&key=", ChaoPaySec);
            return result;
        }

        //第三方（超）支付异步通知
        [Filter.NoFilter]
        public void OtherNotifyCallBack()
        {
            try
            {
                Dictionary<string, string> data = GetRequestPost();
                string status = data["status"];
                string Number = data["order_no"];//商户网站唯一订单号
                bool flag = true;//验证结果是否正确
                string sslog = "";
                foreach (string a in data.Keys)
                {
                    sslog += a + "=" + data[a] + ";";
                }
                logbll.InsertLog(sslog, Common.enLogType.Pay, "/Pay/OtherNotifyCallBack：" + flag + ",Number:" + Number, "");

                if (flag && !string.IsNullOrEmpty(status) && status == "1")
                {
                    string fee = data["fee"];//该笔订单的资金总额，单位为RMB-Yuan。取值范围为[0.01，100000000.00]，精确到小数点后两位。
                    OrdersBLL bll = new OrdersBLL();
                    int orderstatus = bll.GetOrderStatus(Number);
                    if (orderstatus == (int)Common.enOrderStatus.Order)
                    {
                        decimal price = 0;
                        decimal.TryParse(fee, out price);
                        int i = bll.PaySuccess(Number, Number, price);
                        if (i > 0)
                        {
                            Response.Write("success");
                        }
                        else
                        {
                            Response.Write("fail");
                        }
                    }
                    else
                    {
                        Response.Write("success");
                    }
                }
            }
            catch (Exception ex)
            {
                logbll.InsertLog(ex, Common.enLogType.Services);
                Response.Write("fail");
            }
        }

        //手工通知（补单）
        [Filter.NoFilter]
        [HttpPost]
        public void SGPayCallBack(int type, string num)
        {
            string Number = num;//商户网站唯一订单号
            string sslog = "手工操作：" + num;
            logbll.InsertLog(sslog, Common.enLogType.Pay, "/Pay/SGPayCallBack", "");

            if (!string.IsNullOrEmpty(Number))
            {
                if (type == 1)
                {
                    OrdersBLL bll = new OrdersBLL();
                    var orderModel = bll.GetByNumber(Number);
                    int orderstatus = orderModel.Status.Value;
                    if (orderstatus == (int)Common.enOrderStatus.Order)
                    {
                        int i = bll.PaySuccess(Number, Number, orderModel.Price);
                        if (i > 0)
                        {
                            Response.Write("success");
                        }
                        else
                        {
                            Response.Write("fail");
                        }
                    }
                    else
                    {
                        Response.Write("success");
                    }
                }
                else
                {
                    ThirdServices ts = new ThirdServices();
                    var model = ts.GetDetails(Number);
                    if (model == null)
                    {
                        Response.Write("fail");
                        return;
                    }
                    if (ts.CompletedPayServiceLog(model.UID, model.ID))
                    {
                        Response.Write("success");
                        return;
                    }
                    Response.Write("fail");
                    return;
                }
            }
        }

        //手工通知（补单）
        [Filter.NoFilter]
        public ActionResult SGPayCallBack()
        {
            return View();
        }

        //阿里支付异步通知
        [Filter.NoFilter]
        public void ALiNotifyCallBack()
        {
            Dictionary<string, string> data = GetRequestPost();

#if !DEBUG

            bool flag = AlipaySignature.RSACheckV1(data, AlipayConfig.alipay_key, AlipayConfig.charset, "RSA2", false);
            string content = AlipaySignature.GetSignContent(data);
            logbll.InsertLog(content, Common.enLogType.Pay, "/Pay/ALiNotifyCallBack支付结果：" + flag + "OID:" + data["out_trade_no"], "");

            if (flag)
            {
                string Number = data["out_trade_no"];//商户网站唯一订单号
                string trade_no = data["trade_no"];//该交易在支付宝系统中的交易流水号。最长64位。
                string total_amount = data["total_amount"];//该笔订单的资金总额，单位为RMB-Yuan。取值范围为[0.01，100000000.00]，精确到小数点后两位。
                OrdersBLL bll = new OrdersBLL();
                int orderstatus = bll.GetOrderStatus(Number);
                if (orderstatus == (int)Common.enOrderStatus.Order)
                {
                    decimal price = 0;
                    decimal.TryParse(total_amount, out price);
                    int i = bll.PaySuccess(Number, trade_no, price);
                    if (i > 0)
                    {
                        Response.Write("success");
                    }
                    else
                    {
                        Response.Write("fail");
                    }
                }
                else
                {
                    Response.Write("success");
                }
            }
#endif

#if DEBUG
            string Number = data["out_trade_no"];//商户网站唯一订单号
            string trade_no = data["trade_no"];//该交易在支付宝系统中的交易流水号。最长64位。
            string total_amount = data["total_amount"];//该笔订单的资金总额，单位为RMB-Yuan。取值范围为[0.01，100000000.00]，精确到小数点后两位。

            OrdersBLL bll = new OrdersBLL();
            int orderstatus = bll.GetOrderStatus(Number);
            if (orderstatus == (int)Common.enOrderStatus.Order)
            {
                decimal price = 0;
                decimal.TryParse(total_amount, out price);
                int i = bll.PaySuccess(Number, trade_no, price);
                if (i > 0)
                {
                    Response.Write("success");
                }
                else
                {
                    Response.Write("fail");
                }
            }
            else
            {
                Response.Write("success");
            }
#endif
        }
        /// 获取支付宝POST过来通知消息，并以“参数名=参数值”的形式组成数组 
        /// request回来的信息组成的数组
        [NonAction]
        public Dictionary<string, string> GetRequestPost()
        {
            int i = 0;
            Dictionary<string, string> sArray = new Dictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.Form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.Form[requestItem[i]]);
            }

            return sArray;
        }

        #endregion

        #region 微信支付
        //微信公众号支付
        public JsonResult WXGZHPay(Guid id)
        {

            string url = SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "SYSTEM_URL").Value;
            OrdersBLL bll = new OrdersBLL();
            OrderList order = bll.GetDetail(id, UserInfo.ID);

            if (order == null)
            {
                return RJson(-1, "订单不存在");
            }
            if (order.Status != 0)
            {
                return RJson(-2, "订单状态不正确");
            }
            //ThirdServices service = new ThirdServices();
            //var log = service.PayOrders("Pay/WXGZHPay", Common.enPayment.WX,order.Price, order.PayNumber
            //    , "",order.UID.Value,Guid.Empty, Common.enPayFrom.OnLinePay, "");//微信支付
            //if (log != null)//插入成功
            //{
            var wxAPI = new WX_Services.WxPayApi();
            int price = Convert.ToInt32(order.Price * 100);
            var paydata = new WX_Services.WxPayApi().GetUnifiedOrderResult(price, "丝路商城", UserInfo.WX_OpenID, order.Number, url + "/Pay/WXNotifyCallBack");
            string WS_prepay_id = paydata.GetValue("prepay_id").ToString();
            return RJson(1, wxAPI.GetPrePayDataByJS(WS_prepay_id));
            //}
            // else
            // {
            // return RJson(-1,"生成订单失败，请重新尝试");
            // }
        }

        public void WXNotifyCallBack()
        {
            WX_Services.WxPayData notifyData = GetNotifyData();

            //检查支付结果中transaction_id是否存在
            if (!notifyData.IsSet("transaction_id"))
            {
                //若transaction_id不存在，则立即返回结果给微信支付后台
                WX_Services.WxPayData res = new WX_Services.WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付结果中微信订单号不存在");
                Response.Write(res.ToXml());
                Response.End();
                logbll.InsertLog("微信支付失败：transaction_id不存在", Common.enLogType.Pay, "/Pay/WXNotifyCallBack", Request.UserHostAddress);
                return;
            }
            string transaction_id = notifyData.GetValue("transaction_id").ToString();//微信支付订单号

            //查询订单，判断订单真实性
            if (!QueryOrder(transaction_id))
            {
                //若订单查询失败，则立即返回结果给微信支付后台
                WX_Services.WxPayData res = new WX_Services.WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单查询失败");
                Response.Write(res.ToXml());
                Response.End();
                return;
            }
            //查询订单成功
            else
            {
                //
                string out_trade_no = notifyData.GetValue("out_trade_no").ToString();
                OrdersBLL tbll = new OrdersBLL();
                RelexBarDLL.OrderList opf = tbll.GetByNumber(out_trade_no);
                if (opf.Status == 1)
                {
                    WX_Services.WxPayData res1 = new WX_Services.WxPayData();
                    res1.SetValue("return_code", "SUCCESS");
                    res1.SetValue("return_msg", "订单已支付");
                    Response.Write(res1.ToXml());
                    Response.End();
                    return;
                }
                if (opf == null)
                {
                    WX_Services.WxPayData res1 = new WX_Services.WxPayData();
                    res1.SetValue("return_code", "FAIL");
                    res1.SetValue("return_msg", "商品不存在");
                    Response.Write(res1.ToXml());
                    Response.End();
                    return;
                }
                else
                {
                    decimal price = Convert.ToDecimal(notifyData.GetValue("total_fee")) / 100;



                    int i = tbll.PaySuccess(out_trade_no, transaction_id, price);
                    if (i > 0)
                    {
                        WX_Services.WxPayData res = new WX_Services.WxPayData();
                        res.SetValue("return_code", "SUCCESS");
                        res.SetValue("return_msg", "OK");
                        Response.Write(res.ToXml());
                        Response.End();
                        return;
                    }
                    else
                    {
                        WX_Services.WxPayData res = new WX_Services.WxPayData();
                        res.SetValue("return_code", "FAIL");
                        res.SetValue("return_msg", "支付失败");
                        Response.Write(res.ToXml());
                        Response.End();
                        return;
                    }
                }



            }
        }
        //查询订单
        private bool QueryOrder(string transaction_id)
        {
            WX_Services.WxPayData req = new WX_Services.WxPayData();
            req.SetValue("transaction_id", transaction_id);
            WX_Services.WxPayData res = WxPayApi.OrderQuery(req);
            if (res.GetValue("return_code").ToString() == "SUCCESS" &&
                res.GetValue("result_code").ToString() == "SUCCESS")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 接收从微信支付后台发送过来的数据并验证签名
        /// </summary>
        /// <returns>微信支付后台返回的数据</returns>
        public WX_Services.WxPayData GetNotifyData()
        {
            //接收从微信后台POST过来的数据
            System.IO.Stream s = Request.InputStream;
            int count = 0;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            s.Flush();
            s.Close();
            s.Dispose();
            //转换数据格式并验证签名
            WX_Services.WxPayData data = new WX_Services.WxPayData();

            logbll.InsertLog("微信支付异步通知结果:" + builder.ToString(), Common.enLogType.Pay, "/Pay/WXNotifyCallBack", Request.UserHostAddress);
            try
            {
                data.FromXml(builder.ToString());
            }
            catch (Exception ex)
            {
                logbll.InsertLog("微信支付异常:" + ex.Message + ";微信支付结果：" + builder.ToString(), Common.enLogType.Pay, "WXCallBack.ashx", Request.UserHostAddress);
                //若签名错误，则立即返回结果给微信支付后台
                WX_Services.WxPayData res = new WX_Services.WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
                Response.Write(res.ToXml());
                Response.End();
            }
            return data;
        }
        #endregion


        #region 支付购买成牙商，无需生成订单
        //阿里支付调用，无订单
        public JsonResult YaSh_ALiPay(Guid? pid, int t)
        {
            if (pid == null)
            {
                return RJson(-1, "商品不存在");
            }
            ProductsBLL bll = new ProductsBLL();
            var product = bll.GetProduct(pid.Value);
            if (product == null)
            {
                return RJson(-1, "商品不存在");
            }
            if (product.Status != (int)Common.enProductType.Virtual)
            {
                return RJson(-2, "商品不是牙商商品");
            }

            try
            {
                string url = SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "DOMAIN").Value;
                try
                {
                    string ChaoPay = SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "ChaoPay").Value;
                    string ChaoPayKey = SysConfigBLL.SysConfigList.FirstOrDefault(m => m.Name == "ChaoPayKey").Value;
                    //string reqStr = "type=1&fee={1}&order_no={2}&url={3}&subject=丝路股份商城&returnurl=" + Request.Url.ToString().ToLower();

                    ThirdServices ts = new ThirdServices();
                    var otherModel = ts.PayOrders("YaSh_ALiPay", Common.enPayment.ALI, product.Price
                             , "", "升级牙商", UserInfo.ID, product.ID
                             , Common.enPayFrom.UpdateUserLV, "");

                    SortedDictionary<string, object> data = new SortedDictionary<string, object>();
                    data.Add("type", t);
                    data.Add("uid", ChaoPayKey);
                    data.Add("fee", otherModel.PayPrice);
                    data.Add("order_no", otherModel.PayNumber);
                    data.Add("url", url + "/Pay/OtherCallBack_NoOrder");
                    data.Add("returnurl", (url + "/Home/PaymentPattern?pid=" + pid).ToLower());
                    string reqStr = GetChaoPayStr(data);

                    return RJson(1, ChaoPay + reqStr);
                }
                catch (Exception ex)
                {
                    return RJson(-3, ex.Message);
                }
            }
            catch (Exception ex)
            {
                return RJson(-1, "支付失败:" + ex.Message);
            }
        }

        //第三方（超）支付异步通知
        [Filter.NoFilter]
        public void OtherCallBack_NoOrder()
        {
            Dictionary<string, string> data = GetRequestPost();
            string status = data["status"];
            string Number = data["order_no"];//商品编号
            bool flag = true;//验证结果是否正确
            string sslog = "";
            foreach (string a in data.Keys)
            {
                sslog += a + "=" + data[a] + ";";
            }
            logbll.InsertLog(sslog, Common.enLogType.Pay, "/Pay/OtherCallBack_NoOrder：" + flag + ",Number:" + Number, "");

            if (flag && !string.IsNullOrEmpty(status) && status == "1")
            {
                ThirdServices ts = new ThirdServices();
                var model = ts.GetDetails(Number);
                if (model == null)
                {
                    Response.Write("fail");
                    return;
                }
                if (ts.CompletedPayServiceLog(model.UID, model.ID))
                {
                    Response.Write("success");
                    return;
                }
                Response.Write("fail");
                return;
            }
        }

        #endregion

        #region 首购卷支付
        /// <summary>
        /// 首购卷支付，仅用于升级牙商使用
        /// </summary>
        /// <param name="id">订单号</param>
        /// <returns></returns>
        public JsonResult SGJPay(Guid? pid)
        {
            if (pid == null)
            {
                return RJson(-1, "商品不存在");
            }
            ProductsBLL bll = new ProductsBLL();
            var product = bll.GetProduct(pid.Value);
            if (product == null)
            {
                return RJson(-1, "商品不存在");
            }
            if (product.Status != (int)Common.enProductType.Virtual)
            {
                return RJson(-2, "商品不是牙商商品");
            }

            try
            {
                OrdersBLL obll = new OrdersBLL();
                int i = obll.PayYaShangs(pid.Value, UserInfo.ID, Common.enPayment.LOCAL.ToString());
                if (i <= 0)
                {
                    return RJson(-1, ((Common.ErrorCode)i).ToString());
                }
                return RJson(1, "");
            }
            catch (Exception ex)
            {
                return RJson(-1, "支付失败:" + ex.Message);
            }
        }

        #endregion
    }
}