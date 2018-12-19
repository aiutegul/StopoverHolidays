﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;

namespace StopoverAdminPanel.Auth
{
	public class AuthRepository : IDisposable
	{
		private readonly AuthContext _ctx;
		private readonly UserManager<ApplicationUser> _userManager;

		public AuthRepository()
		{
			_ctx = new AuthContext();
			_userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_ctx));
		}

		public async Task InitializeRoles()
		{
			var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_ctx));
			if (!await roleManager.RoleExistsAsync("User"))
			{
				await roleManager.CreateAsync(new IdentityRole("User"));
			}

			if (!await roleManager.RoleExistsAsync("Admin"))
			{
				await roleManager.CreateAsync(new IdentityRole("Admin"));
			}

			if (!await roleManager.RoleExistsAsync("Office"))
			{
				await roleManager.CreateAsync(new IdentityRole("Office"));
			}
		}

		public List<string> GetListRoles()
		{
			var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_ctx));
			return roleManager.Roles.Select(x => x.Name).ToList();
		}

		public List<FormattedUser> GetUsers()
		{
			var formatterUsers = _userManager.Users.Select(u => new FormattedUser
			{
				Id = u.Id,
				UserName = u.UserName,
				PartnerId = u.PartnerId
			}).ToList();
			formatterUsers.ForEach(user => { user.Role = _userManager.GetRoles(user.Id).ToList()[0]; });
			return formatterUsers;
		}

		public async Task<IdentityResult> DeleteUser(string userid)
		{
			var usr = await _userManager.FindByIdAsync(userid);
			if (usr != null)
			{
				var result = await _userManager.DeleteAsync(usr);
				if (result.Succeeded)
				{
					return result;
				}
			}

			return null;
		}

		public async Task<IdentityResult> RegisterUser(UserModel userModel)
		{
			ApplicationUser user = new ApplicationUser
			{
				UserName = userModel.UserName,
				Email = userModel.UserName,
				PartnerId = userModel.PartnerId
			};
			string generatedPassword = Membership.GeneratePassword(10, 0);
			var result = await _userManager.CreateAsync(user, generatedPassword);

			if (result.Succeeded)
			{
				var currentuser = _userManager.FindByName(user.UserName);
				string role = userModel.Role;
				var rolesResult = await _userManager.AddToRolesAsync(currentuser.Id, role);
				if (rolesResult.Succeeded)
				{
					using (HttpClient client = new HttpClient())
					{
						client.BaseAddress = new Uri(ConfigurationManager.AppSettings["emailApiAddr"]);
						var destination = new EmailDestination
						{
							Addresses = new List<Addresses> { new Addresses { Address = user.Email } }
						};
						var message = new Messages { Text = "Your StopoverAdminPanel password: " + "<b>" + generatedPassword + "</b>" };
						var model = new EmailModel
						{
							Destinations = new List<EmailDestination> { destination },
							Messages = new List<Messages> { message }
						};
						var body = JsonConvert.SerializeObject(model);
						var content = new StringContent(body, Encoding.UTF8, "application/json");
						var emailResult = await client.PostAsync("", content);
						if (emailResult.IsSuccessStatusCode)
						{
						}
						else
						{
							// not sent
						}
					}
				}
				else
				{
					return rolesResult;
				}
			}
			return result;
		}

		public async Task<ApplicationUser> FindUser(string userName, string password)
		{
			ApplicationUser user = await _userManager.FindAsync(userName, password);
			return user;
		}

		public async Task<ApplicationUser> FindUserById(string userId)
		{
			ApplicationUser user = await _userManager.FindByIdAsync(userId);
			return user;
		}

		public async Task<IList<string>> UserRoles(string userId)
		{
			IList<string> roles = await _userManager.GetRolesAsync(userId);

			return roles;
		}

		public void Dispose()
		{
			_ctx.Dispose();
			_userManager.Dispose();
		}
	}
}