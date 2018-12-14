using System.Web.Mvc;

namespace StopoverAdminPanel.Controllers
{
	//[Authorize]

	public class MainController : Controller
	{

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