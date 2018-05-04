$(document).ready(function(){
    /*头部手机端视口设置*/
    (function(){
        var i = window.devicePixelRatio>1?1/window.devicePixelRatio:1;
	    var meta =document.createElement("meta");
	    meta.name="viewport";
	    meta.content='width=device-width,user-scalable=no,initial-scale='+i+',minimum-scale='+i+',maximum-scale='+i;
	    document.getElementsByTagName("head")[0].appendChild(meta);
	    
	    var html = document.getElementsByTagName("html")[0];
	    var iW =document.body.offsetWidth;
	    var scale=iW/750*20;
	    html.style.fontSize=scale+"px";
    })();
    
    /*添加地址——点击选择性别 男士或者女士*/
    (function(){
        $(".ad-msg-cb").on("click",function(){
            $(this).find(".ad-msg-cbicon").addClass("checked").parents(".ad-msg-cb").siblings(".ad-msg-cb").find(".ad-msg-cbicon").removeClass("checked");
        });
    })();
    
    /*确认订单——数量加减*/
    (function(){
        $(".bi-reduce").on("click",function(){
            var o=parseInt($(this).siblings(".bi-num-txt").val());
            o--;
            if(o<1){
                o=1;
            };
            $(this).siblings(".bi-num-txt").val(o);
        });
        $(".bi-add").on("click",function(){
            var o=parseInt($(this).siblings(".bi-num-txt").val());
            o++;
            $(this).siblings(".bi-num-txt").val(o);
        });
    })();
    
    /*所有蛋糕——点击切换分类和口味*/
    (function(){
        $(".cb-item").on("click",function(){
            $(this).addClass("active").siblings(".cb-item").removeClass("active");
        });
    })();
    
    /*购买须知——点击切换*/
    (function(){
        $(".pN-item").on("click",function(){
            $(this).addClass("active").siblings(".pN-item").removeClass("active");
        });
    })();
    
    /*产品详情——点击尺寸显示下拉菜单*/
    (function(){
        $(".pd-guige-item").on("click",function(){
            $(this).find(".pd-guigelist").stop().slideDown(200);
        });
        $(".pd-guigelist li").on("click",function(ev){
            $(this).parents(".pd-guige-a").find(".pd-guige-span").html($(this).html()).siblings(".pd-guigelist").hide();
            ev.stopPropagation();
        });
    })();
    
    /*全屏滑屏调用*/
    //startSwipe(第一个参数：图片个数/显示小点的个数，第二个参数：滑屏最顶层的盒子，第三个参数：显示分页小点的盒子)；
    //startSwipe(5,"mySwipe","pager-index");
    //startSwipe(3,"mySwipe-pd","pager-pd");
    
 
    
	
	
	/*选择支付方式*/
	(function(){
	    $(".pm-item").on("click",function(){
	        $(this).find(".uncheckedicon").addClass("active").parents(".pm-item").siblings(".pm-item").find(".uncheckedicon").removeClass("active");
	    });
	})();
	
});

function startSwipe(length, id1, id2) {
    /*1.定义全局变量，用于对应下标*/
    var index = 0;

    /*2.轮播至少3张图片以上*/
    var mySwipe = Swipe(document.getElementById(id1), {
        auto: 6000,
        callback: function (index, element) {
            index = mySwipe.getPos();
            slideTab(index);
        }
    });

    /*3.分页的盒子存在的话，执行以下代码，否则直接return，防止某些页面出现获取元素为undefined的情况*/
    var pager = document.getElementById(id2);
    if (!pager) return;

    /*4.根据传进来的长度参数，动态循环出分页显示的li*/
    for (var i = 0; i < length; i++) {
        var bullet = document.createElement("li");
        bullet.className = "point";
        if (i === 0) {
            addClassName({ ele: bullet, nameArr: ["active"] });
        };
        bullet.setAttribute("data-tab", i);
        pager.appendChild(bullet);
    };

    /*5.获取所有生成的小点集合*/
    var bullets = pager.querySelectorAll("li");

    function slideTab(index) {
        var o = bullets.length;
        while (o--) {
            bullets[o].className = bullets[o].className.replace("active", " ");
        };
        bullets[index].className = "active";
    };

    /*添加某个元素class名字的函数封装*/
    function addClassName(int) {
        if (int.ele) {
            if (!int.ele.className) {
                int.ele.className = int.nameArr.join(" ");
            } else {
                var arr = int.ele.className.split(" ");
                int.ele.className = arr.concat(int.nameArr).join(" ");
            };
        };
    };
};