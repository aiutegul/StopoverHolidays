using System.Web.Mvc;

namespace StopoverAdminPanel.Controllers
{
	//[Authorize]

	public class MainController : Controller
	{
		[Authorize]
		public ActionResult Index()
		{
			if (User.IsInRole("User"))
			{
				return RedirectToAction("OrderRequests");
			}
			else if (User.IsInRole("Office"))
			{
				return RedirectToAction("Orders");
			}
			else
			{
				return RedirectToAction("Orders");
			}
		}

		[Authorize(Roles = "Admin, Office")]
		public ActionResult Orders()
		{
			return View();
		}

		[Authorize(Roles = "Admin")]
		public ActionResult Hotels()
		{
			return View();
		}

		[Authorize(Roles = "Admin")]
		public ActionResult Activities()
		{
			return View();
		}

		[Authorize(Roles = "Admin")]
		public ActionResult Transfers()
		{
			return View();
		}

		[Authorize(Roles = "Admin, User")]
		public ActionResult OrderRequests()
		{
			return View();
		}
	}
}