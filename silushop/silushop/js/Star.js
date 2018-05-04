var starnum = 1;
$(function () {
    var $lis = $(".starul li");
    for (var i = 1; i <= $lis.length; i++) {
        $lis[i - 1].index = i;
        $($lis[i - 1]).on("click",function () { //鼠标点击,同时会调用onmouseout,改变tempnum值点亮星星
            tempnum = this.index;
            fnShow(this.index);
        });
    }
});
//var num = finalnum = tempnum = 0;
//var lis = document.getElementsByClassName("starul")[0].getElementsByTagName("li");
//num:传入点亮星星的个数
//finalnum:最终点亮星星的个数
//tempnum:一个中间值
function fnShow(num) {
    starnum = num;
    var $lis = $(".starul li");
    finalnum = num || tempnum;//如果传入的num为0，则finalnum取tempnum的值
    for (var i = 0; i < $lis.length; i++) {
        $lis[i].className = i < finalnum ? "light" : "";//点亮星星就是加class为light的样式
    }
}