using Microsoft.AspNet.Identity.EntityFramework;

namespace StopoverAdminPanel.Auth
{
	public class AuthContext : IdentityDbContext<ApplicationUser>
	{
		public AuthContext()
			: base("name=AuthContext")
		{

		}
	}
}