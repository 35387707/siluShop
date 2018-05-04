using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;

namespace RelexBarBLL.Services
{
    public class WX_Services
    {
        public enum euScope
        {
            /// <summary>
            /// 不弹出授权页面，直接跳转，只能获取用户openid
            /// </summary>
            snsapi_base = 1,
            /// <summary>
            /// 弹出授权页面，可通过openid拿到昵称、性别、所在地
            /// </summary>
            snsapi_userinfo = 2,
        }

        public enum euEncryptType
        {
            /// <summary>
            /// MD5加密
            /// </summary>
            MD5 = 1,
            /// <summary>
            /// SHA1加密
            /// </summary>
            SHA1 = 2,
        }

        public class WX_UserInfo
        {
            public string openid { get; set; }
            public string nickname { get; set; }
            public string sex { get; set; }
            public string province { get; set; }
            public string city { get; set; }
            public string country { get; set; }
            public string headimgurl { get; set; }
        }

        public class WxPayData
        {
            public WxPayData()
            {

            }

            public WxPayData(euEncryptType encrypt)
            {
                _encrypt = encrypt;
            }

            public euEncryptType _encrypt = euEncryptType.SHA1;

            /// <summary>
            /// 公众号id
            /// </summary>
            public static string wxAppid = string.Empty;
            /// <summary>
            /// 商户号
            /// </summary>
            public static string wxMCHID = string.Empty;
            /// <summary>
            /// 公众号秘钥
            /// </summary>
            public static string wxKey = string.Empty;

            //采用排序的Dictionary的好处是方便对数据包进行签名，不用再签名之前再做一次排序
            private System.Collections.Generic.SortedDictionary<string, object> m_values =
                new System.Collections.Generic.SortedDictionary<string, object>();

            public static DateTime timeoutSession = DateTime.Now;

            public static string GetJsApiTicket()
            {
                string tikect = string.Empty;
                if (HttpContext.Current.Session["tikect"] == null || timeoutSession.AddSeconds(6000) < DateTime.Now)
                {
                    string token = string.Empty;
                    string html_token = HttpService.Get("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + wxAppid + "&secret=" + wxKey);
                    if (!html_token.Contains("errcode"))
                    {
                        Match mc = Regex.Match(html_token, "[\"]access_token[\"]\\s*:\\s*[\"]([^\"]+)[\"]");
                        if (mc.Success)
                        {
                            HttpContext.Current.Session["token"] = mc.Groups[1].Value;
                        }
                        else
                        { HttpContext.Current.Session["token"] = ""; }
                    }
                    token = HttpContext.Current.Session["token"].ToString();
                    //writelogs("token=" + token);

                    string html_tiket = HttpService.Get("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token=" + token + "&type=jsapi");
                    Match mc2 = Regex.Match(html_tiket, "[\"]ticket[\"]\\s*:\\s*[\"]([^\"]+)[\"]");
                    if (mc2.Success)
                    {
                        HttpContext.Current.Session["tikect"] = mc2.Groups[1].Value;
                    }
                    timeoutSession = DateTime.Now;
                }
                tikect = HttpContext.Current.Session["tikect"].ToString();
                //writelogs("tikect=" + tikect);
                return tikect;
            }

            /// <summary>
            /// 获取微信登录的授权url
            /// </summary>
            /// <returns></returns>
            public string GetWXLoginURL(string redirect_uri, euScope scope, string param)
            {
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope={2}&state={3}#wechat_redirect";
                return string.Format(url, wxAppid, HttpContext.Current.Server.UrlEncode(redirect_uri), scope, param);
                //如果用户同意授权，页面将跳转至 redirect_uri/?code=CODE&state=STATE。
                //若用户禁止授权，则重定向后不会带上code参数，仅会带上state参数redirect_uri?state=STATE 
            }
            /// <summary>
            /// 获取微信用户信息
            /// </summary>
            /// <returns></returns>
            public WX_UserInfo GetWXUserinfo()
            {
                string code = HttpContext.Current.Request.QueryString["code"];
                WX_UserInfo user = null;
                try
                {
                    string url = "https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code";
                    string result = HttpService.Get(string.Format(url, wxAppid, wxKey, code));
                    var dic = CommonClass.ChangeData.ExchangeDataType(result);
                    if (dic != null && dic.Keys.Contains("openid"))
                    {
                        user = new WX_UserInfo();
                        user.openid = dic["openid"].ToString();

                        if (string.Compare(dic["scope"].ToString(), euScope.snsapi_userinfo.ToString(), true) == 1)
                        {
                            url = "https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}&lang=zh_CN";
                            result = HttpService.Get(string.Format(url, dic["access_token"], user.openid, code));
                            dic = CommonClass.ChangeData.ExchangeDataType(result);
                            if (dic != null && dic.Keys.Contains("openid"))
                            {
                                user.openid = dic["openid"].ToString();
                                user.nickname = dic["nickname"].ToString();
                                user.sex = dic["sex"].ToString();
                                user.province = dic["province"].ToString();
                                user.city = dic["city"].ToString();
                                user.country = dic["country"].ToString();
                                user.headimgurl = dic["headimgurl"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    new LogsBLL().InsertLog(ex.ToString(), Common.enLogType.Services);
                }
                return user;
            }

            /**
            * 设置某个字段的值
            * @param key 字段名
             * @param value 字段值
            */
            public void SetValue(string key, object value)
            {
                m_values[key] = value;
            }

            /**
            * 根据字段名获取某个字段的值
            * @param key 字段名
             * @return key对应的字段值
            */
            public object GetValue(string key)
            {
                object o = null;
                m_values.TryGetValue(key, out o);
                return o;
            }

            /**
             * 判断某个字段是否已设置
             * @param key 字段名
             * @return 若字段key已被设置，则返回true，否则返回false
             */
            public bool IsSet(string key)
            {
                object o = null;
                m_values.TryGetValue(key, out o);
                if (null != o)
                    return true;
                else
                    return false;
            }

            /**
            * @将Dictionary转成xml
            * @return 经转换得到的xml串
            * @throws WxPayException
            **/
            public string ToXml()
            {
                //数据为空时不能转化为xml格式
                if (0 == m_values.Count)
                {
                }

                string xml = "<xml>";
                foreach (System.Collections.Generic.KeyValuePair<string, object> pair in m_values)
                {
                    //字段值不能为null，会影响后续流程
                    if (pair.Value == null)
                    {
                    }

                    if (pair.Value.GetType() == typeof(int))
                    {
                        xml += "<" + pair.Key + ">" + pair.Value + "</" + pair.Key + ">";
                    }
                    else if (pair.Value.GetType() == typeof(string))
                    {
                        xml += "<" + pair.Key + ">" + "<![CDATA[" + pair.Value + "]]></" + pair.Key + ">";
                    }
                    else//除了string和int类型不能含有其他数据类型
                    {
                    }
                }
                xml += "</xml>";
                return xml;
            }

            /**
            * @将xml转为WxPayData对象并返回对象内部的数据
            * @param string 待转换的xml串
            * @return 经转换得到的Dictionary
            * @throws WxPayException
            */
            public System.Collections.Generic.SortedDictionary<string, object> FromXml(string xml)
            {
                if (string.IsNullOrEmpty(xml))
                {
                    return new SortedDictionary<string, object>();
                }

                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(xml);
                System.Xml.XmlNode xmlNode = xmlDoc.FirstChild;//获取到根节点<xml>
                System.Xml.XmlNodeList nodes = xmlNode.ChildNodes;
                foreach (System.Xml.XmlNode xn in nodes)
                {
                    System.Xml.XmlElement xe = (System.Xml.XmlElement)xn;
                    m_values[xe.Name] = xe.InnerText;//获取xml的键值对到WxPayData内部的数据中
                }

                try
                {
                    //2015-06-29 错误是没有签名
                    if ((m_values.ContainsKey("return_code") && m_values["return_code"].ToString() != "SUCCESS")
                        || (m_values.ContainsKey("result_code") && m_values["result_code"].ToString() == "0"))
                    {
                        return m_values;
                    }
                    CheckSign();//验证签名,不通过会抛异常
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return m_values;
            }

            /**
            * @Dictionary格式转化成url参数格式
            * @ return url格式串, 该串不包含sign字段值
            */
            public string ToUrl()
            {
                string buff = "";
                foreach (System.Collections.Generic.KeyValuePair<string, object> pair in m_values)
                {
                    if (pair.Value == null)
                    {
                    }

                    if (pair.Key != "sign" && pair.Value.ToString() != "")
                    {
                        buff += pair.Key + "=" + pair.Value + "&";
                    }
                }
                buff = buff.Trim('&');
                return buff;
            }

            /**
            * @values格式化成能在Web页面上显示的结果（因为web页面上不能直接输出xml格式的字符串）
            */
            public string ToPrintStr()
            {
                string str = "";
                foreach (System.Collections.Generic.KeyValuePair<string, object> pair in m_values)
                {
                    str += string.Format("{0}={1}<br>", pair.Key, pair.Value.ToString());
                }
                return str;
            }

            /**
            * @生成签名，详见签名生成算法
            * @return 签名, sign字段不参加签名
            */
            public string MakeSign()
            {
                return MakeSign(wxKey);
            }

            public string MakeSign(string key)
            {
                //转url格式
                string str = ToUrl();

                var sb = new System.Text.StringBuilder();
                if (_encrypt == euEncryptType.SHA1)
                {
                    //sha1计算
                    var sha1 = System.Security.Cryptography.SHA1.Create();
                    var bs = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
                    foreach (byte b in bs)
                    {
                        sb.Append(b.ToString("x2"));
                    }
                }
                else if (_encrypt == euEncryptType.MD5)
                {
                    //MD5加密
                    //在string后加入API KEY
                    str += "&key=" + key;
                    var md5 = System.Security.Cryptography.MD5.Create();
                    var bs = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
                    foreach (byte b in bs)
                    {
                        sb.Append(b.ToString("x2"));
                    }
                    //所有字符转为大写
                    return sb.ToString().ToUpper();
                }
                return sb.ToString();
            }

            public bool CheckSign()
            {
                return CheckSign(wxKey);
            }
            /**
            * 
            * 检测签名是否正确
            * 正确返回true，错误抛异常
            */
            public bool CheckSign(string key)
            {
                //如果没有设置签名，则跳过检测
                if (!IsSet("sign"))
                {
                    return false;
                }
                //如果设置了签名但是签名为空，则抛异常
                else if (GetValue("sign") == null || GetValue("sign").ToString() == "")
                {
                    return false;
                }

                //获取接收到的签名
                string return_sign = GetValue("sign").ToString();

                //在本地计算新的签名
                string cal_sign = MakeSign(key);

                if (cal_sign == return_sign)
                {
                    return true;
                }
                else
                    return false;
            }

            /**
            * @获取Dictionary
            */
            public System.Collections.Generic.SortedDictionary<string, object> GetValues()
            {
                return m_values;
            }

            /**
            * 生成时间戳，标准北京时间，时区为东八区，自1970年1月1日 0点0分0秒以来的秒数
             * @return 时间戳
            */
            public static string GenerateTimeStamp()
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return Convert.ToInt64(ts.TotalSeconds).ToString();
            }

            /**
            * 生成随机串，随机串包含字母或数字
            * @return 随机串
            */
            public static string GenerateNonceStr()
            {
                return Guid.NewGuid().ToString().Replace("-", "");
            }

            /**
            * 根据当前系统时间加随机序列来生成订单号
             * @return 订单号
            */
            public static string GenerateOutTradeNo()
            {
                var ran = new Random();
                return string.Format("{0}{1}{2}", wxMCHID, DateTime.Now.ToString("yyyyMMddHHmmss"), ran.Next(999));
            }

            public static string ToUrlParams(System.Collections.Generic.SortedDictionary<string, object> map)
            {
                string buff = "";
                foreach (System.Collections.Generic.KeyValuePair<string, object> pair in map)
                {
                    buff += pair.Key + "=" + pair.Value + "&";
                }
                buff = buff.Trim('&');
                return buff;
            }

            public static void writelogs(string msg)
            {
                new LogsBLL().InsertLog(msg, Common.enLogType.Services);
            }
        }

        /// <summary>
        /// http连接基础类，负责底层的http通信
        /// </summary>
        public class HttpService
        {
            //=======【证书路径设置】===================================== 
            /* 证书路径,注意应该填写绝对路径（仅退款、撤销订单时需要）
            */
            public const string SSLCERT_PATH = "cert/apiclient_cert.p12";
            public const string SSLCERT_PASSWORD = "1233410002";

            public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
            {
                //直接确认，否则打不开    
                return true;
            }

            public static string Post(string xml, string url, bool isUseCert, int timeout)
            {
                System.GC.Collect();//垃圾回收，回收没有正常关闭的http连接

                string result = "";//返回结果

                HttpWebRequest request = null;
                HttpWebResponse response = null;
                Stream reqStream = null;

                try
                {
                    //设置最大连接数
                    ServicePointManager.DefaultConnectionLimit = 200;
                    //设置https验证方式
                    if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                    {
                        ServicePointManager.ServerCertificateValidationCallback =
                                new RemoteCertificateValidationCallback(CheckValidationResult);
                    }

                    /***************************************************************
                    * 下面设置HttpWebRequest的相关属性
                    * ************************************************************/
                    request = (HttpWebRequest)WebRequest.Create(url);

                    request.Method = "POST";
                    request.Timeout = timeout * 1000;

                    //设置代理服务器
                    //WebProxy proxy = new WebProxy();                          //定义一个网关对象
                    //proxy.Address = new Uri(WxPayConfig.PROXY_URL);              //网关服务器端口:端口
                    //request.Proxy = proxy;

                    //设置POST的数据类型和长度
                    request.ContentType = "text/xml";
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                    request.ContentLength = data.Length;

                    //是否使用证书
                    if (isUseCert)
                    {
                        string path = HttpContext.Current.Request.PhysicalApplicationPath;
                        X509Certificate2 cert = new X509Certificate2(path + SSLCERT_PATH, SSLCERT_PASSWORD);
                        request.ClientCertificates.Add(cert);
                    }

                    //往服务器写入数据
                    reqStream = request.GetRequestStream();
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();

                    //获取服务端返回
                    response = (HttpWebResponse)request.GetResponse();

                    //获取服务端返回数据
                    StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    result = sr.ReadToEnd().Trim();
                    sr.Close();
                }
                catch (System.Threading.ThreadAbortException e)
                {
                    System.Threading.Thread.ResetAbort();
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                    }
                }
                catch (Exception e)
                {
                }
                finally
                {
                    //关闭连接和流
                    if (response != null)
                    {
                        response.Close();
                    }
                    if (request != null)
                    {
                        request.Abort();
                    }
                }
                return result;
            }

            /// <summary>
            /// 处理http GET请求，返回数据
            /// </summary>
            /// <param name="url">请求的url地址</param>
            /// <returns>http GET成功后返回的数据，失败抛WebException异常</returns>
            public static string Get(string url)
            {
                System.GC.Collect();
                string result = "";

                HttpWebRequest request = null;
                HttpWebResponse response = null;

                //请求url以获取数据
                try
                {
                    //设置最大连接数
                    ServicePointManager.DefaultConnectionLimit = 200;
                    //设置https验证方式
                    if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                    {
                        ServicePointManager.ServerCertificateValidationCallback =
                                new RemoteCertificateValidationCallback(CheckValidationResult);
                    }

                    /***************************************************************
                    * 下面设置HttpWebRequest的相关属性
                    * ************************************************************/
                    request = (HttpWebRequest)WebRequest.Create(url);

                    request.Method = "GET";

                    //设置代理
                    //WebProxy proxy = new WebProxy();
                    //proxy.Address = new Uri(WxPayConfig.PROXY_URL);
                    //request.Proxy = proxy;

                    //获取服务器返回
                    response = (HttpWebResponse)request.GetResponse();

                    //获取HTTP返回数据
                    StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    result = sr.ReadToEnd().Trim();
                    sr.Close();
                }
                catch (System.Threading.ThreadAbortException e)
                {
                    System.Threading.Thread.ResetAbort();
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    { }
                }
                catch (Exception e)
                {
                }
                finally
                {
                    //关闭连接和流
                    if (response != null)
                    {
                        response.Close();
                    }
                    if (request != null)
                    {
                        request.Abort();
                    }
                }
                return result;
            }
        }
    }
}
