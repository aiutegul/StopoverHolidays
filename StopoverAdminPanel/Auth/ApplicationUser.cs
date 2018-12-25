using Microsoft.AspNet.Identity.EntityFramework;

namespace StopoverAdminPanel.Auth
{
	public class ApplicationUser : IdentityUser
	{
		public int? PartnerId { get; set; }
	}
}