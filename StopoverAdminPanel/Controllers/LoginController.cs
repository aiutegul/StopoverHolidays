using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using StopoverAdminPanel.Auth;

namespace StopoverAdminPanel.Controllers
{
	public class LoginController : Controller
	{
		private readonly HttpClient _client;

		public LoginController(HttpClient client)
		{
			_client = client;
			_client.BaseAddress = new Uri(System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority));
		}

		// GET: Login
		public ActionResult Login(LoginModel model)
		{
			return View();
		}

		[HttpPost]
		public ActionResult Login(LoginModel model, string returnUrl)
		{
			HttpContent content = new FormUrlEncodedContent(new[]
			{
					new KeyValuePair<string, string>("grant_type", "password"),
					new KeyValuePair<string, string>("username", model.Username),
					new KeyValuePair<string, string>("password", model.Password)
				});

			var result = _client.PostAsync("/api/Account/login", content).Result;
			if (result.IsSuccessStatusCode)
			{
				string resultContent = result.Content.ReadAsStringAsync().Result;

				var response = JsonConvert.DeserializeObject<Token>(resultContent);

				AuthenticationProperties options = new AuthenticationProperties
				{
					AllowRefresh = true,
					IsPersistent = true,
					ExpiresUtc = DateTime.UtcNow.AddSeconds(int.Parse(response.expires_in))
				};

				var claims = new[]
				{
					new Claim(ClaimTypes.Name, model.Username),
					new Claim("AccessToken", $"Bearer {response.access_token}"),
				};

				var cookie = new HttpCookie("access_token")
				{
					Value = response.access_token
				};
				Response.Cookies.Add(cookie);
				var identity = new ClaimsIdentity(claims, "ApplicationCookie");
				var responseRoles = JsonConvert.DeserializeObject<List<string>>(response.roles);
				foreach (var role in responseRoles)
				{
					identity.AddClaim(new Claim(ClaimTypes.Role, role));
				}
				Request.GetOwinContext().Authentication.SignIn(options, identity);

				if (Url.IsLocalUrl(returnUrl))
				{
					return Redirect(returnUrl);
				}
				return RedirectToAction("Index", "Main");
			}
			else
			{
				ModelState.AddModelError("", "Login or password are incorrect!");
			}
			return View(model);
		}

		public ActionResult LogOut()
		{
			Request.GetOwinContext().Authentication.SignOut("ApplicationCookie");
			HttpContext.Response.Cookies.Clear();
			HttpContext.Request.Cookies.Clear();
			return RedirectToAction("Login");
		}
	}
}