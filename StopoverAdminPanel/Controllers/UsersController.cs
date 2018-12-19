using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;
using StopoverAdminPanel.Auth;
using StopoverAdminPanel.UserModels;

namespace StopoverAdminPanel.Controllers
{
	[Authorize(Roles = "Admin")]
	public class UsersController : Controller
	{
		private readonly HttpClient _client;

		public UsersController(HttpClient client)
		{
			_client = client;
			_client.BaseAddress = new Uri(System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority));
		}

		public async Task<ActionResult> UserList()
		{
			var result = await _client.GetAsync("api/Account/Users");
			if (result.IsSuccessStatusCode)
			{
				var users = await result.Content.ReadAsStringAsync();
				return View(JsonConvert.DeserializeObject<List<FormattedUser>>(users));
			}
			return View();
		}

		[Authorize(Roles = "Admin")]
		public async Task<ActionResult> EditUser(string userid)
		{
			if (userid == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			var result = await _client.GetAsync("api/Account/");
			var user = JsonConvert.DeserializeObject<FormattedUser>(await result.Content.ReadAsStringAsync());
			var rolesAndPartners = await GetRolesAndPartnersList();
			var partners = rolesAndPartners.Partners;
			var allroles = rolesAndPartners.Roles;
			var rolesResult = await _client.GetAsync("api/Account/" + userid + "/roles");
			if (rolesResult.IsSuccessStatusCode)
			{
				var role = JsonConvert.DeserializeObject<List<string>>(await rolesResult.Content.ReadAsStringAsync())[0];
				foreach (var r in allroles)
				{
					if (role.Equals(r.Value))
					{
						r.Selected = true;
					}
				}
			}

			foreach (var p in partners)
			{
				if (user.PartnerId.ToString().Equals(p.Value))
				{
					p.Selected = true;
				}
			}
			user.Roles = allroles;
			user.Partners = partners;
			return View(user);
		}

		[Authorize(Roles = "Admin")]
		public async Task<ActionResult> DeleteUser(string userid)
		{
			if (userid == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			var result = await _client.DeleteAsync("api/Account/" + userid);
			if (result.IsSuccessStatusCode)
			{
				return RedirectToAction("UserList");
			}
			return Content("Can't delete");
		}

		public async Task<ActionResult> Register()
		{
			var model = new RegisterUserModel();
			var rolesAndPartners = await GetRolesAndPartnersList();
			if (rolesAndPartners != null)
			{
				model.roles = rolesAndPartners.Roles;
				model.partners = rolesAndPartners.Partners;
				return View(model);
			}

			return new HttpStatusCodeResult(400, "Service for registration not available, try again later");
		}

		[HttpPost]
		public async Task<ActionResult> Register(RegisterUserModel model)
		{
			if (ModelState.IsValid)
			{
				var usrModel = new UserModel
				{
					UserName = model.UserName,
					PartnerId = model.PartnerId,
					Role = model.role
				};
				var content = new StringContent(JsonConvert.SerializeObject(usrModel), Encoding.UTF8, "application/json");
				var result = await _client.PostAsync("api/Account/Register", content);
				if (result.IsSuccessStatusCode)
				{
					return RedirectToAction("Register");
				}

				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, model.UserName + " not registred");
			}

			var rolesAndPartners = await GetRolesAndPartnersList();
			model.roles = rolesAndPartners.Roles;
			model.partners = rolesAndPartners.Partners;
			return View(model);
		}

		[NonAction]
		[Authorize(Roles = "Admin")]
		public async Task<SelectLists> GetRolesAndPartnersList()
		{
			var result = await _client.GetAsync("api/Account/GetPartnersAndRoles");

			if (result.IsSuccessStatusCode)
			{
				var resultContent = await result.Content.ReadAsStringAsync();
				var des = JsonConvert.DeserializeObject<PartnersListModel>(resultContent);
				var partnersList = new List<SelectListItem>();
				foreach (var element in des.partners)
				{
					partnersList.Add(new SelectListItem
					{
						Value = element.Id.ToString(),
						Text = element.Code,
					});
				}

				var rolesList = des.roles
					.Select(x => new SelectListItem { Text = x, Value = x })
					.ToList();
				return new SelectLists { Roles = rolesList, Partners = partnersList };
			}

			return null;
		}

		protected override void Initialize(RequestContext requestContext)
		{
			base.Initialize(requestContext);
			if (requestContext.HttpContext.User.Identity.IsAuthenticated)
			{
				var identity = (ClaimsIdentity)User.Identity;
				IEnumerable<Claim> claims = identity?.Claims;
				var token = claims?.FirstOrDefault(t => t.Type.Equals("AccessToken"));
				_client.DefaultRequestHeaders.Add("Authorization", token?.Value);
			}
		}
	}
}