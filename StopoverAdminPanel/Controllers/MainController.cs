using System.Web.Mvc;

namespace StopoverAdminPanel.Controllers
{
	//[Authorize]

	public class MainController : Controller
	{
		public ActionResult AllOrderStopovers()
		{
			return View();
		}

		public ActionResult AllOrderActivities()
		{
			return View();
		}

		[Authorize(Roles = "Admin, Office")]
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

		[Authorize(Roles = "Admin, User")]
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