using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelexBarBLL;
using System.Net;
using System.Net.Sockets;

namespace siluExecute
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("程序开始");
            DateTime dt1 = System.DateTime.Now;
            HolidaysBLL hbll = new HolidaysBLL();
            LogsBLL logbll = new LogsBLL();
            string hostName = Dns.GetHostName();//本机名   
            System.Net.IPAddress[] addressList = Dns.GetHostAddresses(hostName);//会返回所有地址，包括IPv4和IPv6   
            string ip = string.Empty;
            int a = 0;
            for (int i = 0; i < addressList.Length; i++)
            {
                if (addressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    ip += (a > 0 ? "," : "") + addressList[i].ToString();
                    a++;
                }
            }
            if (ip.Length > 50)
            {
                ip = ip.Substring(0, 50);
            }
            //if (hbll.ToDayIsHolidays())//不需要判断节假日
            //{
            //    Console.WriteLine("节假日");
            //}
            //else
            {
                try
                {
                    UsersBLL bll = new UsersBLL();
                    bll.TimedTaskRun();
                    DateTime dt2 = DateTime.Now;
                    TimeSpan ts = dt2.Subtract(dt1);
                    logbll.InsertLog("收益定时任务执行成功,运行时间" + ts.TotalMilliseconds + "毫秒", Common.enLogType.Info, "siluExecute", ip);
                }
                catch (Exception ex)
                {
                    logbll.InsertLog("收益定时任务执行失败：异常：" + ex.ToString(), Common.enLogType.Info, "siluExecute", ip);
                }

            }
            //try
            //{
            //    UsersBLL bll = new UsersBLL();
            //    bll.CommunityRewardsTask(DateTime.Now);
            //}
            //catch (Exception ex)
            //{
            //    logbll.InsertLog("小区奖励执行失败：异常：" + ex.ToString(), Common.enLogType.Info, "siluExecute", ip);
            //}
        }
    }
}
