using System;
using System.Collections.Generic;
using System.Linq;
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
            var getTokenUrl = "http://"+HttpContext.Request.Url.Host+":"+ HttpContext.Request.Url.Port+"/api/Account/login";

            using (HttpClient httpClient = new HttpClient())
            {
                HttpContent content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", model.username),
                    new KeyValuePair<string, string>("password", model.password)
                });

                HttpResponseMessage result = httpClient.PostAsync(getTokenUrl, content).Result;

                // TODO if not 200 alert username and password not correct

                string resultContent = result.Content.ReadAsStringAsync().Result;

                var token = JsonConvert.DeserializeObject<Token>(resultContent);

                AuthenticationProperties options = new AuthenticationProperties();

                options.AllowRefresh = true;
                options.IsPersistent = true;
                options.ExpiresUtc = DateTime.UtcNow.AddSeconds(int.Parse(token.expires_in));

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, model.username),
                    new Claim("AccessToken", string.Format("Bearer {0}", token.access_token)),
                };
                HttpCookie cookie = new HttpCookie("access_token");

                cookie.Value = token.access_token;

                // Добавить куки в ответ
                Response.Cookies.Add(cookie);
                var identity = new ClaimsIdentity(claims, "ApplicationCookie");

                Request.GetOwinContext().Authentication.SignIn(options, identity);
                var xs = System.Web.HttpContext.Current.User;

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