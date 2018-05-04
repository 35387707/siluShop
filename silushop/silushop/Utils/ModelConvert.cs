using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace silushop.Utils
{
    public static class ModelConvert
    {
        public static string split(this string str,int start,int count) {
            if (str.Length>count) {
                return str.Substring(start, count);
            }else
            {
                return str;
            }
        }
        public static string toLocation(this RelexBarDLL.AdsList ads)
        {
            switch (ads.Location)
            {
                case 0:
                    return "丝路联盟首页轮播";
                case 1:
                    return "丝路商城首页轮播";
                case 2:
                    return "首页body";
                default:
                    return "";
            }
        }
        public static string toOrderType(this RelexBarBLL.Models.OrderListModel order)
        {
            switch (order.OrderType)
            {
                case 1:
                    return "快递配送";
                case 2:
                    return "门店收货";
                case 3:
                    return "门店自提";
            }
            return "";
        }
        public static string toOrderStatus(this RelexBarBLL.Models.OrderListModel order)
        {
            switch (order.Status)
            {
                case -1:
                    return "已取消";
                case 0:
                    return "待付款";
                case 1:
                    return "已支付";
                case 2:
                    return "已发货";
                case 3:
                    return "已收货";
                case 4:
                    return "已完成订单";
                case 5:
                    return "退货中";
            }
            return "";
        }
    }
}