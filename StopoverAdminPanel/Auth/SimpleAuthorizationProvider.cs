using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using StopoverAdminPanel.Audit;

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
				ApplicationUser user = await _repo.FindUser(context.UserName, context.Password);

				if (user == null)
				{
					context.SetError("Invalid grant", "The user name or password is incorrect.");
					return;
				}
				var rolesOfUser = await _repo.UserRoles(user.Id);
				var identity = new ClaimsIdentity(context.Options.AuthenticationType);
				identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
				identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
				foreach (string role in rolesOfUser)
				{
					identity.AddClaim(new Claim(ClaimTypes.Role, role));
				}
				identity.AddClaim(new Claim("sub", context.UserName));
				var props = new AuthenticationProperties(new Dictionary<string, string>
				{
					{
						"roles", JsonConvert.SerializeObject(rolesOfUser)
					},
					{
						"PartnerId", user.PartnerId.ToString()
					}
				});
				var ticket = new AuthenticationTicket(identity, props);
				DbContextAuditable.SetAuditContext(new AuditContext());
				DbContextAuditable.SetUser(user.UserName);
				context.Validated(ticket);
			}
		}

		public override Task TokenEndpoint(OAuthTokenEndpointContext context)
		{
			foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
			{
				context.AdditionalResponseParameters.Add(property.Key, property.Value);
			}

			return Task.FromResult<object>(null);
		}
	}
}