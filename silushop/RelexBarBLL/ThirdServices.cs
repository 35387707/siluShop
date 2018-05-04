using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarDLL;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Core;
using Aliyun.Acs.Dysmsapi.Model.V20170525;

namespace RelexBarBLL
{
    public class ThirdServices : Common
    {
        LogsBLL logBll = new LogsBLL();

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="recPhone"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool SendSms(string recPhone, string msg)
        {

            //System.Net.WebClient wc = new System.Net.WebClient();
            ////string result = wc.DownloadString(string.Format(SysConfigBLL.SMSUrl, SysConfigBLL.SMSUser, SysConfigBLL.SMSPsw, recPhone,
            ////    System.Web.HttpUtility.UrlEncode(msg)));
            //wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            //string data = @"appid={0}&to={1}&content={2}&signature={3}";
            //string result = Encoding.UTF8.GetString(wc.UploadData(SysConfigBLL.SMSUrl, Encoding.UTF8.GetBytes(string.Format(data
            //    , SysConfigBLL.SMSUser, recPhone, msg, SysConfigBLL.SMSPsw))));

            //if (result.ToLower().Contains("\"success\""))//成功
            //    result = "1";
            //else
            //{
            //    logBll.InsertLog("发送短信失败:" + result, enLogType.Error);
            //    result = "未知错误，请联系管理员";
            //}
            SysConfigBLL bll = new SysConfigBLL();
            //Top.Api.ITopClient client = new Top.Api.DefaultTopClient(bll.Get("SMSURL"), bll.Get("SMSUSER"), bll.Get("SMSPSW"));
            //Top.Api.Request.AlibabaAliqinFcSmsNumSendRequest req = new Top.Api.Request.AlibabaAliqinFcSmsNumSendRequest();
            //req.SmsType = "normal";
            //var q2 =bll.Get("SMSSignName");
            //if (!string.IsNullOrEmpty(q2))
            //    req.SmsFreeSignName = q2;
            //else
            //    req.SmsFreeSignName = "丝路联盟";
            //req.SmsParam = "{\"code\":\"" + msg + "\"}";
            //req.RecNum = recPhone;

            //var q = bll.Get("SMSTemplateID");
            //if (!string.IsNullOrEmpty(q))
            //    req.SmsTemplateCode = q;
            //else
            //    return false;
            //var rsp = client.Execute(req);
            //if (rsp.IsError)
            //{
            //    logBll.InsertLog("发送短信失败:(" + rsp.ErrCode + ")" + rsp.ErrMsg, enLogType.Error);
            //    return false;
            //}
            //else
            //{
            //    return true;
            //}

            String product = "Dysmsapi";//短信API产品名称
            String domain = bll.Get("SMSURL");//短信API产品域名
            String accessId = bll.Get("SMSUSER");
            String accessSecret = bll.Get("SMSPSW");
            String regionIdForPop = "cn-hangzhou";
            IClientProfile profile = DefaultProfile.GetProfile(regionIdForPop, accessId, accessSecret);
            DefaultProfile.AddEndpoint(regionIdForPop, regionIdForPop, product, domain);
            IAcsClient acsClient = new DefaultAcsClient(profile);
            SendSmsRequest request = new SendSmsRequest();
            try
            {
                request.PhoneNumbers = recPhone;
                request.SignName = bll.Get("SMSSignName");
                request.TemplateCode = bll.Get("SMSTemplateID");
                request.TemplateParam = "{\"code\":\"" + msg + "\"}";
                //请求失败这里会抛ClientException异常
                SendSmsResponse sendSmsResponse = acsClient.GetAcsResponse(request);
                logBll.InsertLog("接收手机：" + recPhone + "，发送短信结果:" + sendSmsResponse.Message, enLogType.SMS);
                return true;
            }
            catch (Exception ex)
            {
                logBll.InsertLog("发送短信失败:" + ex.ToString(), enLogType.Error);
                return false;
            }







        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="recEmail"></param>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool SendEmail(string recEmail, string subject, string content)
        {
            try
            {
                CommonClass.SendMail sm = new CommonClass.SendMail(SysConfigBLL.EmailUser, SysConfigBLL.EmailPsw,
                    SysConfigBLL.EmailUser, recEmail, SysConfigBLL.EmailServer);
                sm.Subject = subject;
                sm.Body = content;
                sm.Send();

                logBll.InsertLog(string.Format("收件{0}；主题：{1}；内容：{2}"), enLogType.Email);

                return true;
            }
            catch (Exception ex)
            {
                logBll.InsertLog("发送邮件出错：" + ex.ToString(), enLogType.Error);
                return false;
            }
        }

        /// <summary>
        /// 插入支付接口日志
        /// </summary>
        /// <param name="page"></param>
        /// <param name="Payment"></param>
        /// <param name="money"></param>
        /// <param name="ordernum"></param>
        /// <param name="remark">标注</param>
        /// <param name="openid">微信/支付等第三方平台用户id</param>
        /// <returns></returns>
        public OtherPayServiceLog PayOrders(string page, enPayment Payment, decimal money
            , string ordernum, string remark, Guid UID, Guid ToUid, enPayFrom PayFrom, string openid)
        {
            try
            {
                OtherPayServiceLog paylog = new OtherPayServiceLog();
                paylog.ID = Guid.NewGuid();
                paylog.Page = page;
                paylog.Payment = Payment.ToString();
                paylog.PayPrice = money;
                paylog.PayNumber = GetServiceNumer();
                paylog.OrderNumber = ordernum;
                paylog.ReqStr = "";
                paylog.Status = (int)enOrderStatus.Order;
                paylog.Remark = remark;
                paylog.UID = UID;
                paylog.TOID = ToUid;
                paylog.OrderType = (int)PayFrom;
                paylog.CreateTime = paylog.UpdateTime = DateTime.Now;

                using (RelexBarEntities DBContext = new RelexBarEntities())
                {
                    DBContext.OtherPayServiceLog.Add(paylog);
                    if (DBContext.SaveChanges() > 0)
                    {
                        return paylog;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                logBll.InsertLog("支付失败:" + ex.ToString(), enLogType.Error);
                return null;
            }
        }

        public OtherPayServiceLog GetDetails(Guid ID)
        {
            try
            {
                using (RelexBarEntities DBContext = new RelexBarEntities())
                {
                    var paylog = DBContext.OtherPayServiceLog.FirstOrDefault(m => m.ID == ID);

                    return paylog;
                }
            }
            catch
            {
                return null;
            }
        }

        public OtherPayServiceLog GetDetails(string paynumber)
        {
            try
            {
                using (RelexBarEntities DBContext = new RelexBarEntities())
                {
                    var paylog = DBContext.OtherPayServiceLog.FirstOrDefault(m => m.PayNumber == paynumber);

                    return paylog;
                }
            }
            catch
            {
                return null;
            }
        }

        public bool CompletedPayServiceLog(Guid? UID, Guid ID)
        {
            try
            {
                using (RelexBarEntities DBContext = new RelexBarEntities())
                {
                    var paylog = DBContext.OtherPayServiceLog.FirstOrDefault(m => m.UID == UID && m.ID == ID);
                    if (paylog != null)
                    {
                        if (paylog.Status == (int)enOrderStatus.Order)
                        {
                            if (paylog.OrderType == (int)enPayFrom.Recharge)//直接充值
                            {
                                RechargeBLL recharBll = new RechargeBLL();
                                if (recharBll.InsertBalance(paylog.UID.Value, paylog.PayPrice) <= 0)//充值结果
                                {
                                    return false;
                                }
                            }
                            if (paylog.OrderType == (int)enPayFrom.UpdateUserLV)//升级会员
                            {
                                OrdersBLL obll = new OrdersBLL();
                                int i = obll.PayYaShangs(paylog.TOID.Value, paylog.UID.Value, paylog.Payment);
                                if (i <= 0)//充值结果
                                {
                                    return false;
                                }
                            }
                            if (paylog.OrderType == (int)enPayFrom.OutLinePay)//如果是线下支付金额
                            {
                                Common.enPayment payment;
                                if (!Enum.TryParse(paylog.Payment, out payment))
                                {
                                    payment = enPayment.WFT;
                                }
                                PayListBLL paybll = new PayListBLL();
                                if (paybll.PayForOutline(paylog.UID.Value, paylog.TOID, paylog.ID, payment, paylog.PayPrice) <= 0)//充值结果
                                {
                                    return false;
                                }
                            }
                            else//购买产品
                            {
                                OrdersBLL orderbll = new OrdersBLL();
                                var order = orderbll.GetDetail(paylog.OrderNumber);
                                if (order != null && order.Status == (int)enOrderStatus.Order)//订单还是下单状态，未支付成功
                                {
                                    if (orderbll.UpdateStatus(order.ID, enOrderStatus.Payed) <= 0)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        paylog.Status = (int)enOrderStatus.Payed;
                        paylog.UpdateTime = DateTime.Now;

                        DBContext.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                logBll.InsertLog("完成订单失败：" + ex, enLogType.Services);
                return false;
            }
        }
    }
}
