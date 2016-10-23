using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RBS.Library;

namespace RBS.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return RedirectToAction("Login", "User");
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}