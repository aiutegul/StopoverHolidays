using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

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

		public async Task<IdentityResult> RegisterUser(UserModel userModel)
		{
			ApplicationUser user = new ApplicationUser
			{
				UserName = userModel.UserName
			};

			var result = await _userManager.CreateAsync(user, userModel.Password);

			if (result.Succeeded)
			{
				var currentuser = _userManager.FindByName(user.UserName);
				string role = userModel.Role;
				await _userManager.AddToRolesAsync(currentuser.Id, role);
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