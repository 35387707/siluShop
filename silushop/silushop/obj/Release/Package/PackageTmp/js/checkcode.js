function GetImgCode(c) {
    $(c).attr("src", "/Safe/GetVerifyCode?type=1&rd=" + Math.random());
}
function daojishi(c) {
    var n = $(c).attr("now");
    if (n > 0) {
        $(c).val(n + "s后获取验证码");
        $(c).text(n + "s后获取验证码");
        $(c).attr("now", n - 1);
        $(c).attr("lastbg", $(c).css("background-color"));
        $(c).css("background-color", "gray");
    }
    else {
        $(c).removeAttr("disabled");
        $(c).val("重新获取验证码");
        $(c).text("重新获取验证码");
        $(c).css("background-color", $(c).attr("lastbg"));
        clearInterval(interdjs);
    }
}
var interdjs;
function GetSMSCode(dom, phone) {
    if (phone.length != 11 || phone[0] != "1" || isNaN(parseInt(phone))) {
        alert("请先填写正确的手机号码");
        return;
    }
    $(dom).attr("disabled", "disabled");
    $.ajax({
        //提交数据的类型 POST GET
        type: "GET",
        //提交的网址
        url: "/Safe/GetVerifyCode",
        //提交的数据
        data: "type=2&rec=" + phone + "&rd=" + Math.random(),
        //返回数据的格式
        datatype: "json",//"xml", "html", "script", "json", "jsonp", "text".
        //在请求之前调用的函数
        beforeSend: function () { },
        //成功返回之后调用的函数             
        success: function (data) {
            if (data == "1") {
                $(dom).attr("now", "60");
                interdjs = setInterval(daojishi, 1000, dom);
            }
            else {
                alert('短信获取失败！请重新点击获取。');
                $(dom).removeAttr("disabled");
            }
        },
        //调用执行后调用的函数
        complete: function (XMLHttpRequest, textStatus) {
            //alert(textStatus);
        },
        //调用出错执行的函数
        error: function () {
            //请求出错处理
        }
    });
}
function copyValue(v) {
    window.clipboardData.setData("Text", v);
    alert("复制成功!");
}