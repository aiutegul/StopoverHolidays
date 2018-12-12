using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StopoverAdminPanel.Controllers
{
    //[Authorize]

    public class MainController : Controller
    {
        // GET: Main
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult AllOrderStopovers()
        {
            return View();
        }

        public ActionResult AllOrderActivities()
        {
            return View();
        }

        public ActionResult Orders()
        {
            return View();
        }

        public ActionResult Hotels()
        {
            return View();
        }

        public ActionResult Activities()
        {
            return View();
        }

        public ActionResult Passengers()
        {
            return View();
        }

        public ActionResult Flights()
        {
            return View();
        }

        public ActionResult Transfers()
        {
            return View();
        }

        public ActionResult OrderRequests()
        {
            return View();
        }

        public ActionResult OrderStopoverData()
        {
            return View();
        }

        public ActionResult OrderActivityData()
        {
            return View();
        }

    }
}