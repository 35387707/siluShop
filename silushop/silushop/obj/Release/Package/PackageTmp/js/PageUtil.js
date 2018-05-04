(function ($) {
    var ms = {
        init: function (totalsubpageTmep, args) {
            return (function () {
                ms.fillHtml(totalsubpageTmep, args);
               ms.bindEvent(totalsubpageTmep, args);
            })();
        },
        //填充html
        fillHtml: function (totalsubpageTmep, args) {
            return (function () {
                totalsubpageTmep = "";
                //加入上一页
                if (args.currPage != 1 && args.totalPage>0)
                    totalsubpageTmep += "<li class='ali'><a href='javascript:void(0);' onclick='toPage(" + (args.currPage - 1) + ")' class='geraltTb_pager' data-go='" + (args.currPage - 1) + "' >上一页</a></li>";

                // 页码大于等于4的时候，添加第一个页码元素
                if (args.currPage != 1 && args.currPage >= 4 && args.totalPage != 4) {
                    totalsubpageTmep += "<li class='ali'><a href='javascript:void(0);' class='geraltTb_pager' data-go='1' >" + 1 + "</a></li>";
                }
                /* 当前页码>4, 并且<=总页码，总页码>5，添加“···”*/
                if (args.currPage - 2 > 2 && args.currPage <= args.totalPage && args.totalPage > 5) {
                    totalsubpageTmep += "<li class='ali'><a href='javascript:void(0);' class='geraltTb_' data-go='' >...</a></li>";
                }
                /* 当前页码的前两页 */
                var start = args.currPage - 2;
                /* 当前页码的后两页 */
                var end = args.currPage + 2;

                if ((start > 1 && args.currPage < 4) || args.currPage == 1) {
                    end++;
                }
                if (args.currPage > args.totalPage - 4 && args.currPage >= args.totalPage) {
                    start--;
                }
                for (; start <= end; start++) {
					if(start==args.currPage){
					    totalsubpageTmep += "<li class='ali'><a href='javascript:void(0);' style='color:white;background-color:#337ab7' class='geraltTb_pager' data-go='" + start + "' >" + start + "</a></li>";
					}
                    else if (start <= args.totalPage && start >= 1) {
                        totalsubpageTmep += "<li class='ali'><a href='javascript:void(0);' onclick='toPage(" + start + ")' class='geraltTb_pager' data-go='" + start + "' >" + start + "</a></li>";
                    }
                }
                if (args.currPage + 2 < args.totalPage - 1 && args.currPage >= 1 && args.totalPage > 5) {
                    totalsubpageTmep += "<li class='ali'><a href='javascript:void(0);' class='geraltTb_' data-go='' >...</a></li>";
                }

                if (args.currPage != args.totalPage && args.currPage < args.totalPage - 2 && args.totalPage != 4) {
                    totalsubpageTmep += "<li class='ali'><a href='javascript:void(0);' class='geraltTb_pager' data-go='" + args.totalPage + "' >" + args.totalPage + "</a></li>";
                }

                //加入下一页
                if (args.currPage != args.totalPage)
                    totalsubpageTmep += "<li class='ali'><a href='javascript:void(0);' onclick='toPage(" + (args.currPage + 1) + ")' class='geraltTb_pager' data-go='" + (args.currPage + 1) + "'  >下一页</a></li>";

                $(".pagination").html(totalsubpageTmep);
            })();
        },
        //绑定事件
        bindEvent: function (totalsubpageTmep, args) {
            return (function () {
                totalsubpageTmep.on("click", "a.geraltTb_pager", function (event) {
                    //var current = parseInt($(this).text());
                    var current = parseInt($(this).attr('data-go'));
                    ms.fillHtml(totalsubpageTmep, { "currPage": current, "totalPage": args.totalPage, "turndown": args.turndown });
                    //if (typeof (args.backFn) == "function") {
                    //    args.backFn(current);
                    //}
                });
            })();
        }
    }
    $.fn.createPage = function (options) {
        ms.init(this, options);
    }
})(jQuery);