using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCStudy.Controllers
{
    public class ListController : Controller
    {
        // GET: List
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public string GetString()
        {
            return "hello world!";
        }

        public ActionResult GetView()
        {
            return View("ListView");
        }
    }
}