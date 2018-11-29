using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace StopoverAdminPanel.Auth
{
    public class AuthRepository : IDisposable
    {
        private AuthContext _ctx;

        private UserManager<IdentityUser> _userManager;

        public AuthRepository()
        {
            _ctx = new AuthContext();
            _userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));
        }

        public async Task<IdentityResult> RegisterUser(UserModel userModel)
        {
            IdentityUser user = new IdentityUser
            {
                UserName = userModel.UserName
            };

            var result = await _userManager.CreateAsync(user, userModel.Password);
            //var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_ctx));
            //await RoleManager.CreateAsync(new IdentityRole("Agent"));
            //await RoleManager.CreateAsync(new IdentityRole("Admin"));

            if (result.Succeeded)
            {
                var currentuser = _userManager.FindByName(user.UserName);
                //change role
                string role;
                if (userModel.role.Equals("Agent") || userModel.role.Equals("Admin"))
                {
                    role = userModel.role;
                }
                else
                {
                    role = "Agent";
                }
                await _userManager.AddToRolesAsync(currentuser.Id, role);
            }
            return result;
        }

        public async Task<IdentityUser> FindUser(string userName, string password)
        {
            IdentityUser user = await _userManager.FindAsync(userName, password);
            return user;
        }

        public async Task<IdentityUser> FindUserById(string userId)
        {
            IdentityUser user = await _userManager.FindByIdAsync(userId);
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