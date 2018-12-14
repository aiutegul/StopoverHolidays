using System.Threading.Tasks;
using System.Web.Mvc;
using StopoverAdminPanel.UserModels;

namespace StopoverAdminPanel.Controllers
{
	public class UsersController : Controller
	{
		public async Task<ActionResult> Register(RegisterUserModel model)
		{
			if (ModelState.IsValid)
			{
			}
			return View(model);
		}
	}
}