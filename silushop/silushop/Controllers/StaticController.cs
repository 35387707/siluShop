using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace silushop.Controllers
{
    public class StaticController : Controller
    {
        // GET: Static
        public ActionResult Header()
        {
            return PartialView();
        }
        public ActionResult ShopFooter(int id) {
            return PartialView(id);
        }
        public ActionResult TransforoutResult() {
            return View();
        }
    }
}