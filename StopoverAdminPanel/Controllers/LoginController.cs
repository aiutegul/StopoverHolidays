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
		// GET: Login
		public ActionResult LoginView()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Login(LoginModel model, string returnUrl)
		{
			var getTokenUrl = "http://" + HttpContext.Request.Url?.Host + ":" + HttpContext.Request.Url?.Port + "/api/Account/login";

			using (HttpClient httpClient = new HttpClient())
			{
			    httpClient.BaseAddress = new Uri(getTokenUrl);
				HttpContent content = new FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string, string>("grant_type", "password"),
					new KeyValuePair<string, string>("username", model.Username),
					new KeyValuePair<string, string>("password", model.Password)
				});

				HttpResponseMessage result = httpClient.PostAsync("", content).Result;

				// TODO if not 200 alert username and password not correct

				string resultContent = result.Content.ReadAsStringAsync().Result;

				var response = JsonConvert.DeserializeObject<Token>(resultContent);

				AuthenticationProperties options = new AuthenticationProperties();

				options.AllowRefresh = true;
				options.IsPersistent = true;
				options.ExpiresUtc = DateTime.UtcNow.AddSeconds(int.Parse(response.expires_in));

				var claims = new[]
				{
					new Claim(ClaimTypes.Name, model.Username),
					new Claim("AccessToken", string.Format("Bearer {0}", response.access_token)),
				};

				HttpCookie cookie = new HttpCookie("access_token");

				cookie.Value = response.access_token;

				// Добавить куки в ответ
				Response.Cookies.Add(cookie);
				var identity = new ClaimsIdentity(claims, "ApplicationCookie");
				var responseRoles = JsonConvert.DeserializeObject<List<string>>(response.roles);
				foreach (var role in responseRoles)
				{
					identity.AddClaim(new Claim(ClaimTypes.Role, role));
				}
				Request.GetOwinContext().Authentication.SignIn(options, identity);
			}

			return RedirectToAction("Orders", "Main");
		}

		public ActionResult LogOut()
		{
			Request.GetOwinContext().Authentication.SignOut("ApplicationCookie");
			//Response.Cookies.Remove("access_token");
			//Request.Cookies.Remove("access_token");
			HttpContext.Response.Cookies.Clear();
			HttpContext.Request.Cookies.Clear();
			return RedirectToAction("LoginView");
		}
	}
}