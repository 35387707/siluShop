$(document).ready(function(){
    /*首页banner轮播*/
    (function(){
        var index=0;
        var l=$(".bannerlist .banner").length;
        function indexBannerAutoPlay(){
        	index++;
	        if(index>l-1){
	            index=0;
	        };
	        $(".bannerlist .banner").eq(index).fadeIn().siblings(".banner").fadeOut();
	    };
	    setInterval(indexBannerAutoPlay,5000);
    })();
    
    /*订单填写、购物车商品数量点击加减*/
    (function(){
        $(".sc-reduce-btn").on("click", function () {
            var dj = $(this).parent().parent().prev().find(".price-item").text();

        	var obj=$(this).siblings(".sc-txt");
        	var o=parseInt(obj.val());
        	o--;
        	if (o < 1) o = 1;
        	$(this).parent().parent().next().find(".total-item").text((parseFloat(dj)*o).toFixed(2));
        	obj.val(o);
        	ChangeData($(this).parent().parent().parent().attr("id"), o);
        	showTotalPrice();
        });
        $(".sc-add-btn").on("click", function () {
            var dj = $(this).parent().parent().prev().find(".price-item").text();
          
        	var obj=$(this).siblings(".sc-txt");
        	var o=parseInt(obj.val());
        	o++;
        	ChangeData($(this).parent().parent().parent().attr("id"),o);
        	$(this).parent().parent().next().find(".total-item").text((parseFloat(dj) * o).toFixed(2));
        	obj.val(o);
        	showTotalPrice();
        });
    })();
    
    /*订单填写、购物车商品数量点击右侧叉号删除相应订单*/
    
    
    /*支付方式——点击选择支付宝或微信支付*/
    (function(){
        $(".pm-cb").on("click",function(){
            $(this).addClass("active").parents(".pm-item").siblings(".pm-item").find(".pm-cb").removeClass("active");
        });
    })();
    
    /*支付方式——选择支付方式剩余付款时间倒计时*/
    /*
    (function(){
        var countDown=1200;
        var oMinutes=parseInt(countDown/60);
        var oSeconds=parseInt(countDown%60);
        var str=oT(oMinutes)+"分"+oT(oSeconds)+"秒";
        //补零函数
        function oT(n){
            return n<10?"0"+n:n;
        };
        $("#pm-time").html(str);
        function daojishi(){
            oSeconds--;
            if(oSeconds<0){
                oSeconds=59;
                oMinutes--;
                if(oMinutes<0){
                    clearInterval(timer1);
                    timer1=null;
                    alert("您的付款时间已结束！");
                    return;
                };
            };
            str=oT(oMinutes)+"分"+oT(oSeconds)+"秒";
            $("#pm-time").html(str);
        };
        var timer1=setInterval(daojishi,1000);
    })();
    */
    /*个人中心取消订单——点击确认是否取消订单*/
    (function(){
        $(".cancel-order").on("click",function(){
            var o=window.confirm("再次确认：是否取消订单？");
            if(o){
                $(this).parents(".order-item").hide();
            }else{
                return;  
            };
        });
    })();
    
    /*产品列表——点击选择分类*/
    (function(){
        $(".cb-item").on("click",function(){
            $(this).addClass("active").siblings(".cb-item").removeClass("active");
        });
    })();
    
    /*商品详情——点击尺寸显示分类*/
    (function(){
        $(".pd-guige-a").on("click",function(){
	        $(this).find(".pd-guigelist").stop().slideDown(100);
	    });
	    $(".pd-guigelist li").on("click",function(ev){
	        $("#pd-guige-span").html($(this).html()).siblings(".pd-guigelist").hide();
	        ev.stopPropagation();
	    });
    })();
});
