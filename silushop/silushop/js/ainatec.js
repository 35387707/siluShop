jQuery(document).ready(function(){ 
	$("#helptab>ul>li").click(function(){
		index = $(this).index();
		$("#helptab li").removeClass("active").eq(index).addClass("active");
		$("#helptab_con ul").removeClass("active").eq(index).addClass("active");
	})
}); 
;