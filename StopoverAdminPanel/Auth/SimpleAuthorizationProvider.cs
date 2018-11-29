using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.OAuth;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StopoverAdminPanel.Auth
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            using (AuthRepository _repo = new AuthRepository())
            {
                IdentityUser user = await _repo.FindUser(context.UserName, context.Password);

                if (user == null)
                {
                    context.SetError("Invalid grant", "The user name or password is incorrect.");
                    return;
                }
                var rolesOfUser = await _repo.UserRoles(user.Id);
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                string roleOfUser = "";
                foreach (string role in rolesOfUser)
                {
                    roleOfUser = role;
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
                identity.AddClaim(new Claim("sub", context.UserName));

                context.Validated(identity);
            }
        }
    }
}