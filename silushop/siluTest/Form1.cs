using RelexBarBLL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace siluTest
{
    public partial class Form1 : Form
    {
        Thread thread;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnstart_Click(object sender, EventArgs e)
        {
            thread = new Thread(task);
          
            thread.Start();
            btnstart.Enabled = false;
        }
        public void btnEnabled() {
            btnstart.Invoke(new MethodInvoker(()=>{
                btnstart.Enabled = true;
            }));
        }
        public void addString(string str) {
            listView1.Invoke(new MethodInvoker(()=> {
                listView1.Items.Add(str);
            }));
            
        }
        public void task() {
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
            DateTime endtime = dtpendtime.Value;
            DateTime starttime = DateTime.Parse(DateTime.Now.ToString("yyy-MM-dd"));
            while (starttime <= endtime)
            {
                if (hbll.ToDayIsHolidays(starttime))
                {
                    addString(starttime.ToString("yyyy-MM-dd")+"节假日\r\n");
                }
                else
                {
                    try
                    {
                        DateTime dt1 = System.DateTime.Now;
                        UsersBLL bll = new UsersBLL();
                        bll.TimedTaskRun(starttime);
                        DateTime dt2 = System.DateTime.Now;
                        TimeSpan ts = dt2.Subtract(dt1);
                        addString(starttime.ToString("yyyy-MM-dd")+"收益定时任务执行成功,运行时间" + ts.TotalMilliseconds + "毫秒\r\n");
                        logbll.InsertLog("收益定时任务执行成功,运行时间" + ts.TotalMilliseconds + "毫秒", Common.enLogType.Info, "siluExecute", ip);
                    }
                    catch (Exception ex)
                    {
                        addString(starttime.ToString("yyyy-MM-dd")+"收益定时任务执行失败：异常：" + ex.ToString() + "\r\n");
                        logbll.InsertLog("收益定时任务执行失败：异常：" + ex.ToString(), Common.enLogType.Info, "siluExecute", ip);
                    }

                }

                starttime = starttime.AddDays(1);
            }
            MessageBox.Show("执行成功");
            btnEnabled();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
